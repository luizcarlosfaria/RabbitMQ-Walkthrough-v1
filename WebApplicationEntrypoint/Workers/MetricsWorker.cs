﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQWalkthrough.Core.Infrastructure.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplicationEntrypoint.Workers
{
    public class MetricsWorker : BackgroundService
    {
        private readonly ILogger<MetricsWorker> _logger;
        private readonly MetricsService metricsService;

        public MetricsWorker(ILogger<MetricsWorker> logger, MetricsService metricsService)
        {
            this._logger = logger;
            this.metricsService = metricsService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                this._logger.LogInformation("MetricsWorker running at: {time}", DateTimeOffset.Now);
                try
                {

                    this.metricsService.CollectAndStoreAsync();
                    await Task.Delay(1000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}
