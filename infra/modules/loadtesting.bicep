param projectName string
param environment string
param location string = resourceGroup().location

@description('Azure Load Testing resource for performance tests')
resource loadTesting 'Microsoft.LoadTestService/loadtests@2023-04-01' = {
  name: 'lt-${projectName}-${environment}'
  location: location
}

output loadTestingName string = loadTesting.name
output loadTestingId string = loadTesting.id
