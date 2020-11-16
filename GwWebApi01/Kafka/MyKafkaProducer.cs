using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using CommonLibrary.Models;
using Newtonsoft.Json;
using System.Threading;

namespace GwWebApi01.Kafka
{

    public struct kafkaTopic
    {

    }


    public class MyKafkaProducer : IDisposable
    {
        //private string bootstrapServers;
        public string topic;
        private ProducerConfig producerConfig;
        private IProducer<Null, string> producer;
        //private static Logger log = Logger.GetInstance("./logfile", true);
        private static Error kafkaError = null;


        //コンストラクタ
        public MyKafkaProducer()
        {
            this.producerConfig = new ProducerConfig();
        }

        public MyKafkaProducer(string bootstrapServers, string topic)
        {
            this.producerConfig = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                Debug = "all"
            };
            this.topic = topic;
            try
            {
                this.producer = new ProducerBuilder<Null, string>(this.producerConfig)
                .SetLogHandler(LogHandler)
                .SetErrorHandler(ErrorHandler)
                .Build();
            }
            catch
            {
                throw;

            }
        }

        Action<IProducer<Null, string>, LogMessage> LogHandler =
        (IProducer<Null, string> i, LogMessage l) =>
        {
            //Console.WriteLine($"{DateTime.UtcNow}:{l.Level}:{l.Message}");   
        };

        static Action<IProducer<Null, string>, Error> ErrorHandler =
        (IProducer<Null, string> i, Error e) =>
        {
            Console.WriteLine($"{DateTime.UtcNow}:{e.Code}'{e.Reason}");
            /*
            try
            {
                i.AbortTransaction(TimeSpan.FromMilliseconds(10));
            }
            catch
            {
                throw;
            }
            */
        };

        Action<DeliveryReport<Null, string>> deliveryHandler =
        (r) => {
            var myDelivaryResult = new DeliveryLog()
            {
                topicTimeStamp = r.Timestamp.UtcDateTime,
                topic = r.Topic,
                offset = r.Offset.Value,
                partition = r.TopicPartition.Partition.Value
                //message = r.Message.Value
            };
            Console.WriteLine(JsonConvert.SerializeObject(myDelivaryResult));
            //log.Info(JsonConvert.SerializeObject(myDelivaryResult));
        };

        public async Task<DeliveryLog> produceAsyncWithConnection(string bootstrapServers, string topic, string message)
        {
            try
            {
                /*
                if(kafkaError.Code == ErrorCode.Local_Transport)
                {
                    Console.WriteLine($"{DateTime.UtcNow}:{kafkaError.Code}'{kafkaError.Reason}");
                    //throw new KafkaException(kafkaError);
                    throw new SystemException();
                }
                */

                this.producerConfig.BootstrapServers = bootstrapServers;
                kafkaError = null;
                this.producer = new ProducerBuilder<Null, string>(this.producerConfig)
                .SetLogHandler(LogHandler)
                .SetErrorHandler(ErrorHandler)
                .Build();
                var deliveryResult = await this.producer.ProduceAsync(this.topic, new Message<Null, string> { Value = message });
                return new DeliveryLog()
                {
                    topicTimeStamp = deliveryResult.Timestamp.UtcDateTime,
                    topic = deliveryResult.Topic,
                    offset = deliveryResult.Offset.Value,
                    partition = deliveryResult.Partition.Value,
                    //message = deliveryResult.Message.Value
                };
            }
            catch (ProduceException<Null, string> e)
            {
                Console.WriteLine(e.Error.Reason);
                throw;
            }
            catch
            {
                throw;
            }
        }

        public async Task<DeliveryLog> produceAsync(string message, CancellationToken cancellationToken)
        {
            try
            {
                var deliveryResult = await this.producer.ProduceAsync(this.topic, new Message<Null, string> { Value = message }, cancellationToken);

                return new DeliveryLog()
                {
                    topicTimeStamp = deliveryResult.Timestamp.UtcDateTime,
                    topic = deliveryResult.Topic,
                    offset = deliveryResult.Offset.Value,
                    partition = deliveryResult.Partition.Value,
                    //message = deliveryResult.Message.Value
                };
            }
            catch(ProduceException<Null, string> e)
            {
                Console.WriteLine(e.Error.Reason);
                throw;
            }
            catch(OperationCanceledException ex)
            {
                Console.WriteLine(ex.Message + "probably kafka delay or down");
                throw;
            }
            catch
            {
                throw;

            }

        }
        public void produce(string message)
        {
            try
            {
                this.producer.Produce(this.topic, new Message<Null, string> { Value = message }, deliveryHandler);
                //return handler.Error.isError;

            }
            catch(Exception)
            {
                //
                throw;
            }
        }

        public void Dispose()
        {
            this.producer.Flush(TimeSpan.FromSeconds(10));
        }
    }
}