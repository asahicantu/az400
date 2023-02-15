# 1. Lab 14 Enable Dynamic Configuration and Feature Flags

## Overview

Azure App Configuration provides a service to manage application settings and feature flags centrally. Modern programs, especially those running in a cloud, generally have many distributed components. Spreading configuration settings across these components can lead to hard-to-troubleshoot errors during application deployment. Use App Configuration to store all the settings for your application and secure their accesses in one place.

## Objectives

After you complete this lab, you will be able to:

* Enable dynamic configuration
* Manage feature flags
* Exercise 0: Configure the lab prerequisites
* In this exercise, you will set up the prerequisites for the lab, which consist of a new Azure DevOps project with a repository based on the <https://github.com/MicrosoftLearning/eShopOnWeb>.

## Task 0: Associate your subscription with your project

Navigate to the following URL <https://dev.azure.com> and login as `<username>` and the password `<password>`

1. Click Start free.

2. On the Get started with Azure DevOps page click Continue.

## Task 1: (skip if done) Create and configure the team project

   In this task, you will create an eShopOnWeb Azure DevOps project to be used by several labs.

1. On your lab computer, in a browser window open your Azure DevOps organization. Click on New Project. Give your project the name eShopOnWeb and choose Scrum on the Work Item process dropdown. Click on Create.

2. In the vertical navigational pane of the Azure DevOps portal, select Azure Devops Logo and then click the Organization settings icon.

3. In the navigational pane, select Billing.

4. Under Billing select Set up billing.

5. In the Set up billing pane, once your susbscription is valid in a green box select Save.

6. Set the Paid parallel jobs for MS Hosted CI/CD and Self-Hosted CI/CD to 1.

7. Scroll down to the bottom of the page and click Save then navigate back to your project.

## Task 2: (skip if done) Import eShopOnWeb Git Repository

In this task you will import the eShopOnWeb Git repository that will be used by several labs.

1. On your lab computer, in a browser window open your Azure DevOps organization and the previously created eShopOnWeb project. Click on Repos>Files , Import. On the Import a Git Repository window, paste the following URL <https://github.com/MicrosoftLearning/eShopOnWeb.git> and click on Import:

2. Hover on the main branch then click the ellipsis on the right of the column.

3. Click on Set as default branch

## Exercise 1: (skip if done) Import and run CI/CD Pipelines

    In this exercise, you will import and run the CI pipeline, configure the service connection with your Azure Subscription and then import and run the CD pipeline.

## Task 1: Import and run the CI pipeline

    Let's start by importing the CI pipeline named eshoponweb-ci.yml, <https://github.com/MicrosoftLearning/eShopOnWeb/blob/main/.ado/eshoponweb-ci.yml>.

1. Go to Pipelines>Pipelines

2. Click on Create Pipeline button

3. Select Azure Repos Git (Yaml)

    The repository is organized the following way:

    ```bash
        .ado folder contains Azure DevOps YAML pipelines
        .devcontainer folder container setup to develop using containers (either locally in VS Code or GitHub Codespaces)
        .azure folder contains Bicep&ARM infrastructure as code templates used in some lab scenarios.
        .github folder container YAML GitHub workflow definitions.
        src folder contains the .NET 6 website used on the lab scenarios.
        Task 3: (skip if done) Set main branch as default branch
        Go to Repos>Branches.
    ```

4. Select the eShopOnWeb repository

5. Select Existing Azure Pipelines YAML File

6. Select the /.ado/eshoponweb-cd-aci.yml file then click on Continue

7. Click the Run button to run the pipeline

8. Your pipeline will take a name based on the project name. Let's rename it for identifying the pipeline better. Go to Pipelines>Pipelines and click on the recently created pipeline. Click on the ellipsis and Rename/Remove option. Name it eshoponweb-ci and click on Save.

## Task 2: Manage the service connection

    You can create a connection from Azure Pipelines to external and remote services for executing tasks in a job.

    In this task, you will create a service principal by using the Azure CLI, which will allow Azure DevOps to:

   * Deploy resources on your azure subscription
   * Deploy the eShopOnWeb application

    > Note: If you do already have a service principal, you can proceed directly to the next task.

    You will need a service principal to deploy Azure resources from Azure Pipelines.

    A service principal is automatically created by Azure Pipeline when you connect to an Azure subscription from inside a pipeline definition or when you create a new service connection from the project settings page (automatic option). You can also manually create the service principal from the portal or using Azure CLI and re-use it across projects.

1. From the lab computer, start a web browser, navigate to <https://portal.azure.com>, and sign in with the user account that has the Owner role in the Azure subscription you will be using in this lab and has the role of the Global Administrator in the Azure AD tenant associated with this subscription. <username> / <password>

2. In the Azure portal, click on the Cloud Shell icon, located directly to the right of the search textbox at the top of the page.

3. If prompted to select either Bash or PowerShell, select Bash.

    > Note: If this is the first time you are starting Cloud Shell and you are presented with the You have no storage mounted message, select the subscription you are using in this lab, and select Create storage.

4. From the Bash prompt, in the Cloud Shell pane, run the following commands to retrieve the values of the Azure subscription ID attribute:

    ```bash
        subscriptionName=$(az account show --query name --output tsv)
        subscriptionId=$(az account show --query id --output tsv)
        echo $subscriptionName
        echo $subscriptionId
    ```

    > Note: Copy both values to a text file. You will need them later in this lab.

5. From the Bash prompt, in the Cloud Shell pane, run the following command to create a service principal:

    ```bash
        az ad sp create-for-rbac --name sp-az400-azdo --role contributor --scopes /subscriptions/$subscriptionId
    ```

    > Note: The command will generate a JSON output. Copy the output to text file. You will need it later in this lab.

6. Next, from the lab computer, start a web browser, navigate to the Azure DevOps eShopOnWeb project. Click on Project Settings>Service Connections (under Pipelines) and New Service Connection.

7. On the New service connection blade, select Azure Resource Manager and Next (may need to scroll down).

8. The choose Service principal (manual) and click on Next.

9. Fill in the empty fields using the information gathered during previous steps:
   1. Subscription Id and Name
   2. Service Principal Id (or clientId), Key (or Password) and TenantId.

10. In Service connection name type azure subs. This name will be referenced in YAML pipelines when needing an Azure DevOps Service Connection to communicate with your Azure subscription.

11. Click on Verify and Save.

### Task 3: Import and run the CD pipeline

1. Go to Pipelines>Pipelines

2. Click on New pipeline button

3. Select Azure Repos Git (Yaml)

4. Select the eShopOnWeb repository

5. Select Existing Azure Pipelines YAML File

6. Select the /.ado/eshoponweb-cd-aci.yml file then click on Continue

7. In the YAML pipeline definition, customize:

    YOUR-SUBSCRIPTION-ID with your Azure subscription id.
    az400eshop-NAME replace NAME to make it globally unique.
    AZ400-EWebShop-NAME with the resource group name defined before in the lab.
    Click on Save and Run and wait for the pipeline to execute successfully.
    > Note: The deployment may take a few minutes to complete.
8. The CD definition consists of the following tasks:

    Resources: it is prepared to automatically trigger based on CI pipeline completion. It also downloads the repository for the bicep file.
    AzureResourceManagerTemplateDeployment: Deploys the Azure Web App using bicep template.
    Your pipeline will take a name based on the project name. Let's rename it for identifying the pipeline better. Go to Pipelines>Pipelines and click on the recently created pipeline. Click on the ellipsis and Rename/Remove option. Name it eshoponweb-cd-webapp-code and click on Save.

## Exercise 2: Manage Azure App Configuration

    In this exercise, you will create the App Configuration resource in Azure, enable the managed identity and then test the full solution.

    > Note: This exercise doesn't require any coding skills. The website's code implements already Azure App Configuration functionalities. If you want to know how to implement this in your application, please take a look at these tutorials: Use dynamic configuration in an ASP.NET Core app and Manage feature flags in Azure App Configuration.

## Task 1: Create the App Configuration resource

    In the Azure Portal, search for the App Configuration service

1. Click Create app configuration then select:

    1. Your Azure Subscription

    2. The Resource Group created previously (it should be named AZ400-EWebShop-NAME)

    3. The location

    4. A unique like appcs-NAME-REGION for example

    5. Select the Free pricing tier

    6. Click on Review + create then Create.

2. After creating the App Configuration service, go to Overview and copy/save the value of the Endpoint.

## Task 2: Enable Managed Identity

1. Go to the Web App deployed using the pipeline (it should be named az400-webapp-NAME).

2. In the Settings section, click on Identity then switch status to On in the System Assigned section, click save>yes and wait a few seconds for the operation to finish.

3. Go back to the App Configuration service and click on Access control then Add role assignment.

4. In the Role section, select App Configuration Data Reader

5. In the Members section, check Manage Identity then select the managed identity of your Web App (they should have the same name).

6. Click on Review and assign

## Task 3: Configure the Web App

1. In order to make sure that your website is accessing App Configuration, you need to update its configuration.

2. Go back to your Web App.

3. In the Settings section, click on Configuration.

4. Add two new application settings:

    First app setting
        Name: UseAppConfig
        Value: true
        Second app setting
        
        Name: AppConfigEndpoint
        Value: the value you saved/copied previously from App Configuration Endpoint. It should look like <https://appcs-NAME-REGION.azconfig.io>
5. Click Ok then Save and wait for the settings to be updated.

6. Go to Overview and click on Browse

7. At this step, you will see no changes in the website since the App Configuration doesn't contain any data. This is what you will do in the next tasks.

## Task 4: Test the Configuration Management

    In your website, select Visual Studio in the Brand drop-down list and click on the arrow button (>).

    You will see a message saying "THERE ARE NO RESULTS THAT MATCH YOUR SEARCH". The goal of this Lab is to be able to update that value without updating the website's code or redeploying it.

1. In order to try this, go back to App Configuration.

2. In the Operations section, select Configuration Explorer.

3. Click on Create > Key-value then add:

4. Key: eShopWeb:Settings:NoResultsMessage

5. Value: type your custom message

6. Click Apply then go back to your website and refresh the page.

7. You should see your new message instead of the old default value.

___
Congratulations! In this task, you tested the Configuration explorer in Azure App Configuration.

## Task 5: Test the Feature Flag

    Let's continue to test the Feature manager.

1. In order to try this, go back to App Configuration.

2. In the Operations section, select Feature manager.

3. Click on Create then add:

    1. Enable feature flag: Checked

    2. Feature flag name: SalesWeekend

4. Click Apply then go back to your website and refresh the page.

5. You should see an image with text "ALL T-SHIRTS ON SALE THIS WEEKEND".

6. You can disable this feature in App Configuration and then you would see that the image disappears.

___
Congratulations! In this task, you tested the Feature manager in Azure App Configuration.

## Review

In this lab, you learned how to dynamically enable configuration and manage feature flags.
