using KacharaManagement.Business.Interfaces;
using KacharaManagement.Repository.Interfaces;
using KacharaManagement.Core.Entities;
using KacharaManagement.Core;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace KacharaManagement.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class GothamController : ControllerBase
    {
        private readonly IGothamService _service;
        private readonly ILogEntryRepository _logRepo;
        private readonly string _apiKey;
        private static readonly object TruckMovementLock = new();
        private static TruckMovementResponse TruckMovementState = new();

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
            var stopwatch = Stopwatch.StartNew();
            try
            {
                if (request.Key != _apiKey)
                {
                    _ = _logRepo.AddAsync(new LogEntry
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

                // Alert logic: Bin1 FULL, Bin2 BRIGHT, or Bin3 DAMP/FLOOD! all trigger alert
                bool isFull = s1.Equals("FULL", StringComparison.OrdinalIgnoreCase);
                bool isBright = s2.Equals("BRIGHT", StringComparison.OrdinalIgnoreCase);
                bool isDamp = s3.Equals("DAMP", StringComparison.OrdinalIgnoreCase);
                bool isFlood = s3.Equals("FLOOD!", StringComparison.OrdinalIgnoreCase);

                bool alert = isFull || isBright || isDamp || isFlood;
                bool needsTruck = alert;

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
                    TruckState = null,
                    TruckStarted = null,
                    TruckMoving = null,
                    TruckReached = null,
                    TruckLatitude = null,
                    TruckLongitude = null,
                    TruckLocation = null,
                    Source = "Arduino",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                };

                await _service.AddSensorHistoryAsync(entity);

                var resp = new
                {
                    status = "OK",
                    alert = alert,
                    needsTruck = needsTruck,
                };
                return Ok(resp);
            }
            catch (Exception ex)
            {
                _ = _logRepo.AddAsync(new LogEntry
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
                    _ = _logRepo.AddAsync(new LogEntry
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

                string bin2Led = bin2State.Equals("BRIGHT", StringComparison.OrdinalIgnoreCase) ? "red"
                    : bin2State.Equals("DIM", StringComparison.OrdinalIgnoreCase) ? "yellow"
                    : bin2State.Equals("DARK", StringComparison.OrdinalIgnoreCase) ? "green" : "off";

                string bin3Led = bin3State.Equals("FLOOD!", StringComparison.OrdinalIgnoreCase) ? "red"
                    : bin3State.Equals("DAMP", StringComparison.OrdinalIgnoreCase) ? "yellow"
                    : bin3State.Equals("DRY", StringComparison.OrdinalIgnoreCase) ? "green" : "off";

                // needsTruck logic: s1 != "EMPTY" && s2 != "DARK" && s3 != "DRY"
                // Bin2 DARK = closed = good state, so truck needed if NOT dark
                bool needsTruck =
                    !bin1State.Equals("EMPTY", StringComparison.OrdinalIgnoreCase)
                    && !bin2State.Equals("DARK", StringComparison.OrdinalIgnoreCase)
                    && !bin3State.Equals("DRY", StringComparison.OrdinalIgnoreCase);

                var resp = new
                {
                    id = latest.Id,
                    bin1 = new { fill = latest.Bin1Fill, state = latest.Bin1State, led = bin1Led },
                    bin2 = new { light = latest.Bin2Light, state = latest.Bin2State, led = bin2Led },
                    bin3 = new { water = latest.Bin3Water, state = latest.Bin3State, led = bin3Led },
                    alert = latest.Alert == 1,
                    needsTruck = needsTruck,
                    timestamp = latest.CreatedAt.ToString("o")
                };
                _ = _logRepo.AddAsync(new LogEntry
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
                _ = _logRepo.AddAsync(new LogEntry
                {
                    Level = "Error",
                    Message = ex.Message,
                    Exception = ex.ToString(),
                    Source = "Status"
                });
                return StatusCode(500, new ErrorResponse { Msg = "Internal server error" });
            }
        }

        [HttpPost("truck-status")]
        public async Task<IActionResult> TruckStatus([FromBody] TruckMovementRequest request)
        {
            if (request == null)
                return BadRequest();

            if (request.HistoryId.HasValue && request.HistoryId.Value <= 0)
            {
                return BadRequest(new { status = "ERROR", msg = "historyId must be greater than 0" });
            }

            if (request.Key != _apiKey)
            {
                _ = _logRepo.AddAsync(new LogEntry
                {
                    Level = "Warning",
                    Message = "Invalid API key for truck status",
                    RequestBody = System.Text.Json.JsonSerializer.Serialize(request),
                    Source = "TruckStatus"
                });
                return Unauthorized(new { status = "ERROR", msg = "Invalid key" });
            }

            var snapshot = new TruckMovementResponse
            {
                Status = "OK",
                TruckState = NormalizeTruckState(request),
                Started = request.Started,
                Moving = request.Moving,
                Reached = request.Reached,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Location = request.Location
            };

            lock (TruckMovementLock)
            {
                TruckMovementState = snapshot;
            }

            var updated = await _service.UpdateTruckMovementAsync(request);
            if (!updated)
            {
                if (request.HistoryId.HasValue)
                {
                    return NotFound(new { status = "ERROR", msg = $"History record with id {request.HistoryId.Value} was not found" });
                }

                return NotFound(new { status = "ERROR", msg = "No history record available to update" });
            }

            _ = _logRepo.AddAsync(new LogEntry
            {
                Level = "Info",
                Message = $"Truck status updated: {snapshot.TruckState}",
                ResponseBody = System.Text.Json.JsonSerializer.Serialize(snapshot),
                Source = "TruckStatus"
            });

            await Task.CompletedTask;
            return Ok(snapshot);
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
                        NeedsTruck = x.NeedsTruck,
                        TruckState = x.TruckState,
                        TruckStarted = x.TruckStarted,
                        TruckMoving = x.TruckMoving,
                        TruckReached = x.TruckReached,
                        TruckLatitude = x.TruckLatitude,
                        TruckLongitude = x.TruckLongitude,
                        TruckLocation = x.TruckLocation,
                        Source = x.Source,
                        Timestamp = x.CreatedAt.ToString("o")
                    }).ToList()
                };
                _ = _logRepo.AddAsync(new LogEntry
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
                _ = _logRepo.AddAsync(new LogEntry
                {
                    Level = "Error",
                    Message = ex.Message,
                    Exception = ex.ToString(),
                    Source = "History"
                });
                return StatusCode(500, new ErrorResponse { Msg = "Internal server error" });
            }
        }

        private static TruckMovementResponse GetTruckMovementSnapshot()
        {
            lock (TruckMovementLock)
            {
                return new TruckMovementResponse
                {
                    Status = TruckMovementState.Status,
                    TruckState = TruckMovementState.TruckState,
                    Started = TruckMovementState.Started,
                    Moving = TruckMovementState.Moving,
                    Reached = TruckMovementState.Reached,
                    Latitude = TruckMovementState.Latitude,
                    Longitude = TruckMovementState.Longitude,
                    Location = TruckMovementState.Location
                };
            }
        }

        private static string NormalizeTruckState(TruckMovementRequest request)
        {
            if (request.Reached)
                return "Reached";

            if (request.Moving)
                return "Moving";

            if (request.Started)
                return "Started";

            return string.IsNullOrWhiteSpace(request.State) ? "Idle" : request.State;
        }
    }
}
