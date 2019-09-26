# SSW.Consulting DevOps

The following sections describe what is necessary to deploy the ARM templates to Azure for your own development and testing purposes without contending with other developers over shared resources.

NOTE: These scripts currently only deploy the AZURE RESOURCES required by the applicatiun, not the CODE!

## Setting up a Service Principle so you can deploy to your own Azure Subscription

In order to quickly and easily deploy ARM templates from your local machine to your personal subscription you need to have a Service Princple created that the Azure CLI can use to authenticate itself.

If you don't have an SP already created, perform the following steps:

1. Open a Powershell terminal (I like the new Windows Terminal)
1. Run `az login` and sign into your personal Azure account
1. Run the `setup-servicePrinciple.ps1` script and supply the `subscriptionId` parameter
   * you can also manually enter your Azure subscription ID whgen prompted
1. The script will create an Azure AD App called `AzureDeploySP` (see AppRegistrations) and configure a Service Principle for the app
1. Once the SP has been created and configured, the `azure.json` file will be created that holds all the necessary info for the Azure CLI to login and deploy your templates
   * *DO NOT COMMIT THIS FILE TO SOURCE CONTROL*

## Deploying to Azure

To deploy the templates from you local machine to your personal subscription, follow these steps:

1. Open a Powershell terminal
1. Run the `deploy-local.ps1` script
    * This script will use the `infrastructure.parameters.local.json` file by default
    * You will need to create your own `infrastructure.parameters.local.json` file by duplicating the existing .dev verion and filling in your own values
1. Wait for the deployment to complete
1. Go to [Azure Portal](portal.azure.com), find your resource group, and verify that the resources have been created

## What are all the files
|Filename|Description|
|-|-|
|`setup-servicePrinciple.ps1` | Create an Azure AD App with Service Principle so that you can deploy the resources from your local machine to Azure. |
|`azure.json`| This is a generated file that holds the subscription ID, tenant ID, service principle ID, and password necessary to authenticate the Azure CLI against your account before deploying resource. |
|`infrastructure.json`| This is the ARM template file containing the definition of all the Azure resources (webapp, database, storage, etc.) required for this project. |
|`deploy-local.ps1`| Deploy the infrastructure ARM template using your *local* parameters file (`infrastructure.parameters.local.json`). |
|`deploy.ps1`| Performs logging into Azure for you and then deployes the `infrastructure.json` ARM template. |
|`deploy-template.ps1`| This file contains the helper code to create a resource group and deploy a specific ARM template and parameters file. |
|`deploy-keyvault.ps1`| This file takes care of creating/updating an Azure KeyVault, adds the secrets from the recent deployment to the KeyVault, and sets appropriate access policies. |


