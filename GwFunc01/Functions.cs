using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using CommonLibrary.Models;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AksPocSampleFunctions
{
    public class Functions
    {
        private HttpClient client = new HttpClient();
        private TelemetryClient _telemetryClient;
        private readonly IConfiguration _configuration;
        private ILogger _logger;

        public Functions(IConfiguration configuration, TelemetryClient tc, ILogger<Functions> logger)
        {
            _configuration = configuration;
            _telemetryClient = tc;
            _logger = logger;
            //_logger.LogInformation("log start");
        }

        // "%BROKER_LIST%": this paramater's value is getting by Environment $BROKER_LIST   
        [FunctionName("Functions")]
        public async Task MultiItemTriggerTenPartitions(
            [KafkaTrigger("%BROKER_LIST%", "%TOPIC%", ConsumerGroup = "%CONSUMER_GROUP%")]
            KafkaEventData<string>[] events
            /* Azure Functionsで動作させるときはこれを使う。
            ,ILogger log
             */
            )
        {
            //_telemetryClient.TrackTrace("function start", SeverityLevel.Information);
            _logger.LogInformation("function start", SeverityLevel.Information);

            //var rets = new System.Collections.Generic.List<ConsumerResult>();
            foreach (var kafkaEvent in events)
            {
                var topicData = JsonConvert.DeserializeObject<RootObject>(kafkaEvent.Value);
                var now = DateTime.UtcNow;
                var consumerResult = new ConsumerResult()
                {
                    PartitionKey = GetInstanceName(),
                    //RowKey = Guid.NewGuid().ToString(),
                    //RowKey = topicData.TransactionId.ToString().PadLeft(8, '0'),
                    //RowKey = topicData.TransactionId,
                    consumeTime = now.ToString("yyyy-MM-dd-HH:mm:ss.fff"),
                    //timespan = (now - topicData.DateTime),
                    partition = kafkaEvent.Partition,
                    topic = kafkaEvent.Topic,
                    topicTime = kafkaEvent.Timestamp.ToString("yyyy-MM-dd-HH:mm:ss.fff"),
                    offset = kafkaEvent.Offset.ToString()
                };

                foreach (var i in topicData.Accidents)
                {
                    try
                    {
                        var jsonstr = JsonConvert.SerializeObject(i);
                        _logger.LogInformation(jsonstr);
                        //_telemetryClient.TrackTrace(jsonstr, SeverityLevel.Information);
                        var content = new StringContent(jsonstr, Encoding.UTF8, "application/json");
                        var ret = await client.PostAsync(_configuration.GetValue<string>("BackEnd_URL"), content);
                    }
                    catch (Exception ex)
                    {
                        _telemetryClient.TrackTrace(ex.Message, SeverityLevel.Error);
                        _logger.LogError(ex.Message);
                    }
                }
            }

            string GetInstanceName()
            {
                var hostname = Environment.GetEnvironmentVariable("COMPUTERNAME") ?? string.Empty;
                if (string.IsNullOrEmpty(hostname))
                {
                    hostname = Environment.MachineName;
                }
                return hostname;
            }
        }

        public class ConsumerResult
        {
            public string PartitionKey { get; set; }
            //public string RowKey { get; set; }
            //public TimeSpan timespan { get; set; }
            public string consumeTime { get; set; }
            public string topic { get; set; }
            public string topicTime { get; set; }
            public int partition { get; set; }
            public string offset { get; set; }
        }
    }
}
