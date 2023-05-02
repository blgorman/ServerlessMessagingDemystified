using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace ServiceBusConsumer
{
    public class Program
    {
        private static IConfigurationRoot _configuration;
        // connection string to the Service Bus namespace
        private static string _sbConnectionString = string.Empty;

        // name of the Service Bus topic
        private static string _sbTopicName = string.Empty;

        // names of subscriptions to the topic
        private static string _sbSubscriptionAllMovies = string.Empty;
        private static string _sbSubscriptionAdultMovies = string.Empty;
        private static string _sbSubscriptionFamilyMovies = string.Empty;

        // the client that owns the connection and can be used to create senders and receivers
        private static ServiceBusClient _sbClient;

        // the sender used to publish messages to the topic
        private static ServiceBusReceiver _sbReceiver;

        public static async Task Main(string[] args)
        {
            BuildOptions();
            Console.WriteLine("Service Bus Pub/Sub consumer started");

            _sbConnectionString = _configuration["ServiceBus:ReadOnlySASConnectionString"];
            _sbTopicName = _configuration["ServiceBus:TopicName"];
            _sbSubscriptionFamilyMovies = _configuration["ServiceBus:SubscriptionNameFamily"];
            _sbSubscriptionAdultMovies = _configuration["ServiceBus:SubscriptionNameAdult"];
            _sbSubscriptionAllMovies = _configuration["ServiceBus:SubscriptionAll"];

            //TODO: add guard clauses to check for null or empty values on config values.

            bool shouldContinue = false;
            do 
            {
                PrintMenu();

                bool success = int.TryParse(Console.ReadLine(), out int choice);

                while (!success || choice < 1 || choice > 3)
                {
                    Console.WriteLine("You made a bad choice. Try again");
                    PrintMenu();
                    success = int.TryParse(Console.ReadLine(), out choice);
                }

                var subChoice = GetSubscriptionChoice(choice);
                if (string.IsNullOrWhiteSpace(subChoice))
                { 
                    shouldContinue = true;
                    continue;
                }

                _sbClient = new ServiceBusClient(_sbConnectionString);
                _sbReceiver = _sbClient.CreateReceiver(_sbTopicName, subChoice, new ServiceBusReceiverOptions() { ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete });
                await ReceiveAllMessageFromSubscription();

                Console.WriteLine("Do you want to continue [y/n]?");
                shouldContinue = Console.ReadLine()?.ToLower().StartsWith('y') ?? false;

            } while (shouldContinue);

            Console.WriteLine("Program completed");
        }

        private static async Task PrintMenu()
        {
            Console.WriteLine(new string('*', 80));
            Console.WriteLine("Which type of movies do you want to review? ");
            Console.WriteLine(new string('*', 80));
            Console.WriteLine("* 1 - All movies");
            Console.WriteLine("* 2 - Family movies");
            Console.WriteLine("* 3 - Adult movies");
            Console.WriteLine(new string('*', 80));
        }

        private static string GetSubscriptionChoice(int choice)
        {
            switch (choice)
            {
                case 1:
                    Console.WriteLine("You have chosen all movies");
                    return _sbSubscriptionAllMovies;
                case 2:
                    Console.WriteLine("You have chosen the family movies");
                    return _sbSubscriptionFamilyMovies;
                case 3:
                    Console.WriteLine("You have chosen the adult movies");
                    return _sbSubscriptionAdultMovies;
                default:
                    Console.WriteLine("You made a bad choice. All movies selected by default");
                    return _sbSubscriptionAllMovies;
            }
        }

        private static async Task ReceiveAllMessageFromSubscription()
        {
            if (_sbReceiver == null)
            {
                Console.WriteLine("Receiver is not initialized correctly.");
                return;
            }   

            var receivedMessages = 0;

            Console.WriteLine("Receiving all messages from your chosen subscription");

            while (true)
            {
                var receivedMessage = await _sbReceiver.ReceiveMessageAsync(TimeSpan.FromSeconds(10));
                if (receivedMessage != null)
                {
                    foreach (var prop in receivedMessage.ApplicationProperties)
                    {
                        Console.Write("{0}={1},", prop.Key, prop.Value);
                    }
                    Console.WriteLine("CorrelationId={0}", receivedMessage.CorrelationId);
                    receivedMessages++;
                }
                else
                {
                    // No more messages to receive.
                    break;
                }
            }
            Console.WriteLine($"Received {receivedMessages} messages from subscription.");
        }

        private static void BuildOptions()
        {
            _configuration = ConfigurationBuilderSingleton.ConfigurationRoot;
        }
    }
}
