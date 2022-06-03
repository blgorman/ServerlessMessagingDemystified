using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace Consumer
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
        private static ServiceBusProcessor processor;

        private const int numOfMessages = 5;

        public static async Task Main(string[] args)
        {
            BuildOptions();
            Console.WriteLine("Consumer");

            connectionString = _configuration["ServiceBus:ConnectionString"];
            queueName = _configuration["ServiceBus:QueueName"];

            // Create the client object that will be used to create sender and receiver objects
            client = new ServiceBusClient(connectionString);

            // create a processor that we can use to process the messages
            processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

            try
            {
                // add handler to process messages
                processor.ProcessMessageAsync += MessageHandler;

                // add handler to process any errors
                processor.ProcessErrorAsync += ErrorHandler;

                // start processing
                await processor.StartProcessingAsync();

                Console.WriteLine("Wait for a minute and then press any key to end the processing");
                Console.ReadKey();

                // stop processing
                Console.WriteLine("\nStopping the receiver...");
                await processor.StopProcessingAsync();
                Console.WriteLine("Stopped receiving messages");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await processor.DisposeAsync();
                await client.DisposeAsync();
            }

            Console.WriteLine("Press any key to end the application");
            Console.ReadKey();
        }

        private static async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"Received: {body}");

            // complete the message. messages is deleted from the queue.
            await args.CompleteMessageAsync(args.Message);
        }

        // handle any errors when receiving messages
        private static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }


        private static void BuildOptions()
        {
            _configuration = ConfigurationBuilderSingleton.ConfigurationRoot;
        }
    }
}
