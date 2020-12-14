using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using CommonLibrary.Storage;
using CommonLibrary.Models;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;
using System.IO;

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
            var topicName = _configuration.GetValue<string>("TOPIC");

            if (topicName.Contains("accident"))
            {
                //_telemetryClient.TrackTrace("function start", SeverityLevel.Information);
                //_logger.LogInformation("function start", SeverityLevel.Information);

                //var rets = new System.Collections.Generic.List<ConsumerResult>();

                _logger.LogInformation("KafkaEventData.Length: {0}", events.Length, SeverityLevel.Information);

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

                    var timeSpan = now - topicData.DateTime;
                    _logger.LogInformation("AccidentData.TimeLag: {0}", (int)timeSpan.TotalMilliseconds, SeverityLevel.Information);

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
                            //_telemetryClient.TrackTrace(ex.Message, SeverityLevel.Error);
                            _logger.LogError(ex.Message);
                        }
                    }
                }
            }
            else if (topicName.Contains("location"))
            {
                _logger.LogInformation("KafkaEventData.Length: {0}", events.Length, SeverityLevel.Information);

                try
                {
                    foreach (var kafkaEvent in events)
                    {
                        var locationList = JsonConvert.DeserializeObject<LocationList2>(kafkaEvent.Value);
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

                        var timeSpan = now - locationList.Locations[0].DateTime;
                        _logger.LogInformation("AccidentData.TimeLag: {0}", (int)timeSpan.TotalMilliseconds, SeverityLevel.Information);


                        var locationListJsonstr = JsonConvert.SerializeObject(locationList);
                        _logger.LogDebug("locationListJsonStr: {0}", locationListJsonstr, LogLevel.Debug);

                        // tcpconnection数の推移を見るためのコード
                        string blobUri = _configuration.GetValue<string>("BLOB_CONNECTIONSTRING");
                        string containerName = _configuration.GetValue<string>("BLOB_ContainerName");
                        string blobName = "location_" + Guid.NewGuid();

                        var blobClient = StorageBlobUtil.GetBlobClient(blobUri, containerName, blobName);
                        var ret = await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(locationListJsonstr)));
                        Console.WriteLine("sendBlob: {0}", ret.Value.BlobSequenceNumber);


                        string queueUri = _configuration.GetValue<string>("Queue_CONNECTIONSTRING");
                        string queueName = _configuration.GetValue<string>("Queue_Name");
                        var storageQueueUtil = new StorageQueueUtil(queueUri, queueName);
                        var recept = await storageQueueUtil.SendQueueMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(locationListJsonstr)));
                        Console.WriteLine("SendQueueMessage: {0}", recept.Value.MessageId);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    _logger.LogError(ex.Message, LogLevel.Error);
                }

            }
            else
            {

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
