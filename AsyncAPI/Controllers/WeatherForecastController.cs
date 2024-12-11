using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace AsyncAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private Channel<WeatherJob> _channel;

        private ConcurrentDictionary<string, Status> _condict;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, Channel<WeatherJob> channel, ConcurrentDictionary<string, Status> condict)
        {
            _logger = logger;
            _channel = channel;
            _condict = condict;
        }
        //Task<IEnumerable<WeatherForecast>>
        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get(int num)
        {
            string id = Guid.NewGuid().ToString();
            await _channel.Writer.WriteAsync(new WeatherJob { id = id, num = num });

            _condict[id] = Status.Queued;

            return Accepted(new { id = id, status = Status.Queued.ToString() });
           
        }

        [HttpGet("{id}/status")]
        public async Task<IActionResult> Get(string id)
        {
            if (!_condict.TryGetValue(id, out var status))
            {
                return NotFound();
            }
            var response = (new { id = id, status= Status.Queued.ToString() });
            if (status == Status.Completed)
            {
                response = response with { status = Status.Completed.ToString() };
            }
            return Ok(response);
        }
    }
}
