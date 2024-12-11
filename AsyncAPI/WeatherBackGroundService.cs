
using AsyncAPI.Controllers;
using System;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace AsyncAPI
{
    public class WeatherBackGroundService : BackgroundService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        private Channel<WeatherJob> _channel;

        private ConcurrentDictionary<string, Status> _condict;
        private readonly ILogger<WeatherBackGroundService> _logger;
        public WeatherBackGroundService(ILogger<WeatherBackGroundService> logger, Channel<WeatherJob> channel, ConcurrentDictionary<string, Status> condict)
        {
            _logger = logger;
            _channel = channel;
            _condict = condict;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach(var job in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    _condict[job.id] = Status.Processing;
                    await Task.Delay(30000);
                    Enumerable.Range(1, job.num).Select(index => new WeatherForecast
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                    }).ToArray();
                    _condict[job.id] = Status.Completed;
                }
                catch(Exception ex)
                {
                    _condict[job.id] = Status.Failed;
                }
            }
            
        }
    }
}
