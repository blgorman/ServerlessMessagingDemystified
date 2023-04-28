using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using MySolutionObjectModels;
using Newtonsoft.Json;
using System.Text;

namespace ServiceBusQueueConsumer
{
    public class Program
    {
        private static IConfigurationRoot _configuration;
        private static string _sbConnectionString;
        private static string _sbQueueName;
        private static int MAX_WAIT_TIME_SECONDS = 20;

        public static async Task Main(string[] args)
        {
            BuildOptions();
            Console.WriteLine("Welcome to the Service Bus Queue Processing Demos");

            //add the service bus connection and clients
            _sbConnectionString = _configuration["ServiceBus:ReadOnlyConnectionString"];
            _sbQueueName = _configuration["ServiceBus:QueueName"];

            if (string.IsNullOrEmpty(_sbConnectionString) || string.IsNullOrEmpty(_sbQueueName))
            {
                Console.WriteLine("Ensure connection string and queue name are set correctly");
                return;
            }

            Console.WriteLine("Would you like to process a single message? (y/n)");
            bool shouldContinue = Console.ReadLine().ToLower().StartsWith("y");
            while (shouldContinue)
            {
                Console.WriteLine("Processing next message!");
                await ProcessOnlyOneMessageAsync();
                Console.WriteLine("Would you like to process another single message? (y/n)");
                shouldContinue = Console.ReadLine().ToLower().StartsWith("y");
            }

            Console.WriteLine("Polling all remaining messages...hit any key to stop polling");
            await PollingQueueProcessingAsync();

            Console.WriteLine("Press any key to end the program");
            Console.ReadKey();
        }

        private static async Task ProcessOnlyOneMessageAsync()
        {
            var client = new ServiceBusClient(_sbConnectionString);
            var options = new ServiceBusReceiverOptions();
            options.PrefetchCount = 1;

            //at-most-once-delivery
            options.ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete;

            //create a receiver that we can use to process the messages one at a time
            var receiver = client.CreateReceiver(_sbQueueName, options);

            
            try
            {
                //max wait time is MAX_WAIT_TIME_SECONDS, if no messages are received by then, we will exit
                var waitTime = TimeSpan.FromSeconds(MAX_WAIT_TIME_SECONDS);
                var msg = await receiver.ReceiveMessageAsync(waitTime);

                if (msg is null || msg?.Body is null)
                {
                    Console.WriteLine("No messages in the queue and wait time has elapsed");
                    return;
                }

                string body = msg.Body.ToString();
                Console.WriteLine($"Received: {body}");

                var movie = JsonConvert.DeserializeObject<Movie>(body);
                await WriteMessageToConsole(movie);

                //no need to complete because we are using ReceiveAndDelete!
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await receiver.DisposeAsync();
                await client.DisposeAsync();
            }
        }

        private static async Task PollingQueueProcessingAsync()
        {
            var client = new ServiceBusClient(_sbConnectionString);
            //create a processor that we can use to process the messages until we decide to stop
            var processor = client.CreateProcessor(_sbQueueName, new ServiceBusProcessorOptions()); 

            try
            {
                // add handler to process messages
                processor.ProcessMessageAsync += MessageHandler;
                

                // add handler to process any errors
                processor.ProcessErrorAsync += ErrorHandler;

                // start processing 
                await processor.StartProcessingAsync();

                Console.ReadKey();

                // stop processing 
                Console.WriteLine("\nStopping the receiver...");
                await processor.StopProcessingAsync();
                Console.WriteLine("Stopped receiving messages");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await processor.DisposeAsync();
                await client.DisposeAsync();
            }
        }

        private static async Task WriteMessageToConsole(Movie movie)
        {
            Console.WriteLine($"Movie Review found: {movie.Title} was released in {movie.ReleaseYear} and is rated {movie.MPAARating}");
        }


        // handle received messages
        private static async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"Received: {body}");

            var movie = JsonConvert.DeserializeObject<Movie>(body);
            await WriteMessageToConsole(movie);

            // complete the message. message is deleted from the queue. 
            await args.CompleteMessageAsync(args.Message);

            System.Threading.Thread.Sleep(2000);
        }

        // handle any errors when receiving messages
        private static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine($"Error: {args.Exception.Message}");
            return Task.CompletedTask;
        }

        private static void BuildOptions()
        {
            _configuration = ConfigurationBuilderSingleton.ConfigurationRoot;
        }
    }
}
