# Lab 13 - Integrating Azure Key Vault with Azure DevOps

## Overview

Azure Key Vault provides secure storage and management of sensitive data, such as keys, passwords, and certificates. Azure Key Vault includes supports for hardware security modules, as well as a range of encryption algorithms and key lengths. By using Azure Key Vault, you can minimize the possibility of disclosing sensitive data through source code, which is a common mistake made by developers. Access to Azure Key Vault requires proper authentication and authorization, supporting fine grained permissions to its content.
In this lab, you will see how you can integrate Azure Key Vault with an Azure Pipelines by using the following steps:
create an Azure Key vault to store a ACR password as a secret.
create an Azure Service Principal to provide access to secrets in the Azure Key Vault.
configure permissions to allow the Service Principal to read the secret.
configure pipeline to retrieve the password from the Azure Key Vault and pass it on to subsequent tasks.

## Objectives

After you complete this lab, you will be able to:

- Create an Azure Active Directory (Azure AD) Service Principal.
- Create an Azure Key Vault.

## Exercise 0: Configure the lab prerequisites

In this exercise, you will set up the prerequisites for the lab, which consist of a new Azure DevOps project with a repository based on the <https://github.com/MicrosoftLearning/eShopOnWeb>.

## Task0: Associate your subscription with your project

1. Navigate to the following URL <https://dev.azure.com> and login as asahi_cantu11UNV@gdcs0.com and the password jMI8LYcAJSp97MWT
2. Click Start free.
3. On the Get started with Azure DevOps page click Continue.

## Task1: (skip if done) Create and configure the team project

1. In this task, you will create an eShopOnWeb Azure DevOps project to be used by several labs.
2. In the vertical navigational pane of the Azure DevOps portal, select Azure Devops Logo and then click the Organization settings icon.
3. In the navigational pane, select Billing.
4. Under Billing select Set up billing.
5. In the Set up billing pane, once your susbscription is valid in a green box select Save.
6. Set the Paid parallel jobs for MS Hosted CI/CD and Self-Hosted CI/CD to 1.
7. Scroll down to the bottom of the page and click Save then navigate back to your project.
8. On your lab computer, in a browser window open your Azure DevOps organization. Click on New Project. Give your project the name eShopOnWeb and leave the other fields with defaults. Click on Create.

## Task2: (skip if done) Import eShopOnWeb Git Repository

In this task you will import the eShopOnWeb Git repository that will be used by several labs.

1. On SEA-DEV, Select File Explorer on the ## Taskbar and navigate to G:\ (Allfiles (G:)) Drive. Double click Git-2.3.36.1.exe, when prompted select Yes.
2. Accept the default values for all windows and proceed with the installation. On the succesfully installed window, uncheck the checkbox to view Release Notes then click Finish.
3. On your lab computer, in a browser window open your Azure DevOps organization and the previoulsy created eShopOnWeb project. Click on Repos>Files , Import. On the Import a Git Repository window, paste the following URL <https://github.com/MicrosoftLearning/eShopOnWeb.git> and click on Import:
4. The repository is organized the following way:

    ```bash
    
    .ado folder contains Azure DevOps YAML pipelines
    .devcontainer folder container setup to develop using containers (either locally in VS Code or GitHub Codespaces)
    .azure folder contains Bicep&ARM infrastructure as code templates used in some lab scenarios.
    .github folder container YAML GitHub workflow definitions.
    ```

5. src folder contains the .NET 6 website used on the lab scenarios.

### Exercise 1: Setup CI pipeline to build eShopOnWeb container

1. Setup CI YAML pipeline for:
2. Creating an Azure Container Registry to keep the container images
3. Using Docker Compose to build and push eshoppublicapi and eshopwebmvc container images. Only eshopwebmvc container will be deployed.

## Task1: (skip if done) Create a Service Principal

In this task, you will create a Service Principal by using the Azure CLI, which will allow Azure DevOps to:

1. Deploy resources on your Azure subscription
2. Have read access on the later created Key Vault secrets.
   > Note: If you do already have a Service Principal, you can proceed directly to the next task.
3. You will need a Service Principal to deploy Azure resources from Azure Pipelines. Since we are going to retrieve secrets in a pipeline, we will need to grant permission to the service when we create the Azure Key Vault.
4. A Service Principal is automatically created by Azure Pipelines, when you connect to an Azure subscription from inside a pipeline definition or when you create a new Service Connection from the project settings page (automatic option). You can also manually create the Service Principal from the portal or using Azure CLI and re-use it across projects.
5. From the lab computer, start a web browser, navigate to <https://portal.azure.com>, and sign in with the user account that has the Owner role in the Azure subscription you will be using in this lab and has the role of the Global Administrator in the Azure AD tenant associated with this subscription.
6. In the Azure portal, click on the Cloud Shell icon, located directly to the right of the search textbox at the top of the page.
7. If prompted to select either Bash or PowerShell, select Bash.
    > Note: If this is the first time you are starting Cloud Shell and you are presented with the You have no storage mounted message, select the subscription you are using in this lab, and select Create storage.
8. From the Bash prompt, in the Cloud Shell pane, run the following commands to retrieve the values of the Azure subscription ID and subscription name attributes:

    ```bash
    az account show --query id --output tsv
    az account show --query name --output tsv
    ```

9. Note: Copy both values to a text file. You will need them later in this lab.
10. From the Bash prompt, in the Cloud Shell pane, run the following command to create a Service Principal (replace the myServicePrincipalName with any unique string of characters consisting of letters and digits) and mySubscriptionID with your Azure subscriptionId :

    ```bash
    az ad sp create-for-rbac --name myServicePrincipalName \
    --role contributor \
    --scopes /subscriptions/mySubscriptionID
    ```

    > Note: The command will generate a JSON output. Copy the output to text file. You will need it later in this lab.
11. Next, from the lab computer, start a web browser, navigate to the Azure DevOps eShopOnWeb project. Click on Project Settings>Service Connections (under Pipelines) and New Service Connection.
12. On the New service connection blade, select Azure Resource Manager and Next (may need to scroll down).
13. The choose Service Principal (manual) and click on Next.
14. Fill in the empty fields using the information gathered during previous steps:
15. Subscription Id and Name
16. Service Principal Id (or clientId), Key (or Password) and TenantId.
17. In Service connection name type azure subs. This name will be referenced in YAML pipelines when needing an Azure DevOps Service Connection to communicate with your Azure subscription.
18. Click on Verify and Save.

## Task2: Setup and Run CI pipeline

In this task, you will import an existing CI YAML pipeline definition, modify and run it. It will create a new Azure Container Registry (ACR) and build/publish the eShopOnWeb container images.

1. From the lab computer, start a web browser, navigate to the Azure DevOps eShopOnWeb project. Go to Pipelines>Pipelines and click on Create Pipeline (or New pipeline).
2. On the Where is your code? window, select Azure Repos Git (YAML) and select the eShopOnWeb repository.
3. On the Configure section, choose Existing Azure Pipelines YAML file. Provide the following path /.ado/eshoponweb-ci-dockercompose.yml and click on Continue.
4. In the YAML pipeline definition, customize your Resource Group name by replacing NAME on AZ400-EWebShop-NAME and replace YOUR-SUBSCRIPTION-ID with the your own Azure subscriptionId.
5. Click on Save and Run and wait for the pipeline to execute successfully.
   > Note: The build may take a few minutes to complete. The build definition consists of the following tasks:
6. AzureResourceManagerTemplateDeployment uses bicep to deploy an Azure Container Registry.
7. PowerShell ## Tasktake the bicep output (acr login server) and creates pipeline variable.
8. DockerCompose ## Taskbuilds and pushes the container images for eShopOnWeb to the Azure Container Registry .
9. Your pipeline will take a name based on the project name. Lets rename it for identifying the pipeline better. Go to Pipelines>Pipelines and click on the recently created pipeline. Click on the elipsis and Rename/Remove option. Name it eshoponweb-ci-dockercompose and click on Save.
10. Once the execution is finished, on the Azure Portal, open previously defined Resource Group, and you should find an Azure Container Registry (ACR) with the created container images eshoppublicapi and eshopwebmvc. You will only use eshopwebmvc on the deploy phase.
11. Click on Access Keys and copy the password value, it will be used in the following task, as we will keep it as a secret in Azure Key Vault.

## Task2: Create an Azure Key vault

In this task, you will create an Azure Key vault by using the Azure portal.
For this lab scenario, we will have a Azure Container Instance (ACI) that pull and runs a container image stored in Azure Container Registry (ACR). We intend to store the password for the ACR as a secret in the key vault.
In the Azure portal, in the Search resources, services, and docs text box, type Key vault and press the Enter key.
Select Key vault blade, click on Create>Key Vault.
On the Basics tab of the Create key vault blade, specify the following settings and click on Next:
Setting Value
Subscription CS-SUB-0147
Resource group the name of a new resource group AZ400-EWebShop-NAME
Key vault name any unique valid name, like ewebshop-kv-NAME (replace NAME)
Region an Azure region close to the location of your lab environment
Pricing tier Standard
Days to retain deleted vaults 7
Purge protection Disable purge protection
On the Access policy tab of the Create key vault blade, on the Access Policy section, click on + Create to setup a new policy.
Note: You need to secure access to your key vaults by allowing only authorized applications and users. To access the data from the vault, you will need to provide read (Get/List) permissions to the previously created service principal that you will be using for authentication in the pipeline.
On the Permission blade, check Get and List permissions below Secret Permission. Click on Next.
on the Principal blade, search for the previously created Service Principal, either using the Id or Name given. Click on Next and Next again.
On the Review + create blade, click on Create
Back on the Create a Key Vault blade, click on Review + Create > Create
Note: Wait for the Azure Key vault to be provisioned. This should take less than 1 minute.
On the Your deployment is complete blade, click on Go to resource.
On the Azure Key vault blade, in the vertical menu on the left side of the blade, in the Objects section, click on Secrets.
On the Secrets blade, click on Generate/Import.
On the Create a secret blade, specify the following settings and click on Create (leave others with their default values):
Setting Value
Upload options Manual
Name acr-secret
Value ACR access password copied in previous task

## Task3: Create a Variable Group connected to Azure Key Vault

1. In this task, you will create a Variable Group in Azure DevOps that will retrieve the ACR password secret from Key Vault using the Service Connection (Service Principal)
2. On your lab computer, start a web browser and navigate to the Azure DevOps project eShopOnWeb.
3. In the vertical navigational pane of the of the Azure DevOps portal, select Pipelines>Library. Click on + Variable Group.
4. On the New variable group blade, specify the following settings:
    | Setting                    | Value                                             |
    |----------------------------|---------------------------------------------------|
    | Variable Group Name        | eshopweb-vg                                       |
    | Link secrets from Azure KV | ... enable                                        |
    | Azure subscription         | Available Azure service connection > Azure subs   |
    | Key vault name             | Your key vault name                               |
5. Under Variables, click on + Add and select the acr-secret secret. Click on OK.
6. Click on Save.

## Task4: Setup CD Pipeline to deploy container in Azure Container Instance(ACI)

In this task, you will import a CD pipeline, customize it and run it for deploying the container image created before in a Azure Container Instance.

1. From the lab computer, start a web browser, navigate to the Azure DevOps eShopOnWeb project. Go to Pipelines>Pipelines and click on New Pipeline.
2. On the Where is your code? window, select Azure Repos Git (YAML) and select the eShopOnWeb repository.
3. On the Configure section, choose Existing Azure Pipelines YAML file. Provide the following path /.ado/eshoponweb-cd-aci.yml and click on Continue.
4. In the YAML pipeline definition, customize:
   1. YOUR-SUBSCRIPTION-ID: cbbc558e-f23f-4d57-a253-1adc3569dfea.
   2. az400eshop-NAME replace NAME to make it globally unique.
   3. YOUR-ACR.azurecr.io and ACR-USERNAME with your ACR login server (both need the ACR name, can be reviewed on the ACR>Access Keys).
   4. AZ400-EWebShop-NAME with the resource group name defined before in the lab.
5. Click on Save and Run and wait for the pipeline to execute successfully.
   > Note: The deployment may take a few minutes to complete. The CD definition consists of the following tasks:
6. Resources : it is prepared to automatically trigger based on CI pipeline completion. It also download the repository for the bicep file.
7. Variables (for Deploy stage) connects to the variable group to consume the Azure Key Vault secret acr-secret
8. AzureResourceManagerTemplateDeployment deploys the Azure Container Instance (ACI) using bicep template and provides the ACR login parameters to allow ACI to download the previously created container image from Azure Container Registry (ACR).
9. Your pipeline will take a name based on the project name. Lets rename it for identifying the pipeline better. Go to Pipelines>Pipelines and click on the recently created pipeline. Click on the ellipsis and Rename/Remove option. Name it eshoponweb-cd-aci and click on Save.

## Review

In this lab, you integrated Azure Key Vault with an Azure DevOps pipeline by using the following steps:
Created an Azure service principal to provide access to secrets in the Azure Key vault and authenticate deployment to Azure from Azure DevOps.
Run 2 YAML pipelines imported from a Git repository.
Configured pipeline to retrieve the password from the Azure Key vault using ADO Variable Group and use it on subsequent tasks.
