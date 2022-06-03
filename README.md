# Serverless Solutions for messaging and handling events at Azure

In this repo you'll find the code and references for my talk on Serverless Messaging Solutions demystified.

When you first start working with Azure serverless solutions, it can be tricky to determine which solution is the best for messaging and event handling.  It can even be confusing as to what the Event Hub and Event Grid are and why you need both and why sometimes they are referenced by each other.

Hopefully, this talk will help to clear these things up, as well as which solution to use for messaging and queuing in various scenarios.

## Event Hub

The Azure Event Hub is your choice for Big Data stream ingestion into Azure.  Along with Event Hub, you have the IoT Hub, which is a subset of the Event Hub. If you have a stream of data coming in at hundreds or thousands of records per second, the Event Hub is your stream ingestion tool of choice.

To create an event hub, you need a namespace.  You then place one or more hubs in the namespace.  The namespace determines the overall throughput and the hub determines how the data is separated and how many individual applications can read their own version of the data.

Most importantly, the talk helps to shed light on the information in this table:

[Comparison of services](https://docs.microsoft.com/en-us/azure/event-grid/compare-messaging-services#comparison-of-services?WT.mc_id=AZ-MVP-5004334)

References for Event Hub and the Event Hub Namespace from this talk include:

- [FAQ - What is an Event Hub Namespace](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-faq?WT.mc_id=AZ-MVP-5004334)  
- [What is Azure Event Hub](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-about?WT.mc_id=AZ-MVP-5004334)  
- [Event Hub Terminology](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-features?WT.mc_id=AZ-MVP-5004334)  
- [Capture events to Azure Storage](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-capture-overview?WT.mc_id=AZ-MVP-5004334)  
- [Use .Net to send and receive events](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-dotnet-standard-getstarted-send?WT.mc_id=AZ-MVP-5004334)

## Event Grid

The Azure event grid is the tool of choice when events are fired at Azure and you need to respond.  Common examples are storage blobs created or virtual machines restarted.  Essentially, everything you do in Azure is monitored, and if you want to subscribe to the event you can likely make it happen.

In some cases you will also choose to write your own custom events.  This is a bit trickier, but you can utilize the same data payload structure as a typical azure event to also create an event topic and subscription(s) to your custom events.

- [What is Azure Event Grid](https://docs.microsoft.com/azure/event-grid/overview?WT.mc_id=AZ-MVP-5004334)  
- [How to filter events with event grid](https://docs.microsoft.com/en-us/azure/event-grid/how-to-filter-events?WT.mc_id=AZ-MVP-5004334)  
- [Event Grid message delivery and retry](https://docs.microsoft.com/en-us/azure/event-grid/delivery-and-retry?WT.mc_id=AZ-MVP-5004334)  
- [Dead Letter and Retry Policies](https://docs.microsoft.com/en-us/azure/event-grid/manage-event-delivery?WT.mc_id=AZ-MVP-5004334)  

## Service Bus

Service Bus has two types of use in Azure.  The first is simple pub/sub for messaging.  This is much easier than a custom event to implement, so if you just need to send information from one application to another in a disconnected fashion, you should likely choose service bus.  

Service bus topics can then be subscribed to from one or more consumers.  Each consumer can further filter the data they care about.  For example, you could have three apps all responding to blob storage, but each could be filtering for different things like a specific type of image or any file with a common prefix.  Finally, you could even filter to something like the container name or a custom subject that is included with the message.

Service Bus Queues are the tool of choice when you need a guaranteed FIFO object at azure to ensure message processing is handled in the order the messages are received.  Once a queue is created, any application with the correct SAS for listening can consume messages from the Queue.  Any application with the SAS for writing can publish messages to the queue.  

- []()
- []()
- []()
- []()
- []()

## Azure Storage Queue

The Azure storage queue is used to read and write messages similar to the service bus queue.  In storage queue, order cannot be guaranteed.  The messages in a storage queue are smaller than the messages in service bus, but storage queue can store millions of messages and they are automatically purged after 7 days.