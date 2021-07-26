using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQWalkthrough.Core.Infrastructure.Metrics.Collectors
{
    public class QueueMetricCollector : IMetricCollector
    {
        private readonly ILogger<QueueMetricCollector> logger;

        public QueueMetricCollector(ILogger<QueueMetricCollector> logger)
        {
            this.logger = logger;
        }


        public void CollectAndSet(Metric metric)
        {
            try
            {
                var metrics = this.GetRawMetrics();

                metric.QueueSize = metrics.Messages;
                metric.PublishRate = metrics.MessageStats?.PublishDetails?.Rate ?? 0;
                metric.ConsumeRate = metrics.MessageStats?.AckDetails?.Rate ?? 0;

            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Não foi possível obter métricas");
            }

        }



        private QueueMetrics GetRawMetrics()
        {
            RestClient client = new("http://rabbitmq:15672/api")
            {
                Authenticator = new HttpBasicAuthenticator("WalkthroughUser", "WalkthroughPassword")
            };

            RestRequest request = new(resource: "/queues/Walkthrough/test_queue", DataFormat.Json);

            var response = client.Get<QueueMetrics>(request);

            if (response.IsSuccessful)
                return response.Data;
            else
                throw new InvalidOperationException(response.ErrorMessage);
        }

        #region Model

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
        public class Arguments
        {
        }

        public class ChannelDetails
        {
            [JsonProperty("connection_name")]
            public string ConnectionName { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("node")]
            public string Node { get; set; }

            [JsonProperty("number")]
            public int Number { get; set; }

            [JsonProperty("peer_host")]
            public string PeerHost { get; set; }

            [JsonProperty("peer_port")]
            public int PeerPort { get; set; }

            [JsonProperty("user")]
            public string User { get; set; }
        }

        public class Queue
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("vhost")]
            public string Vhost { get; set; }
        }

        public class ConsumerDetail
        {
            [JsonProperty("arguments")]
            public Arguments Arguments { get; set; }

            [JsonProperty("channel_details")]
            public ChannelDetails ChannelDetails { get; set; }

            [JsonProperty("ack_required")]
            public bool AckRequired { get; set; }

            [JsonProperty("active")]
            public bool Active { get; set; }

            [JsonProperty("activity_status")]
            public string ActivityStatus { get; set; }

            [JsonProperty("consumer_tag")]
            public string ConsumerTag { get; set; }

            [JsonProperty("exclusive")]
            public bool Exclusive { get; set; }

            [JsonProperty("prefetch_count")]
            public int PrefetchCount { get; set; }

            [JsonProperty("queue")]
            public Queue Queue { get; set; }
        }

        public class BackingQueueStatus
        {
            [JsonProperty("avg_ack_egress_rate")]
            public double AvgAckEgressRate { get; set; }

            [JsonProperty("avg_ack_ingress_rate")]
            public double AvgAckIngressRate { get; set; }

            [JsonProperty("avg_egress_rate")]
            public double AvgEgressRate { get; set; }

            [JsonProperty("avg_ingress_rate")]
            public double AvgIngressRate { get; set; }

            [JsonProperty("delta")]
            public List<object> Delta { get; set; }

            [JsonProperty("len")]
            public int Len { get; set; }

            [JsonProperty("mode")]
            public string Mode { get; set; }

            [JsonProperty("next_seq_id")]
            public int NextSeqId { get; set; }

            [JsonProperty("q1")]
            public int Q1 { get; set; }

            [JsonProperty("q2")]
            public int Q2 { get; set; }

            [JsonProperty("q3")]
            public int Q3 { get; set; }

            [JsonProperty("q4")]
            public int Q4 { get; set; }

            [JsonProperty("target_ram_count")]
            public string TargetRamCount { get; set; }
        }

        [DebuggerDisplay("Rate: {Rate}")]
        public class AckDetails
        {
            [JsonProperty("rate")]
            public double Rate { get; set; }
        }

        [DebuggerDisplay("Rate: {Rate}")]
        public class DeliverDetails
        {
            [JsonProperty("rate")]
            public double Rate { get; set; }
        }

        [DebuggerDisplay("Rate: {Rate}")]
        public class DeliverGetDetails
        {
            [JsonProperty("rate")]
            public double Rate { get; set; }
        }

        [DebuggerDisplay("Rate: {Rate}")]
        public class DeliverNoAckDetails
        {
            [JsonProperty("rate")]
            public double Rate { get; set; }
        }

        [DebuggerDisplay("Rate: {Rate}")]
        public class GetDetails
        {
            [JsonProperty("rate")]
            public double Rate { get; set; }
        }

        [DebuggerDisplay("Rate: {Rate}")]
        public class GetEmptyDetails
        {
            [JsonProperty("rate")]
            public double Rate { get; set; }
        }

        [DebuggerDisplay("Rate: {Rate}")]
        public class GetNoAckDetails
        {
            [JsonProperty("rate")]
            public double Rate { get; set; }
        }

        [DebuggerDisplay("Rate: {Rate}")]
        public class RedeliverDetails
        {
            [JsonProperty("rate")]
            public double Rate { get; set; }
        }

        public class Stats
        {
            [JsonProperty("ack")]
            public int Ack { get; set; }

            [JsonProperty("ack_details")]
            public AckDetails AckDetails { get; set; }

            [JsonProperty("deliver")]
            public int Deliver { get; set; }

            [JsonProperty("deliver_details")]
            public DeliverDetails DeliverDetails { get; set; }

            [JsonProperty("deliver_get")]
            public int DeliverGet { get; set; }

            [JsonProperty("deliver_get_details")]
            public DeliverGetDetails DeliverGetDetails { get; set; }

            [JsonProperty("deliver_no_ack")]
            public int DeliverNoAck { get; set; }

            [JsonProperty("deliver_no_ack_details")]
            public DeliverNoAckDetails DeliverNoAckDetails { get; set; }

            [JsonProperty("get")]
            public int Get { get; set; }

            [JsonProperty("get_details")]
            public GetDetails GetDetails { get; set; }

            [JsonProperty("get_empty")]
            public int GetEmpty { get; set; }

            [JsonProperty("get_empty_details")]
            public GetEmptyDetails GetEmptyDetails { get; set; }

            [JsonProperty("get_no_ack")]
            public int GetNoAck { get; set; }

            [JsonProperty("get_no_ack_details")]
            public GetNoAckDetails GetNoAckDetails { get; set; }

            [JsonProperty("redeliver")]
            public int Redeliver { get; set; }

            [JsonProperty("redeliver_details")]
            public RedeliverDetails RedeliverDetails { get; set; }

            [JsonProperty("publish")]
            public int Publish { get; set; }

            [JsonProperty("publish_details")]
            public PublishDetails PublishDetails { get; set; }
        }

        public class Delivery
        {
            [JsonProperty("channel_details")]
            public ChannelDetails ChannelDetails { get; set; }

            [JsonProperty("stats")]
            public Stats Stats { get; set; }
        }

        public class EffectivePolicyDefinition
        {
        }

        public class GarbageCollection
        {
            [JsonProperty("fullsweep_after")]
            public int FullsweepAfter { get; set; }

            [JsonProperty("max_heap_size")]
            public int MaxHeapSize { get; set; }

            [JsonProperty("min_bin_vheap_size")]
            public int MinBinVheapSize { get; set; }

            [JsonProperty("min_heap_size")]
            public int MinHeapSize { get; set; }

            [JsonProperty("minor_gcs")]
            public int MinorGcs { get; set; }
        }

        public class Exchange
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("vhost")]
            public string Vhost { get; set; }
        }

        [DebuggerDisplay("Rate: {Rate}")]
        public class PublishDetails
        {
            [JsonProperty("rate")]
            public double Rate { get; set; }
        }

        public class Incoming
        {
            [JsonProperty("exchange")]
            public Exchange Exchange { get; set; }

            [JsonProperty("stats")]
            public Stats Stats { get; set; }
        }

        public class MessageStats
        {
            [JsonProperty("ack")]
            public int Ack { get; set; }

            [JsonProperty("ack_details")]
            public AckDetails AckDetails { get; set; }

            [JsonProperty("deliver")]
            public int Deliver { get; set; }

            [JsonProperty("deliver_details")]
            public DeliverDetails DeliverDetails { get; set; }

            [JsonProperty("deliver_get")]
            public int DeliverGet { get; set; }

            [JsonProperty("deliver_get_details")]
            public DeliverGetDetails DeliverGetDetails { get; set; }

            [JsonProperty("deliver_no_ack")]
            public int DeliverNoAck { get; set; }

            [JsonProperty("deliver_no_ack_details")]
            public DeliverNoAckDetails DeliverNoAckDetails { get; set; }

            [JsonProperty("get")]
            public int Get { get; set; }

            [JsonProperty("get_details")]
            public GetDetails GetDetails { get; set; }

            [JsonProperty("get_empty")]
            public int GetEmpty { get; set; }

            [JsonProperty("get_empty_details")]
            public GetEmptyDetails GetEmptyDetails { get; set; }

            [JsonProperty("get_no_ack")]
            public int GetNoAck { get; set; }

            [JsonProperty("get_no_ack_details")]
            public GetNoAckDetails GetNoAckDetails { get; set; }

            [JsonProperty("publish")]
            public int Publish { get; set; }

            [JsonProperty("publish_details")]
            public PublishDetails PublishDetails { get; set; }

            [JsonProperty("redeliver")]
            public int Redeliver { get; set; }

            [JsonProperty("redeliver_details")]
            public RedeliverDetails RedeliverDetails { get; set; }
        }

        [DebuggerDisplay("Rate: {Rate}")]
        public class MessagesDetails
        {
            [JsonProperty("rate")]
            public double Rate { get; set; }
        }

        [DebuggerDisplay("Rate: {Rate}")]
        public class MessagesReadyDetails
        {
            [JsonProperty("rate")]
            public double Rate { get; set; }
        }

        public class MessagesUnacknowledgedDetails
        {
            [JsonProperty("rate")]
            public double Rate { get; set; }
        }

        [DebuggerDisplay("Rate: {Rate}")]
        public class ReductionsDetails
        {
            [JsonProperty("rate")]
            public double Rate { get; set; }
        }

        public class QueueMetrics
        {
            [JsonProperty("consumer_details")]
            public List<ConsumerDetail> ConsumerDetails { get; set; }

            [JsonProperty("arguments")]
            public Arguments Arguments { get; set; }

            [JsonProperty("auto_delete")]
            public bool AutoDelete { get; set; }

            [JsonProperty("backing_queue_status")]
            public BackingQueueStatus BackingQueueStatus { get; set; }

            [JsonProperty("consumer_capacity")]
            public double ConsumerCapacity { get; set; }

            [JsonProperty("consumer_utilisation")]
            public double ConsumerUtilisation { get; set; }

            [JsonProperty("consumers")]
            public int Consumers { get; set; }

            [JsonProperty("deliveries")]
            public List<Delivery> Deliveries { get; set; }

            [JsonProperty("durable")]
            public bool Durable { get; set; }

            [JsonProperty("effective_policy_definition")]
            public EffectivePolicyDefinition EffectivePolicyDefinition { get; set; }

            [JsonProperty("exclusive")]
            public bool Exclusive { get; set; }

            [JsonProperty("exclusive_consumer_tag")]
            public object ExclusiveConsumerTag { get; set; }

            [JsonProperty("garbage_collection")]
            public GarbageCollection GarbageCollection { get; set; }

            [JsonProperty("head_message_timestamp")]
            public object HeadMessageTimestamp { get; set; }

            [JsonProperty("incoming")]
            public List<Incoming> Incoming { get; set; }

            [JsonProperty("memory")]
            public int Memory { get; set; }

            [JsonProperty("message_bytes")]
            public long MessageBytes { get; set; }

            [JsonProperty("message_bytes_paged_out")]
            public int MessageBytesPagedOut { get; set; }

            [JsonProperty("message_bytes_persistent")]
            public long MessageBytesPersistent { get; set; }

            [JsonProperty("message_bytes_ram")]
            public long MessageBytesRam { get; set; }

            [JsonProperty("message_bytes_ready")]
            public long MessageBytesReady { get; set; }

            [JsonProperty("message_bytes_unacknowledged")]
            public long MessageBytesUnacknowledged { get; set; }

            [JsonProperty("message_stats")]
            public MessageStats MessageStats { get; set; }

            [JsonProperty("messages")]
            public int Messages { get; set; }

            [JsonProperty("messages_details")]
            public MessagesDetails MessagesDetails { get; set; }

            [JsonProperty("messages_paged_out")]
            public int MessagesPagedOut { get; set; }

            [JsonProperty("messages_persistent")]
            public int MessagesPersistent { get; set; }

            [JsonProperty("messages_ram")]
            public int MessagesRam { get; set; }

            [JsonProperty("messages_ready")]
            public int MessagesReady { get; set; }

            [JsonProperty("messages_ready_details")]
            public MessagesReadyDetails MessagesReadyDetails { get; set; }

            [JsonProperty("messages_ready_ram")]
            public int MessagesReadyRam { get; set; }

            [JsonProperty("messages_unacknowledged")]
            public int MessagesUnacknowledged { get; set; }

            [JsonProperty("messages_unacknowledged_details")]
            public MessagesUnacknowledgedDetails MessagesUnacknowledgedDetails { get; set; }

            [JsonProperty("messages_unacknowledged_ram")]
            public int MessagesUnacknowledgedRam { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("node")]
            public string Node { get; set; }

            [JsonProperty("operator_policy")]
            public object OperatorPolicy { get; set; }

            [JsonProperty("policy")]
            public object Policy { get; set; }

            [JsonProperty("recoverable_slaves")]
            public object RecoverableSlaves { get; set; }

            [JsonProperty("reductions")]
            public long Reductions { get; set; }

            [JsonProperty("reductions_details")]
            public ReductionsDetails ReductionsDetails { get; set; }

            [JsonProperty("single_active_consumer_tag")]
            public object SingleActiveConsumerTag { get; set; }

            [JsonProperty("state")]
            public string State { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("vhost")]
            public string Vhost { get; set; }
        }


        #endregion

    }
}
