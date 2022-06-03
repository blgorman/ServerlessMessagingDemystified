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


        public static async Task Main(string[] args)
        {
            BuildOptions();
            Console.WriteLine("Hello World");

            //add the service bus connection and clients
            var sbConnectionString = _configuration["ServiceBus:ReadOnlyConnectionString"];
            var sbQueueName = _configuration["ServiceBus:QueueName"];
            var client = new ServiceBusClient(sbConnectionString);

            //https://www.michalbialecki.com/en/2018/05/21/receiving-only-one-message-from-azure-service-bus/
            var queueClient = new QueueClient(sbConnectionString, sbQueueName);

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

            Console.WriteLine($"Movie Review found: {movie.Title} was released in {movie.ReleaseYear} and is rated {movie.MPAARating}");
        }

        private static void BuildOptions()
        {
            _configuration = ConfigurationBuilderSingleton.ConfigurationRoot;
        }
    }
}
