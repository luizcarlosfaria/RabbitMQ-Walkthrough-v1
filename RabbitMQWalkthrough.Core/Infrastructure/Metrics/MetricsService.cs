using RabbitMQWalkthrough.Core.Infrastructure.Metrics.Collectors;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQWalkthrough.Core.Infrastructure.Metrics
{
    public class MetricsService
    {
        private readonly IEnumerable<IMetricCollector> metricCollectors;
        private readonly SqlConnection sqlConnection;

        public MetricsService(IEnumerable<IMetricCollector> metricCollectors, SqlConnection sqlConnection)
        {
            this.metricCollectors = metricCollectors;
            this.sqlConnection = sqlConnection;
        }


        public void CollectAndStore() => this.Store(this.Collect());
        

        private Metric Collect()
        {
            Metric metric = new()
            {
                Date = DateTime.UtcNow
            };

            foreach (IMetricCollector metricCollector in this.metricCollectors)
            {
                metricCollector.CollectAndSet(metric);
            }

            return metric;
        }

        private void Store(Metric metric)
        {
            this.sqlConnection.Execute(@"INSERT INTO [dbo].[Metrics]
           ([Date]
           ,[WorkerCount]
           ,[WorkLoadSize]
           ,[ConsumerCount]
           ,[ConsumerThroughput]
           ,[QueueSize]
           ,[PublishRate]
           ,[ConsumeRate])
     VALUES
           (@Date
           ,@WorkerCount
           ,@WorkLoadSize
           ,@ConsumerCount
           ,@ConsumerThroughput
           ,@QueueSize
           ,@PublishRate
           ,@ConsumeRate)", metric);


        }
    }
}
