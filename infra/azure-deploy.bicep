@description('Location for all resources')
param location string = resourceGroup().location

@description('SQL admin username')
param sqlAdmin string
@description('SQL admin password')
@secure()
param sqlAdminPassword string

@description('Service Bus namespace name')
param serviceBusName string = 'dotnet9-sb-${uniqueString(resourceGroup().id)}'

resource sb 'Microsoft.ServiceBus/namespaces@2021-06-01-preview' = {
  name: serviceBusName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {}
}

resource sqlServer 'Microsoft.Sql/servers@2022-11-01-preview' = {
  name: 'dotnet9sql${uniqueString(resourceGroup().id)}'
  location: location
  properties: {
    administratorLogin: sqlAdmin
    administratorLoginPassword: sqlAdminPassword
  }
  sku: {
    name: 'GP_S_Gen5_2'
    tier: 'GeneralPurpose'
  }
}

resource sqlDb 'Microsoft.Sql/servers/databases@2021-02-01-preview' = {
  parent: sqlServer
  name: 'dotnet9db'
  sku: {
    name: 'GP_Gen5_2'
  }
  properties: {}
}

output serviceBusNamespace string = sb.name
output sqlServerName string = sqlServer.name
output sqlDatabaseName string = sqlDb.name
