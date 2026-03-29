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

                string s1 = request.S1?.ToLowerInvariant() ?? string.Empty;
                string s2 = request.S2?.ToLowerInvariant() ?? string.Empty;
                string s3 = request.S3?.ToLowerInvariant() ?? string.Empty;

                if(request.Al==0)
                {
                    // Alert logic (compare with lowercase)
                    if (s1 == "full" || s2 == "dark" || s3 == "wet")
                    {
                        request.Al = 1;
                    }
                }

                bool alert = request.Al == 1;

                bool needsTruck = false;// needsTruck is true if any bin needs attention

                if((s1 != "empty" && s2 != "bright" && s3 != "dry" )|| alert == true)
                {
                    needsTruck = true;
                }

                if(needsTruck != true)
                {
                    if(s1 != "empty" || s2 != "bright" || s3 != "dry")
                    {
                        needsTruck = true;
                    }
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
                    NeedsTruck = needsTruck, // Save OR logic to DB
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

                // LED color logic (case-insensitive)
                string bin1State = latest.Bin1State?.ToLowerInvariant() ?? string.Empty;
                string bin2State = latest.Bin2State?.ToLowerInvariant() ?? string.Empty;
                string bin3State = latest.Bin3State?.ToLowerInvariant() ?? string.Empty;

                string bin1Led = latest.Bin1Fill >= 90 ? "red" : latest.Bin1Fill >= 70 ? "yellow" : "green";
                string bin2Led = bin2State == "dark" ? "red" : bin2State == "dim" ? "yellow" : "green";
                string bin3Led = bin3State == "flood!" ? "red" : bin3State == "damp" ? "yellow" : "green";

                bool needsTruck = bin1State != "empty" && bin2State != "bright" && bin3State != "dry";

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
