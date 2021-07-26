using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQWalkthrough.Core.Infrastructure.Metrics.Collectors
{
    public class CollectorFixer : IMetricCollector
    {

        double lastValidPublishRate = 0;
        double lastValidConsumeRate = 0;

        int zeroMetrics = 0;

        public void CollectAndSet(Metric metric)
        {
            bool hasMetric = (metric.PublishRate + metric.ConsumeRate > 0);

            if (hasMetric || this.zeroMetrics == 5)
            {
                this.zeroMetrics = 0;
                this.lastValidPublishRate = metric.PublishRate;
                this.lastValidConsumeRate = metric.ConsumeRate;
            }
            else
            {
                this.zeroMetrics++;

                metric.PublishRate = this.lastValidPublishRate;
                metric.ConsumeRate = this.lastValidConsumeRate;
            }


        }


    }
}