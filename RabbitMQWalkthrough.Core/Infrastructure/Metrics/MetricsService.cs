using RabbitMQWalkthrough.Core.Infrastructure.Metrics.Collectors;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace RabbitMQWalkthrough.Core.Infrastructure.Metrics
{
    public class MetricsService
    {
        private readonly IEnumerable<IMetricCollector> metricCollectors;
        private readonly NpgsqlConnection sqlConnection;

        public MetricsService(IEnumerable<IMetricCollector> metricCollectors, NpgsqlConnection sqlConnection)
        {
            this.metricCollectors = metricCollectors;
            this.sqlConnection = sqlConnection;
        }


        public async Task CollectAndStoreAsync() => await this.StoreAsync(await this.CollectAsync());
        

        private async Task<Metric> CollectAsync()
        {
            Metric metric = new()
            {
                Date = DateTime.UtcNow
            };

            foreach (IMetricCollector metricCollector in this.metricCollectors)
            {
                await metricCollector.CollectAndSetAsync(metric);
            }

            return metric;
        }

        private async Task StoreAsync(Metric metric)
        {
            await this.sqlConnection.ExecuteAsync(@"INSERT INTO app.""Metrics""
           (""Date""
           ,""WorkerCount""
           ,""WorkLoadSize""
           ,""ConsumerCount""
           ,""ConsumerThroughput""
           ,""QueueSize""
           ,""PublishRate""
           ,""ConsumeRate"")
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
