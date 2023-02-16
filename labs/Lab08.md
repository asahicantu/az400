# Lab 08 - Deploying Docker containers to Azure App Service web apps

## Overview

In this lab, you will learn how to use an Azure DevOps CI/CD pipeline to build a custom Docker image, push it to Azure Container Registry, and deploy it as a container to Azure App Service.

## Objectives

After you complete this lab, you will be able to:

- Build a custom Docker image by using a Microsoft hosted Linux agent.
- Push an image to Azure Container Registry.
- Deploy a Docker image as a container to Azure App Service by using Azure DevOps.

## Exercise 0: Configure the lab prerequisites

In this exercise, you will set up the prerequisites for the lab, which consist of a new Azure DevOps project with a repository based on the <https://github.com/MicrosoftLearning/eShopOnWeb>.

### Task 0: Associate your subscription with your project

1. Navigate to the following URL <https://dev.azure.com> and login as asahi_cantuQO4MU@gdcs0.com and the password oHWsD4IUONhlb7NH
2. Click Start free.
3. On the Get started with Azure DevOps page click Continue.
4. In the vertical navigational pane of the Azure DevOps portal, select Azure Devops Logo and then click the Organization settings icon.
5. In the navigational pane, select Billing.
6. Under Billing select Set up billing.
7. In the Set up billing pane, once your susbscription is valid in a green box select Save.
8. Set the Paid parallel jobs for MS Hosted CI/CD and Self-Hosted CI/CD to 1.
9. Scroll down to the bottom of the page and click Save then navigate back to your project.

### Task 1: (skip if done) Create and configure the team project

In this task, you will create an eShopOnWeb Azure DevOps project to be used by several labs.
On your lab computer, in a browser window open your Azure DevOps organization. Click on New Project. Give your project the name eShopOnWeb and choose Scrum on the Work Item process dropdown. Click on Create.

### Task 2: (skip if done) Import eShopOnWeb Git Repository

In this task you will import the eShopOnWeb Git repository that will be used by several labs.
On your lab computer, in a browser window open your Azure DevOps organization and the previoulsy created eShopOnWeb project. Click on Repos>Files , Import. On the Import a Git Repository window, paste the following URL <https://github.com/MicrosoftLearning/eShopOnWeb.git> and click on Import:
The repository is organized the following way:

```bash
.ado folder contains Azure DevOps YAML pipelines
.devcontainer folder container setup to develop using containers (either locally in VS Code or GitHub Codespaces)
.azure folder contains Bicep&ARM infrastructure as code templates used in some lab scenarios.
.github folder container YAML GitHub workflow definitions.
```

`src` folder contains the .NET 6 website used on the lab scenarios.

### Task 3: (skip if done) Set main branch as default branch

Go to Repos>Branches
Hover on the main branch then click the ellipsis on the right of the column
Click on Set as default branch

## Exercise 1: Manage the service connection

In this exercise, you will configure the service connection with your Azure Subscription then import and run the CI pipeline.

### Task 1: (skip if done) Manage the service connection

1. You can create a connection from Azure Pipelines to external and remote services for executing tasks in a job.
2. In this task, you will create a service principal by using the Azure CLI, which will allow Azure DevOps to:
3. Deploy resources on your azure subscription
4. Push the docker image to Azure Container Registry
5. Add a role assignment to allow Azure App Service pull the docker image from Azure Container Registry
   > Note: If you do already have a service principal, you can proceed directly to the next task.
6. You will need a service principal to deploy Azure resources from Azure Pipelines.
7. A service principal is automatically created by Azure Pipeline when you connect to an Azure subscription from inside a pipeline definition or when you create a new service connection from the project settings page (automatic option). You can also manually create the service principal from the portal or using Azure CLI and re-use it across projects.
8. From the lab computer, start a web browser, navigate to the <https://portal.azure.com>, and sign in with the user account that has the Owner role in the Azure subscription you will be using in this lab and has the role of the Global Administrator in the Azure AD tenant associated with this subscription.
9. In the Azure portal, click on the Cloud Shell icon, located directly to the right of the search textbox at the top of the page.
10. If prompted to select either Bash or PowerShell, select Bash.
    > Note: If this is the first time you are starting Cloud Shell and you are presented with the You have no storage mounted message, select the subscription you are using in this lab, and select Create storage.
11. From the Bash prompt, in the Cloud Shell pane, run the following commands to retrieve the values of the Azure subscription ID attribute:
12. subscriptionName=$(az account show --query name --output tsv)
13. subscriptionId=$(az account show --query id --output tsv)
14. echo $subscriptionName
15. echo $subscriptionId
    > Note: Copy both values to a text file. You will need them later in this lab.
16. From the Bash prompt, in the Cloud Shell pane, run the following command to create a service principal:
17. az ad sp create-for-rbac --name sp-az400-azdo --role contributor --scopes /subscriptions/$subscriptionId
    > Note: The command will generate a JSON output. Copy the output to text file. You will need it later in this lab.
18. Next, from the lab computer, start a web browser, navigate to the Azure DevOps eShopOnWeb project. Click on Project Settings>Service Connections (under Pipelines) and New Service Connection.
19. On the New service connection blade, select Azure Resource Manager and Next (may need to scroll down).
20. The choose Service principal (manual) and click on Next.
21. Fill in the empty fields using the information gathered during previous steps:
22. Subscription Id and Name
23. Service Principal Id (or clientId), Key (or Password) and TenantId.
24. In Service connection name type azure-connection. This name will be referenced in YAML pipelines when needing an Azure DevOps Service Connection to communicate with your Azure subscription.
25. Click on Verify and Save.

## Exercise 2: Import and run the CI pipeline

In this exercise, you will import and run the CI pipeline.

### Task 1: Import and run the CI pipeline

1. Go to Pipelines>Pipelines
2. Click on New pipeline button
3. Select Azure Repos Git (Yaml)
4. Select the eShopOnWeb repository
5. Select Existing Azure Pipelines YAML File
6. Select the /.ado/eshoponweb-ci-docker.yml file then click on Continue
7. In the YAML pipeline definition, customize:
8. YOUR-SUBSCRIPTION-ID: a4d7c77c-c419-4cb3-acde-1fe82ca17cee
9. rg-az400-container-NAME with the resource group name defined before in the lab.
10. Click on Save and Run and wait for the pipeline to execute succesfully.
    > Note: The deployment may take a few minutes to complete.
11. The CI definition consists of the following tasks:
12. Resources: It downloads the repository filest will be used in the followinf tasks.
13. AzureResourceManagerTemplateDeployment: Deploys the Azure Container Registry using bicep template.
14. PowerShell: Retrieve the ACR Login Server value from the previous task's output and create a new parameter acrLoginServer
15. Docker - Build: Build the docker image and create two tags (Latest and current BuildID)
16. Docker - Push: Push the images to Azure Container Registry
17. Your pipeline will take a name based on the project name. Let's rename it for identifying the pipeline better. Go to Pipelines>Pipelines and click on the recently created pipeline. Click on the ellipsis and Rename/move option. Name it eshoponweb-ci-docker and click on Save.
18. Navigate to the <https://portal.azure.com>, search for the Azure Container Registry in the recently created Resource Group (it should be named rg-az400-container-NAME). Make sure that the eshoponweb/web was created and contains two tags (one of them is Latest).

## Exercise 3: Import and run the CD pipeline

In this exercise, you will configure the service connection with your Azure Subscription then import and run the CD pipeline.

### Task 1: Add a new role assignment

In this task, you will add a new role assignment to allow Azure App Service pull the docker image from Azure Container Registry.

1. Navigate to <https://portal.azure.com> and login using asahi_cantuQO4MU@gdcs0.com and password oHWsD4IUONhlb7NH.
2. In the Azure portal, click on the Cloud Shell icon, located directly to the right of the search textbox at the top of the page.
3. If prompted to select either Bash or PowerShell, select Bash.
4. From the Bash prompt, in the Cloud Shell pane, run the following commands to retrieve the values of the Azure subscription ID attribute:

    ```bash
        spId=$(az ad sp list --display-name sp-az400-azdo --query "[].id" --output tsv)
        echo $spId
        roleName=$(az role definition list --name "User Access Administrator" --query [0].name --output tsv)
        echo $roleName
    ```

5. After getting the service principal ID and the role name, let's create the role assignment by running this command (replace rg-az400-container-NAME with your resource group name)

    ```bash
    az role assignment create --assignee $spId --role $roleName --resource-group "rg-az400-container-NAME"
    ```

6. You should now see the JSON output which confirms the success of the command run.

### Task 2: Import and run the CD pipeline

1. In this task, you will import and run the CI pipeline.
2. Go to Pipelines>Pipelines
3. Click on New pipeline button
4. Select Azure Repos Git (Yaml)
5. Select the eShopOnWeb repository
6. Select Existing Azure Pipelines YAML File
7. Select the /.ado/eshoponweb-cd-webapp-docker.yml file then click on Continue
8. In the YAML pipeline definition, customize:
9. YOUR-SUBSCRIPTION-ID: a4d7c77c-c419-4cb3-acde-1fe82ca17cee
10. rg-az400-container-NAME with the resource group name defined before in the lab.
11. Click on Save and Run and wait for the pipeline to execute succesfully.
    > Note: The deployment may take a few minutes to complete.
12. The CI definition consists of the following tasks:
13. Resources: It downloads the repository files that will be used in the following tasks.
14. AzureResourceManagerTemplateDeployment: Deploys the Azure App Service using bicep template.
15. AzureResourceManagerTemplateDeployment: Add role assignment using Bicep
16. Your pipeline will take a name based on the project name. Let's rename it for identifying the pipeline better. Go to Pipelines>Pipelines and hover on the recently created pipeline. Click on the ellipsis and Rename/move option. Name it eshoponweb-cd-webapp-docker and click on Save.
    > Note 1: The use of the /.azure/bicep/webapp-docker.bicep template creates an app service plan, a web app with system assigned managed identity enabled, and references the docker image pushed previously: ${acr.properties.loginServer}/eshoponweb/web:latest.
    > Note 2: The use of the /.azure/bicep/webapp-to-acr-roleassignment.bicep template creates a new role assignment for the web app with AcrPull role to be able to retreive the docker image. This could be done in the first template, but since the role assignment can take some time to propagate, it's a good idea to do both tasks separately.

### Task 3: Test the solution

1. In the Azure Portal, navigate to the recently created Resource Group, you should now see three resources (Ap Service, App Service Plan and Container Registry).
2. Navigate to the App Service, then click Browse, this will take you to the website.
3. Congratulations! In this exercise, you deployed a website using custom docker image.

## Review

> In this lab, you learned how to use an Azure DevOps CI/CD pipeline to build a custom Docker image, push it to Azure Container Registry, and deploy it as a container to Azure App Service.
