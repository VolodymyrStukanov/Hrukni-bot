﻿using HrukniBot.Services.HoholServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HrukniBot.Services.ResetBackgroundService
{
    internal class ResetHoholsService : BackgroundService
    {
        private readonly ILogger<ResetHoholsService> logger;
        private readonly IHoholsService hoholService;
        public ResetHoholsService(ILogger<ResetHoholsService> logger,
            IHoholsService hoholService)
        {
            this.logger = logger;
            this.hoholService = hoholService;
        }

        private readonly TimeOnly resetHoholsTime = new TimeOnly(6, 0, 0);
        private readonly Dictionary<string, int> waitingTime = new Dictionary<string, int>()
        {
            { "second", 1000 },
            { "tenSeconds", 10000 },
            { "minute", 60000 },
            { "hour", 3600000 }
        };

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var time = DateTime.Now;

                    if (time.Hour == resetHoholsTime.Hour
                        && time.Minute == resetHoholsTime.Minute)
                    {
                        hoholService.ResetHohols();
                        Thread.Sleep(waitingTime["hour"]);
                    }

                    if (time.Hour != resetHoholsTime.Hour - 1)
                        Thread.Sleep(waitingTime["hour"]);
                    else if (time.Minute != (resetHoholsTime.Minute == 0 ? 60 - 1 : resetHoholsTime.Minute - 1))
                        Thread.Sleep(waitingTime["minute"]);
                    else
                        Thread.Sleep(waitingTime["tenSeconds"]);
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                logger.LogError(exception: ex, $"An error occurred in SetNewHohols method");
                return Task.CompletedTask;
            }
        }
    }
}
