using KacharaManagement.Business.Interfaces;
using KacharaManagement.Repository.Interfaces;
using KacharaManagement.Core.Entities;
using KacharaManagement.Core;
using Microsoft.AspNetCore.Mvc;

namespace KacharaManagement.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class GothamController : ControllerBase
    {
        private readonly IGothamService _service;
        private readonly ILogEntryRepository _logRepo;
        private readonly string _apiKey;

        public GothamController(IGothamService service, ILogEntryRepository logRepo, IConfiguration config)
        {
            _service = service;
            _logRepo = logRepo;
            _apiKey = config.GetValue<string>("ApiKey") ?? "";
        }
        [HttpPost("log")]
        public async Task<IActionResult> Log([FromBody] LogEntry entry)
        {
            if (entry == null)
                return BadRequest();
            await _logRepo.AddAsync(entry);
            return Ok();
        }

        [HttpGet("hello")]
        public IActionResult Hello() => Ok(new HelloResponse());

        [HttpGet("update")]
        public async Task<IActionResult> Update([FromQuery] UpdateRequest request)                       
        {
            try
            {
                Console.WriteLine($"/api/update called. Request: {System.Text.Json.JsonSerializer.Serialize(request)}");

                await _logRepo.AddAsync(new LogEntry
                {
                    Level = "Info",
                    Message = "Update request received",
                    RequestBody = System.Text.Json.JsonSerializer.Serialize(request),
                    Source = "Update"
                });
                if (request.Key != _apiKey)
                {
                    await _logRepo.AddAsync(new LogEntry
                    {
                        Level = "Warning",
                        Message = "Invalid API key",
                        RequestBody = System.Text.Json.JsonSerializer.Serialize(request),
                        Source = "Update"
                    });
                    return Unauthorized(new { status = "ERROR", msg = "Invalid key" });
                }

                // Use the states as received (UPPERCASE from Arduino)
                string s1 = request.S1 ?? string.Empty;
                string s2 = request.S2 ?? string.Empty;
                string s3 = request.S3 ?? string.Empty;

                // Alert logic: s1 == "FULL" or s3 == "FLOOD!" (case-insensitive)
                bool alert = s1.Equals("FULL", StringComparison.OrdinalIgnoreCase)
                    || s3.Equals("FLOOD!", StringComparison.OrdinalIgnoreCase);

                // needsTruck logic: s1 != "EMPTY" && s2 != "BRIGHT" && s3 != "DRY" (case-insensitive)
                bool needsTruck =
                    !s1.Equals("EMPTY", StringComparison.OrdinalIgnoreCase)
                    && !s2.Equals("BRIGHT", StringComparison.OrdinalIgnoreCase)
                    && !s3.Equals("DRY", StringComparison.OrdinalIgnoreCase);

                // Set alert flag in request if not already set
                if (request.Al == 0 && alert)
                {
                    request.Al = 1;
                }

                var entity = new SensorHistory
                {
                    Bin1Fill = request.B1,
                    Bin1State = s1,
                    Bin2Light = request.B2,
                    Bin2State = s2,
                    Bin3Water = request.B3,
                    Bin3State = s3,
                    Alert = request.Al,
                    NeedsTruck = needsTruck,
                    Source = "Arduino",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                };

                await _logRepo.AddAsync(new LogEntry
                {
                    Level = "Info",
                    Message = "Attempting to insert SensorHistory",
                    RequestBody = System.Text.Json.JsonSerializer.Serialize(entity),
                    Source = "Update"
                });

                await _service.AddSensorHistoryAsync(entity);

                var resp = new
                {
                    status = "OK",
                    alert = alert,
                    needsTruck = needsTruck,
                };
                await _logRepo.AddAsync(new LogEntry
                {
                    Level = "Info",
                    Message = "Update response sent",
                    ResponseBody = System.Text.Json.JsonSerializer.Serialize(resp),
                    Source = "Update"
                });
                return Ok(resp);
            }
            catch (Exception ex)
            {
                await _logRepo.AddAsync(new LogEntry
                {
                    Level = "Error",
                    Message = ex.Message,
                    Exception = ex.ToString(),
                    Source = "Update"
                });
                return StatusCode(500, new ErrorResponse { Msg = "Internal server error" });
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> Status()
        {
            try
            {
                var latest = await _service.GetLatestStatusAsync();
                if (latest == null)
                {
                    await _logRepo.AddAsync(new LogEntry
                    {
                        Level = "Warning",
                        Message = "No data for status",
                        Source = "Status"
                    });
                    return NotFound(new ErrorResponse { Msg = "No data" });
                }


                // Use the states as stored (UPPERCASE from Arduino)
                string bin1State = latest.Bin1State ?? string.Empty;
                string bin2State = latest.Bin2State ?? string.Empty;
                string bin3State = latest.Bin3State ?? string.Empty;

                // LED color logic (case-insensitive for safety)
                string bin1Led = bin1State.Equals("FULL", StringComparison.OrdinalIgnoreCase) ? "red"
                    : bin1State.Equals("HALF", StringComparison.OrdinalIgnoreCase) ? "yellow"
                    : bin1State.Equals("EMPTY", StringComparison.OrdinalIgnoreCase) ? "green" : "off";

                string bin2Led = bin2State.Equals("DARK", StringComparison.OrdinalIgnoreCase) ? "red"
                    : bin2State.Equals("DIM", StringComparison.OrdinalIgnoreCase) ? "yellow"
                    : bin2State.Equals("BRIGHT", StringComparison.OrdinalIgnoreCase) ? "green" : "off";

                string bin3Led = bin3State.Equals("FLOOD!", StringComparison.OrdinalIgnoreCase) ? "red"
                    : bin3State.Equals("DAMP", StringComparison.OrdinalIgnoreCase) ? "yellow"
                    : bin3State.Equals("DRY", StringComparison.OrdinalIgnoreCase) ? "green" : "off";

                // needsTruck logic: s1 != "EMPTY" && s2 != "BRIGHT" && s3 != "DRY"
                bool needsTruck =
                    !bin1State.Equals("EMPTY", StringComparison.OrdinalIgnoreCase)
                    && !bin2State.Equals("BRIGHT", StringComparison.OrdinalIgnoreCase)
                    && !bin3State.Equals("DRY", StringComparison.OrdinalIgnoreCase);

                var resp = new
                {
                    bin1 = new { fill = latest.Bin1Fill, state = latest.Bin1State, led = bin1Led },
                    bin2 = new { light = latest.Bin2Light, state = latest.Bin2State, led = bin2Led },
                    bin3 = new { water = latest.Bin3Water, state = latest.Bin3State, led = bin3Led },
                    alert = latest.Alert == 1,
                    needsTruck = needsTruck,
                    timestamp = latest.CreatedAt.ToString("o")
                };
                await _logRepo.AddAsync(new LogEntry
                {
                    Level = "Info",
                    Message = "Status response sent",
                    ResponseBody = System.Text.Json.JsonSerializer.Serialize(resp),
                    Source = "Status"
                });
                return Ok(resp);
            }
            catch (Exception ex)
            {
                await _logRepo.AddAsync(new LogEntry
                {
                    Level = "Error",
                    Message = ex.Message,
                    Exception = ex.ToString(),
                    Source = "Status"
                });
                return StatusCode(500, new ErrorResponse { Msg = "Internal server error" });
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> History([FromQuery] int limit = 50)
        {
            try
            {
                var data = await _service.GetHistoryAsync(Math.Min(limit, 100));
                var resp = new HistoryResponse
                {
                    Count = data.Count,
                    Data = data.Select(x => new HistoryItem
                    {
                        Id = x.Id,
                        Bin1Fill = x.Bin1Fill,
                        Bin1State = x.Bin1State,
                        Bin2Light = x.Bin2Light,
                        Bin2State = x.Bin2State,
                        Bin3Water = x.Bin3Water,
                        Bin3State = x.Bin3State,
                        Alert = x.Alert,
                        Timestamp = x.CreatedAt.ToString("o")
                    }).ToList()
                };
                await _logRepo.AddAsync(new LogEntry
                {
                    Level = "Info",
                    Message = "History response sent",
                    ResponseBody = System.Text.Json.JsonSerializer.Serialize(resp),
                    Source = "History"
                });
                return Ok(resp);
            }
            catch (Exception ex)
            {
                await _logRepo.AddAsync(new LogEntry
                {
                    Level = "Error",
                    Message = ex.Message,
                    Exception = ex.ToString(),
                    Source = "History"
                });
                return StatusCode(500, new ErrorResponse { Msg = "Internal server error" });
            }
        }
    }
}
