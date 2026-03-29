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
               
                if(request.Al==0)
                {
                    // Alert logic
                    if (request.S1 == "FULL" || request.S2 == "DARK" || request.S3 == "WET")
                    {
                        request.Al = 1;
                    }
                
                }
                bool alert = request.Al == 1;

                bool needsTruck = false;// needsTruck is true if any bin needs attention

                if((request.S1 != "EMPTY" && request.S2 != "BRIGHT" && request.S3 != "DRY" )|| alert == true)
                {
                    needsTruck = true;
                }

                if(needsTruck != true)
                {
                  if(request.S1 != "EMPTY" || request.S2 != "BRIGHT" || request.S3 != "DRY")
                  {
                    needsTruck = true;
                  }
                }

                var entity = new SensorHistory
                {
                    Bin1Fill = request.B1,
                    Bin1State = request.S1,
                    Bin2Light = request.B2,
                    Bin2State = request.S2,
                    Bin3Water = request.B3,
                    Bin3State = request.S3,
                    Alert = request.Al,
                    NeedsTruck = needsTruck, // Save OR logic to DB
                    CreatedAt = DateTime.UtcNow
                };
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
                var resp = new StatusResponse
                {
                    Bin1 = new BinData { Fill = latest.Bin1Fill, State = latest.Bin1State, Led = latest.Bin1Fill >= 90 ? "red" : latest.Bin1Fill >= 50 ? "yellow" : "green" },
                    Bin2 = new BinData { Light = latest.Bin2Light, State = latest.Bin2State, Led = latest.Bin2Light < 300 ? "yellow" : "green" },
                    Bin3 = new BinData { Water = latest.Bin3Water, State = latest.Bin3State, Led = latest.Bin3State == "FLOOD!" ? "red" : latest.Bin3State == "DAMP" ? "yellow" : "green" },
                    Alert = latest.Alert == 1,
                    NeedsTruck = latest.NeedsTruck,
                    Timestamp = latest.CreatedAt.ToString("o")
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
