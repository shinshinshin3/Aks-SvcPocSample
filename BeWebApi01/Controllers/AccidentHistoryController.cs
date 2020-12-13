using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using CommonLibrary.Models;
using BeWebApi01.Context;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BeWebApi01.Controllers
{

    [Route("api/[controller]")]
    public class AccidentHistoryController : Controller
    {
        private readonly DatabaseContext _context;
        private IConfiguration _configuration;
        private TelemetryClient _telemetryClient;
        private ILogger _logger;

        public AccidentHistoryController(DatabaseContext context, IConfiguration configuraion, TelemetryClient telemetryClient, ILogger<AccidentHistoryController> logger)
        {
            _context = context;
            _telemetryClient = telemetryClient;
            _configuration = configuraion;
            _logger = logger;
        }

        // POST api/Accident
        [HttpPost]
        public async Task<StatusCodeResult> Post([FromBody] Accident Accident)
        {
            //_telemetryClient.TrackTrace(JsonConvert.SerializeObject(Accident));
            try
            {
                //_context.Accident.AsNoTracking();
                _logger.LogDebug("Accident Recieve: {0}", Accident, LogLevel.Debug);
                _context.accident.Add(Accident);
                await _context.SaveChangesAsync();
                _logger.LogInformation(String.Format("SQL Insert: {0}", JsonConvert.SerializeObject(Accident)), LogLevel.Information);
            }
            catch (Exception ex)
            {
                //_telemetryClient.TrackTrace(ex.Message);
                _logger.LogError(ex.Message, LogLevel.Error);
                return StatusCode(500);
            }
            return StatusCode(200);
        }
    }
}
