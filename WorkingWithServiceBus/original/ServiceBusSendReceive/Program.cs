using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace ServiceBusSendReceive
{
    public class Program
    {
        private static IConfigurationRoot _configuration;
        private static string connectionString = "<NAMESPACE CONNECTION STRING>";

        // name of your Service Bus queue
        private static string queueName = "your-queue-name";

        // the client that owns the connection and can be used to create senders and receivers
        private static ServiceBusClient client;

        // the sender used to publish messages to the queue
        private static ServiceBusSender sender;

        private const int numOfMessages = 5;

        public static async Task Main(string[] args)
        {
            BuildOptions();
            Console.WriteLine("Producer");

            connectionString = _configuration["ServiceBus:ConnectionString"];
            queueName = _configuration["ServiceBus:QueueName"];


            // Create the clients that we'll use for sending and processing messages.
            client = new ServiceBusClient(connectionString);
            sender = client.CreateSender(queueName);

            // create a batch
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            for (int i = 1; i <= numOfMessages; i++)
            {
                // try adding a message to the batch
                if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i}")))
                {
                    // if an exception occurs
                    throw new Exception($"Exception {i} has occurred.");
                }
            }

            try
            {
                // Use the producer client to send the batch of messages to the Service Bus queue
                await sender.SendMessagesAsync(messageBatch);
                Console.WriteLine($"A batch of {numOfMessages} messages has been published to the queue.");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }

            Console.WriteLine("Press any key to end the application");
            Console.ReadKey();
        }

        private static void BuildOptions()
        {
            _configuration = ConfigurationBuilderSingleton.ConfigurationRoot;
        }
    }
}
