using RabbitMQWalkthrough.Core.Infrastructure.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQWalkthrough.Core.Infrastructure.Metrics.Collectors
{
    public class ConsumerMetricCollector : IMetricCollector
    {
        private readonly ConsumerManager consumerManager;

        public ConsumerMetricCollector(ConsumerManager consumerManager)
        {
            this.consumerManager = consumerManager;
        }

        public void CollectAndSet(Metric metric)
        {
            metric.ConsumerCount = this.consumerManager.Consumers.Count();
            metric.ConsumerThroughput = this.consumerManager.Consumers.Sum(it => it.MessagesPerSecond);
        }
    }
}
