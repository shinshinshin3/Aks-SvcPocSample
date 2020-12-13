using System;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace CommonLibrary.Storage
{
    public class StorageQueueUtil
    {

        private QueueClient _queueClient;
        private string queueName;

        //Constructor
        public StorageQueueUtil()
        {

        }

        //Constructor Override
        public StorageQueueUtil(string connectionString, string queueName)
        {
            _queueClient = new QueueClient(connectionString, queueName);

        }

        public static void CreateQueue(string connectionString, string queueName)
        {
            QueueClient queueClient = new QueueClient(connectionString, queueName);

            // Create the queue
            queueClient.Create();
        }


        public static void SendQueueMessage(string connectionString, string queueName, string message)
        {
            QueueClient queueClient = new QueueClient(connectionString, queueName);

            // Save the receipt so we can update this message later
            var receipt = queueClient.SendMessage(message);
        }

        public static async Task<Azure.Response<SendReceipt>> SendQueueMessageAsync(string connectionString, string queueName, string message)
        {
            QueueClient queueClient = new QueueClient(connectionString, queueName);
            return await queueClient.SendMessageAsync (message);
        }

        public async Task<Azure.Response<SendReceipt>> SendQueueMessageAsync(string queueName)
        {
            return await _queueClient.SendMessageAsync(queueName);
        }


        public SendReceipt SendQueueMessage(string queueName)
        {
            return _queueClient.SendMessage(queueName);
        }

    }
}
