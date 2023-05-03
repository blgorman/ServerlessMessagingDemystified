@description('Storage account location')
param location string = resourceGroup().location

@minLength(3)
@maxLength(16)
@description('Provide a unique name for the event hub storage account. Use only lower case letters and numbers, at least 3 and less than 17 chars')
param storageName string = 'messagingstorage'
var storageAccountName = substring('${storageName}${uniqueString(resourceGroup().id)}', 0, 24)

@minLength(3)
@maxLength(16)
@description('Provide a unique name for the event grid storage account. Use only lower case letters and numbers, at least 3 and less than 17 chars')
param storageNameEvents string = 'eventstorage'
var storageAccountNameEvents = substring('${storageNameEvents}${uniqueString(resourceGroup().id)}', 0, 24)


@description('Storage account sku')
@allowed([
  'Standard_LRS'
  'Standard_GRS'
  'Standard_RAGRS'
  'Standard_ZRS'
  'Premium_LRS'
  'Premium_ZRS'
  'Standard_GZRS'
  'Standard_RAGZRS'
])
param storageSku string = 'Standard_LRS'

@description('Allow blob public access')
param allowBlobPublicAccess bool = false

@description('Allow shared key access')
param allowSharedKeyAccess bool = true

@description('Storage account access tier, Hot for frequently accessed data or Cool for infreqently accessed data')
@allowed([
  'Hot'
  'Cool'
])
param storageTier string = 'Hot'

@description('Enable blob encryption at rest')
param blobEncryptionEnabled bool = true

@description('Enable file encryption at rest')
param fileEncryptionEnabled bool = true

@description('Enable Blob Retention')
param enableBlobRetention bool = false

@description('Number of days to retain blobs')
param blobRetentionDays int = 7

@description('Blob Container name for Event Hub Capture.')
param captureEventsBlobContainerName string = 'capturedehevents'

@description('Blob Container name for cool storage')
param coolStorageBlobContainerName string = 'serverlesscoolpath'

@description('Enable Hierarchical Namespace')
param enableHierarchicalNamespace bool = true

@description('Enable Immutability Policy')
param enableImmutabilityPolicy bool = false

@description('Event Grid storage account container name')
param eventTriggerContainerName string = 'uploads'

@description('The storage account.  Toggle the public access to true if you want public blobs on the account in any containers')
resource storageaccount 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: storageSku
  }
  properties: {
    allowBlobPublicAccess: allowBlobPublicAccess
    accessTier: storageTier
    allowSharedKeyAccess: allowSharedKeyAccess
    isHnsEnabled: enableHierarchicalNamespace
    immutabilityPolicy: {
      enabled: enableImmutabilityPolicy
    }
    encryption: {
      keySource: 'Microsoft.Storage'
      services: {
        blob: {
          enabled: blobEncryptionEnabled
        }
        file: {
          enabled: fileEncryptionEnabled
        }
      }
    }
  }
}

resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2021-04-01' = {
  parent: storageaccount
  name: 'default'
  properties: {
    deleteRetentionPolicy: {
      enabled: enableBlobRetention
      days: blobRetentionDays
    }
  }
}

//Note: the code makes the queue if it doesn't exist, but it doesn't set the CORS rules.  If you need CORS rules, uncomment the code below and comment out the code in the project.
// resource queueServices 'Microsoft.Storage/storageAccounts/queueServices@2022-09-01' = {
//   name: 'default'
//   parent: storageaccount
//   properties: {
//     cors: {
//       corsRules: [
//         {
//           allowedOrigins: [
//             '*'
//           ]
//           allowedMethods: [
//             'GET'
//             'POST'
//             'PUT'
//             'DELETE'
//             'HEAD'
//             'OPTIONS'
//           ]
//           allowedHeaders: [
//             '*'
//           ]
//           exposedHeaders: [
//             '*'
//           ]
//           maxAgeInSeconds: 86400
//         }
//       ]
//     }
//   }
// }

// Create the capture container
resource capturedEventsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  name: captureEventsBlobContainerName
  parent: blobServices
  properties: {
    metadata: {}
    publicAccess: 'None'
  }
}

// Create the cool storage container
resource coolStorageContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  name: coolStorageBlobContainerName
  parent: blobServices
  properties: {
    metadata: {}
    publicAccess: 'None'
  }
}

// //Create a storage queue named "messagingqueue" [must also have the queueServices resource uncommented]
// resource storageQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2021-04-01' = {
//   name: 'messagingqueue'
//   parent: queueServices
//   properties: {
//     metadata: {}
//   }
// }

@description('The storage account.  Toggle the public access to true if you want public blobs on the account in any containers')
resource storageaccount2 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: storageAccountNameEvents
  location: location
  kind: 'StorageV2'
  sku: {
    name: storageSku
  }
  properties: {
    allowBlobPublicAccess: allowBlobPublicAccess
    accessTier: storageTier
    allowSharedKeyAccess: allowSharedKeyAccess
    isHnsEnabled: enableHierarchicalNamespace
    immutabilityPolicy: {
      enabled: enableImmutabilityPolicy
    }
    encryption: {
      keySource: 'Microsoft.Storage'
      services: {
        blob: {
          enabled: blobEncryptionEnabled
        }
        file: {
          enabled: fileEncryptionEnabled
        }
      }
    }
  }
}

resource blobServicesEvents 'Microsoft.Storage/storageAccounts/blobServices@2021-04-01' = {
  parent: storageaccount2
  name: 'default'
  properties: {
    deleteRetentionPolicy: {
      enabled: enableBlobRetention
      days: blobRetentionDays
    }
  }
}

// Create the cool storage container
resource eventTriggerContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  name: eventTriggerContainerName
  parent: blobServicesEvents
  properties: {
    metadata: {}
    publicAccess: 'None'
  }
}

output storageAccountFullName string = storageAccountName
output captureBlobEventsContainerName string = captureEventsBlobContainerName
output storageAccountEventsFullname string = storageAccountNameEvents
