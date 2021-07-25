using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQWalkthrough.Core.Metrics
{
    public class Metric
    {
        public int MetricId { get; set; }
        public DateTime Date { get; set; }
        public int WorkerCount { get; set; }
        public int WorkLoadSize { get; set; }
        public int ConsumerCount { get; set; }
        public int ConsumerThroughput { get; set; }
        public int QueueSize { get; set; }
        public double PublishRate { get; set; }
        public double ConsumeRate { get; set; }
    }
}
