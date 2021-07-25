using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQWalkthrough.Core.Metrics.Collectors
{
    public interface IMetricCollector
    {
        void CollectAndSet(Metric metric);
    }
}
