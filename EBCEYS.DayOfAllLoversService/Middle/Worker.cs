using System.Runtime.Versioning;
using EBCEYS.DayOfAllLoversService.Extensions;

namespace EBCEYS.DayOfAllLoversService.Middle
{
    [SupportedOSPlatform("windows")]
    internal class Worker(ILogger<Worker> logger, IConfiguration config, TextSpeekerService speaker) : BackgroundService
    {
        private readonly List<string> texts = config.GetSection("TextesToSpeak")?.Get<List<string>>() ?? throw new ArgumentException("Get empty textes to speak!");
        private readonly int delayStartInterval = config.GetSection("SpeakerDelayInterval")?.GetValue<int?>("Start") ?? 0;
        private readonly int delayEndInterval = config.GetSection("SpeakerDelayInterval")?.GetValue<int?>("End") ?? 100;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (delayEndInterval <= 0 || delayStartInterval < 0 || delayStartInterval > delayEndInterval)
            {
                throw new InvalidOperationException("Intervals are invalid!");
            }
            logger.LogInformation("Service started at {now}", DateTimeOffset.Now);
            while (!stoppingToken.IsCancellationRequested)
            {
                TimeSpan randomTime = TimeSpan.FromSeconds(Random.Shared.Next(delayStartInterval, delayEndInterval));
                string randomText = texts.GetRandomElement() ?? "Какой-то случайный комплимент";
                await SayTextAsync(randomText);
                await Task.Delay(randomTime, stoppingToken);
            }
        }

        private Task SayTextAsync(string text)
        {
            speaker.Speak(text);
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping service at {now}", DateTimeOffset.Now);
            //speaker.CancelAll();
            return base.StopAsync(cancellationToken);
        }
    }
}
