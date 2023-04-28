#note: ensure you are on the correct subscription and create a resource group#
rg='rgServerlessMessagingDemystified'
loc='eastus'
az group create --name $rg --location $loc
#deploy event hub #
filePath='eventHub.bicep'
az deployment group create --resource-group $rg --template-file $filePath
