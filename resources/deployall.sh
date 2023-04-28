#note: ensure you are on the correct subscription and create a resource group#
rg='rgServerlessMessagingDemystified'
loc='eastus'
az group create --name $rg --location $loc
#Deploy all resources#
filePath='allMessagingResources.bicep'
#note: You may wish to override some of the params#
az deployment group create --resource-group $rg --template-file $filePath
