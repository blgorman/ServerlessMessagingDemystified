@description('Specifies a project name that is used to generate the Event Hub name and the Namespace name.')
param projectName string = 'myorglogging'

@description('Name of the SErvice Bus Queue')
param serviceBusQueueName string = 'FileProcessingQueue'

@description('Name of the Topic')
param serviceBusTopicName string = 'MoviesToReview'

// @description('Name of the Subscription')
// param serviceBusSubscriptionAllMovies string = 'AllMovies'
// param serviceBusSubscriptionFamilyMovies string = 'AllFamilyMovies'
// param serviceBusSubscriptionAdultMovies string = 'AllAdultMovies'

// @description('Name of the Rule')
// param serviceBusRuleAllMovies string = 'AllMoviesRule'
// param serviceBusRuleFamilyMovies string = 'AllFamilyMoviesRule'
// param serviceBusRuleAdultMovies string = 'AllAdultMoviesRule'

@description('Location for all resources.')
param location string = resourceGroup().location

var serviceBusNamespaceName = substring('${projectName}sbns${uniqueString(resourceGroup().id)}', 0, 24)

resource serviceBusNamespaceResource 'Microsoft.ServiceBus/namespaces@2018-01-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {
  }
}

resource serviceBusQueueResource 'Microsoft.ServiceBus/namespaces/queues@2022-01-01-preview' = {
  parent: serviceBusNamespaceResource
  name: serviceBusQueueName
  properties: {
    lockDuration: 'PT5M'
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: false
    requiresSession: false
    defaultMessageTimeToLive: 'P10675199DT2H48M5.4775807S'
    deadLetteringOnMessageExpiration: false
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    maxDeliveryCount: 10
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    enablePartitioning: false
    enableExpress: false
  }
}

resource serviceBusTopicResource 'Microsoft.ServiceBus/namespaces/topics@2017-04-01' = {
  parent: serviceBusNamespaceResource
  name: serviceBusTopicName
  properties: {
    defaultMessageTimeToLive: 'P10675199DT2H48M5.4775807S'
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: false
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enableBatchedOperations: true
    supportOrdering: true
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    enablePartitioning: false
    enableExpress: false
  }
}

// resource serviceBusTopicAllMoviesSubscriptionResource 'Microsoft.ServiceBus/namespaces/topics/Subscriptions@2017-04-01' = {
//   parent: serviceBusTopicResource
//   name: serviceBusSubscriptionAllMovies
//   properties: {
//     lockDuration: 'PT1M'
//     requiresSession: false
//     defaultMessageTimeToLive: 'P10675199DT2H48M5.4775807S'
//     deadLetteringOnMessageExpiration: false
//     maxDeliveryCount: 10
//     enableBatchedOperations: true
//     autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
//   }
// }

// resource serviceBusRuleResource 'Microsoft.ServiceBus/namespaces/topics/Subscriptions/Rules@2017-04-01' = {
//   parent: serviceBusTopicAllMoviesSubscriptionResource
//   name: serviceBusRuleAllMovies
//   properties: {
    
    
//   }
// }
