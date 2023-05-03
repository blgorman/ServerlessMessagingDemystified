using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace ServiceBusAdministrator
{
    /// <summary>
    /// Note: This code is idempotent so you can run multiple times without error, even if topics/subscriptions do not or already exist.
    /// </summary>
    public class Program
    {
        private static IConfigurationRoot _configuration;
        // Service Bus Administration Client object to create topics and subscriptions
        private static ServiceBusAdministrationClient _adminClient;

        // connection string to the Service Bus namespace
        private static string _sbConnectionString = string.Empty;

        // name of the Service Bus topic
        private static string _sbTopicName = string.Empty;

        // names of subscriptions to the topic
        private static string _sbSubscriptionAllMovies = string.Empty;
        private static string _sbSubscriptionAdultMovies = string.Empty;
        private static string _sbSubscriptionFamilyMovies = string.Empty;

        //NOTE:
        /*
         * All of the following code is used to create the topic and subscriptions in your service bus namespace.
         * Everything done in code here (including topics, subscriptions, and filters) could have been done in the Bicep/ARM template, 
         *          but it's done here to show you how to do it in code.
         *                                     
        */
        public static async Task Main(string[] args)
        {
            //ensure appsettings/user secrets are loaded into the _configuration variable
            BuildOptions();
            
            //Get connection, topic, and subscription information from the configuration file
            _sbConnectionString = _configuration["ServiceBus:NamespaceRootAccessConnectionString"];
            _sbTopicName = _configuration["ServiceBus:TopicName"];
            _sbSubscriptionFamilyMovies = _configuration["ServiceBus:SubscriptionNameFamily"];
            _sbSubscriptionAdultMovies = _configuration["ServiceBus:SubscriptionNameAdult"];
            _sbSubscriptionAllMovies = _configuration["ServiceBus:SubscriptionAll"];

            //TODO: add guard clauses to check for null or empty values on config values.

            try
            {
                Console.WriteLine("Creating the Service Bus Administration Client object");
                _adminClient = new ServiceBusAdministrationClient(_sbConnectionString);

                //Create the topic and subscriptions

                //Ensure sure the topic exists
                await EnsureTopicExists(_sbTopicName);

                // Ensure the three subscriptions exist with appropriate filters

                // Create a True Rule filter with an expression that always evaluates to true
                // It's equivalent to using SQL rule filter with 1=1 as the expression
                var ruleFilter = new TrueRuleFilter();
                await EnsureSubscriptionExists(_sbTopicName, _sbSubscriptionAllMovies, ruleFilter);

                // Create a SQL filter with MPAARating set to "PG-13", or "R"
                var adultRatingRuleFilter = new SqlRuleFilter("MPAARating='PG-13' OR MPAARating = 'R'");
                await EnsureSubscriptionExists(_sbTopicName, _sbSubscriptionAdultMovies, adultRatingRuleFilter);

                // Create a SQL filter with MPAARating set to "PG-13", or "R"
                var familyRatingRuleFilter = new SqlRuleFilter("MPAARating='PG' OR MPAARating = 'G'");
                await EnsureSubscriptionExists(_sbTopicName, _sbSubscriptionFamilyMovies, familyRatingRuleFilter);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            Console.WriteLine("Topic and subscriptions exist as required");  
        }

        private static async Task EnsureTopicExists(string topicName)
        {
            //idempotent execution: first check to see if the topic already exists
            var topicExists = await CheckIfTopicExists(topicName);

            //if the topic does not exist, create it
            if (!topicExists)
            {
                Console.WriteLine($"Creating the topic {topicName}");
                await _adminClient.CreateTopicAsync(topicName);
            }
        }

        private static async Task<bool> CheckIfTopicExists(string topicName)
        {
            bool topicExists = false;
            try
            {
                //determine if the topic already exists
                var existingTopic = await _adminClient.GetTopicAsync(topicName);
                topicExists = existingTopic != null;
                Console.WriteLine($"Topic {topicName} exists");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not find existing topic {topicName}{System.Environment.NewLine}{ex.Message}");
            }
            return topicExists;
        }

        private static async Task EnsureSubscriptionExists(string topicName, string subscriptionName, RuleFilter ruleFilter)
        {
            //idempotent execution: first check to see if the subscription already exists
            bool subscriptionExists = await CheckIfSubscriptionExists(topicName, subscriptionName);

            //if the subscription does not exist, create it
            if (!subscriptionExists)
            {
                Console.WriteLine($"Creating the subscription {subscriptionName} for the topic {topicName}");

                await _adminClient.CreateSubscriptionAsync(
                        new CreateSubscriptionOptions(topicName, subscriptionName),
                        new CreateRuleOptions(subscriptionName, ruleFilter));
            }
        }

        private static async Task<bool> CheckIfSubscriptionExists(string topic, string subscription)
        { 
            bool subscriptionExists = false;
            try
            {
                var anExistingSubscription = await _adminClient.GetSubscriptionAsync(topic, subscription);
                subscriptionExists = anExistingSubscription is not null;
                Console.WriteLine($"Subscription {subscription} exists: {subscriptionExists}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking for existing subscription {subscription}{System.Environment.NewLine}{ex.Message}");
            }
            return subscriptionExists;
        }
        
        private static void BuildOptions()
        {
            _configuration = ConfigurationBuilderSingleton.ConfigurationRoot;
        }
    }
}
