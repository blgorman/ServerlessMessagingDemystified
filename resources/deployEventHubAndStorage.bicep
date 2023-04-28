@description('Storage account location')
param location string = resourceGroup().location

@minLength(3)
@maxLength(16)
@description('Provide a unique name for the storage account. Use only lower case letters and numbers, at least 3 and less than 17 chars')
param storageName string = 'messagingstorage'

@description('Blob Container name for Event Hub Capture.')
param captureEventsBlobContainerName string = 'capturedehevents'

@description('Blob Container name for cool storage')
param coolStorageBlobContainerName string = 'serverlesscoolpath'

@description('Specifies a project name that is used to generate the Event Hub name and the Namespace name.')
param eventHubProjectName string = 'myorglogging'

module deployStorage 'storage.bicep' = {
  name: 'deployStorage'
  params: {
    storageName: storageName
    captureEventsBlobContainerName: captureEventsBlobContainerName
    coolStorageBlobContainerName: coolStorageBlobContainerName
  }
}

var storageAccountName = deployStorage.outputs.storageAccountFullName
var captureBlobContainerName = deployStorage.outputs.captureBlobEventsContainerName

@description('Deploy the Event Hub with capture to the storage account container')
module deployEventHub 'eventHub.bicep' = {
  name: 'deployEventHub'
  params: {
    location: location
    projectName: eventHubProjectName
    captureBlobContainerName: captureBlobContainerName
    storageName: storageAccountName
    eventHubSku: 'Standard'
  }
}
