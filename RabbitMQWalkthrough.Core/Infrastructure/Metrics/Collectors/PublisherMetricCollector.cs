﻿using RabbitMQWalkthrough.Core.Infrastructure.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQWalkthrough.Core.Infrastructure.Metrics.Collectors
{
    public class PublisherMetricCollector : IMetricCollector
    {
        private readonly PublisherManager publisherManager;

        public PublisherMetricCollector(PublisherManager publisherManager)
        {
            this.publisherManager = publisherManager;
        }

        public Task CollectAndSetAsync(Metric metric)
        {
            metric.WorkerCount = this.publisherManager.Publishers.Count();
            metric.WorkLoadSize = this.publisherManager.Publishers.Sum(it => it.MessagesPerSecond);

            return Task.CompletedTask;
        }
    }
}
