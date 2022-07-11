using Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus;
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
        private static string _sbOnlyOneConnectionString;
        private static string _sbQueueName;


        public static async Task Main(string[] args)
        {
            BuildOptions();
            Console.WriteLine("Hello World");

            //add the service bus connection and clients
            _sbConnectionString = _configuration["ServiceBus:ReadOnlyConnectionString"];
            _sbOnlyOneConnectionString = _configuration["ServiceBus:ReadOnlyConnectionStringWithoutEntityPath"];
            _sbQueueName = _configuration["ServiceBus:QueueName"];

            if (string.IsNullOrEmpty(_sbConnectionString) || string.IsNullOrEmpty(_sbQueueName))
            {
                Console.WriteLine("Ensure connection string and queue name are set correctly");
                return;
            }
            
            ProcessOnlyOneMessage();

            await TypicalQueueProcessing();
        }

        private static async void ProcessOnlyOneMessage()
        {
            //NOTE: This is now deprecated, as noted in the nuget package manager
            //      but it still works at this time.
            //https://www.michalbialecki.com/en/2018/05/21/receiving-only-one-message-from-azure-service-bus/
            var queueClient = new QueueClient(_sbOnlyOneConnectionString, _sbQueueName);

            var messageBody = string.Empty;
            var movie = new Movie();

            try
            {
                queueClient.RegisterMessageHandler(
                async (message, token) =>
                {
                    messageBody = Encoding.UTF8.GetString(message.Body);
                    Console.WriteLine($"Received: {messageBody}");

                    movie = JsonConvert.DeserializeObject<Movie>(messageBody);
                    Console.WriteLine($"Movie converted: {movie.Title} | {movie.MPAARating}");

                    await queueClient.CompleteAsync(message.SystemProperties.LockToken);
                    await queueClient.CloseAsync();
                },
                new MessageHandlerOptions(async args => Console.WriteLine(args.Exception))
                { MaxConcurrentCalls = 1, AutoComplete = false });

                Console.WriteLine($"Message: {messageBody}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            var autoTimeOut = DateTime.Now.AddSeconds(30);
            while (string.IsNullOrWhiteSpace(movie?.Title) && DateTime.Now < autoTimeOut)
            {
                Thread.Sleep(1000);
            }

            if (string.IsNullOrWhiteSpace(movie?.Title))
            {
                Console.WriteLine(new Exception("No data returned from the queue for processing"));
                return;
            }

            await WriteMessageToConsole(movie);
            
        }

        private static async Task TypicalQueueProcessing()
        {
            var client = new ServiceBusClient(_sbConnectionString);
            var processor = client.CreateProcessor(_sbQueueName, new ServiceBusProcessorOptions());

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
