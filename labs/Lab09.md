# Lab 09 - Controlling Deployments using Release Gates

## Overview

This lab covers the configuration of the deployment gates and details how to use them to control the execution of Azure Pipelines. To illustrate their implementation, you'll configure a release definition with two environments for an Azure Web App. You'll deploy to the Canary environment only when there are no blocking bugs for the app and mark the Canary environment complete only when there are no active alerts in Application Insights of Azure Monitor.
A release pipeline specifies the end-to-end release process for an application to be deployed across various environments. Deployments to each environment are fully automated by using jobs and tasks. Ideally, you don't want new updates to the applications to be simultaneously exposed to all the users. It's a best practice to expose updates in a phased manner, that is, expose them to a subset of users, monitor their usage, and expose them to other users based on the experience of the initial set of users.
Approvals and gates enable you to take control over the start and completion of the deployments in a release. You can wait for users to approve or reject deployments with approvals manually. Using release gates, you can specify application health criteria to be met before the release is promoted to the following environment. Before or after any environment deployment, all the specified gates are automatically evaluated until they pass or reach your defined timeout period and fail.
Gates can be added to an environment in the release definition from the pre-deployment conditions or the post-deployment conditions panel. Multiple gates can be added to the environment conditions to ensure all the inputs are successful for the release.

As an example:

Pre-deployment gates ensure no active issues in the work item or problem management system before deploying a build to an environment.
Post-deployment gates ensure no incidents from the app's monitoring or incident management system after being deployed before promoting the release to the following environment.
There are 4 types of gates included by default in every account.

* Invoke Azure Function: Trigger the execution of an Azure Function and ensures a successful completion.
* Query Azure Monitor alerts: Observe the configured Azure Monitor alert rules for active alerts.
* Invoke REST API: Make a call to a REST API and continues if it returns a successful response.
* Query work items: Ensure the number of matching work items returned from a query is within a threshold.

## Objectives

- After you complete this lab, you will be able to:
- Configure release pipelines.
- Configure release gates.
- Test release gates.

## Exercise 0: Configure the lab prerequisites

In this exercise, you will set up the prerequisites for the lab, which consist of a new Azure DevOps project with a repository based on the <https://github.com/MicrosoftLearning/eShopOnWeb>.

### Task 0: Associate your subscription with your project

1. Navigate to the following URL <https://dev.azure.com> and login as [usr] and the password [psw]
2. Click Start free.
3. On the Get started with Azure DevOps page click Continue.

### Task 1: (skip if done) Create and configure the team project

In this task, you will create an eShopOnWeb_ReleaseGates Azure DevOps project to be used by several labs.

1. On your lab computer, in a browser window open your Azure DevOps organization. Click on New Project. Give your project the name eShopOnWeb_ReleaseGates and leave the other fields with defaults. Click on Create.

### Task 2: (skip if done) Import eShopOnWeb Git Repository

In this task you will import the eShopOnWeb Git repository that will be used by several labs.
On your lab computer, in a browser window open your Azure DevOps organization and the previously created eShopOnWeb_ReleaseGates project. Click on Repos>Files , Import a Repository. Select Import. On the Import a Git Repository window, paste the following URL <https://github.com/MicrosoftLearning/eShopOnWeb.git> and click Import:
The repository is organized the following way:

```bash
.ado folder contains Azure DevOps YAML pipelines
.devcontainer folder container setup to develop using containers (either locally in VS Code or GitHub Codespaces)
.azure folder contains Bicep&ARM infrastructure as code templates used in some lab scenarios.
.github folder container YAML GitHub workflow definitions.
```

src folder contains the .NET 6 website used on the lab scenarios.

### Task 2: Create two Azure web apps

1. In this task, you will create two Azure web apps representing the Canary and Production environments, into which you'll deploy the application via Azure Pipelines.
2. From the lab computer, start a web browser, navigate to the <https://portal.azure.com>, and sign in with the user account that has the Owner role in the Azure subscription you will be using in this lab and has the role of the Global Administrator in the Azure AD tenant associated with this subscription.
3. In the Azure portal, click the Cloud Shell icon, located directly to the right of the search textbox at the top of the page.
4. If prompted to select either Bash or PowerShell, select Bash.
    > Note: If this is the first time you are starting Cloud Shell and you are presented with the You have no storage mounted message, select the subscription you are using in this lab, and select Create storage.
5. From the Bash prompt, in the Cloud Shell pane, run the following command to create a resource group (replace the <region> variable placeholder with the name of the Azure region that will host the two Azure web apps, for example 'westeurope' or 'centralus' or any other available region of your choice):
    > Note: possible locations can be found by running the following command, use the Name on <region> : az account list-locations -o table

    ```bash
    REGION='centralus'
    RESOURCEGROUPNAME='az400m04l09-RG'
    az group create -n $RESOURCEGROUPNAME -l $REGION
    To create an App service plan
    SERVICEPLANNAME='az400m04l09-sp1'
    az appservice plan create -g $RESOURCEGROUPNAME -n $SERVICEPLANNAME --sku S1
    Create two web apps with unique app names.
    SUFFIX=$RANDOM$RANDOM
    az webapp create -g $RESOURCEGROUPNAME -p $SERVICEPLANNAME -n RGATES$SUFFIX-Canary
    az webapp create -g $RESOURCEGROUPNAME -p $SERVICEPLANNAME -n RGATES$SUFFIX-Prod
    ```

    > Note: Record the name of the Canary web app. You will need it later in this lab.

6. Wait for the Web App Services Resources provisioning process to complete and close the Cloud Shell pane.

### Task 3: Configure an Application Insights resource

1. In the Azure portal, use the Search resources, services, and docs text box at the top of the page to search for Application Insights and, in the list of results, select Application Insights.
2. On the Application Insights blade, select + Create.
3. On the Application Insights blade, on the Basics tab, specify the following settings (leave others with their default values):
4. Setting Value
5. Resource group az400m04l09-RG
6. Name the name of the Canary web app you recorded in the previous task
7. Region the same Azure region to which you deployed the web apps earlier in the previous task
8. Resource Mode Classic
   > Note: Disregard the deprecation message. This is required in order to prevent failures of the Enable Continuous Integration DevOps task you will be using later in this lab.
9. Click Review + create and then click Create.
10. Wait for the provisioning process to complete.
11. In the Azure portal, navigate to the resource group az400m04l09-RG you created in the previous task.
12. In the list of resources, click the Canary web app.
13. On the Canary web app page, in the vertical menu on the left, in the Settings section, click Application Insights.
14. On the Application Insights blade, click Turn on Application Insights.
15. In the Change your resource section, click the Select existing resource option, in the list of existing resources, select the newly created Application Insight resource, click Apply and, when prompted for confirmation, click Yes.
16. Wait until the change takes effect.
    > Note: You will create monitor alerts here, which you will use in later part of this lab.
17. From the same Settings / Application Insights menu option, select View Application Insight Data. This redirects you to the Application Insights blade in the Azure Portal.
18. On the Application Insights resource blade, in the Monitoring section, click Alerts and then click Create > Alert rule.
19. On the Select a signal blade, in the Search by signal name textbox, type Failed Requests and select it.
20. On the Create an Alert Rule blade, in the Condition section, leave the Threshold set to Static, validate the other default settings as follows:
21. Aggregation Type: Count
22. Operator: Greater Than
23. Unit: Count
24. In the Threshold value textbox, type 0, and click on Next:Actions. Don't make any changes in the Actions settings blade, and define the following parameters under the Details section:
25. Setting Value
26. Severity 2- Warning
27. Alert rule name RGATESCanary_FailedRequests
28. Advanced Options: Automatically resolve alerts cleared
    > Note: Metric alert rules might take up to 10 minutes to activate.
    > Note: You can create multiple alert rules on different metrics such as availability < 99 percent, server response time > 5 Seconds, or server exceptions > 0
29. Confirm the creation of the Alert rule by clicking Review+Create, and confirm once more by clicking Create. Wait for the alert rule to get created successfully.

## Exercise 1: Configure the build pipeline

1. In this exercise, you will prepare the release artifact (web deploy zip package) for the .NET 6 eShopOnWeb web application source code.
2. From the Devops portal, <https://dev.azure.com>, navigate to the eShopOnWeb_ReleaseGates project you created earlier in this lab.
3. From within the Project, navigate to Pipelines.
4. Click Create Pipelines.
5. From the Where is your Code step, select Azure Repos(Git).
6. In the Select a repository, select eShopOnWeb_ReleaseGates.
7. From the Configure your pipeline step, scroll down in the list and select Existing Azure Pipelines YAML File
8. Under the Select an existing YAML file, select main as Branch, and select /.ado/eshoponweb-ci.yml as Path.
9. Click Continue.
10. From the YAML pipeline screen, click Run to kick off the Build Pipeline.
11. After a few seconds, the status of the Job will switch to Running. Ignore any warnings regarding the source code.
12. Click on the Build Status line to navigate to the detailed Running Job window.
13. Wait for all steps to be completed successfully (green checkmarks). This should take around 2-3min on average.
14. Once the Build Pipeline Job completed successfully, validate the Build Artifacts. From the Job Status page, navigate to the Summary tab. Under Related, it shows 2 published. Select this link.
15. From the Artifacts page appearing, see >Bicep and >WebSite.
16. Open WebSite. Notice BlazorAdmin.zip and Web.zip. We will use the Web.zip artifact in our upcoming Release Pipeline.

## Exercise 2: Configure the release pipeline

In this exercise, you will configure a release pipeline.

### Task 1: Set Up Release Tasks

1. In this task, you will set up the release tasks as part of the Release Pipeline.
2. From the eShopOnWeb_ReleaseGates project in the Azure DevOps portal, in the vertical navigational pane, select Pipelines and then, within the Pipelines section, click Releases.
3. Click New Pipeline.
4. From the Select a template window, choose Azure App Service Deployment (Deploy your application to Azure App Service. Choose from Web App on Windows, Linux, containers, Function Apps, or WebJobs) under the Featured list of templates.
5. Click Apply.
6. From the Stage window appearing, update the default "Stage 1" Stage Name to Canary. Close the popup window by using the X button. You are now in the graphical editor of the Release Pipeline, showing the Canary Stage.
7. Hover the mouse over the Canary Stage, and click the Clone button, to copy the Canary Stage to an additional Stage. Name this Stage Production.
   > Note: The pipeline now contains two stages named Canary and Production.
8. On the Pipeline tab, select the Add an Artifact rectangle, and select the eShopOnWeb_ReleaseGates in the Source (build pipeline) field. Click Add to confirm the selection of the artifact.
9. From the Artifact rectangle, notice the Continuous Integration Trigger (lightning bolt) appearing. Click it to open the Continuous deployment trigger settings.
10. Click the continuous deployment trigger to toggle the switch to enable it. Leave all other settings at default and close the Continuous deployment trigger pane, by clicking the x mark in its upper right corner.
11. Within the Canary Environments stage, click the 1 job, 2 tasks label and review the tasks within this stage.
    > Note: The canary environment has 1 task which, respectively, publishes the artifact package to Azure Web App.
12. On the All pipelines > New Release Pipeline pane, ensure that the Canary stage is selected. In the Azure subscription dropdown list, select your Azure subscription and click Authorize. If prompted, authenticate by using the user account with the Owner role in the Azure subscription.
13. Confirm the App Type is set to "Web App on Windows". Next, in the App Service name dropdown list, select the name of the Canary web app.

### Select the Task Deploy Azure App Service. In the Package or Folder field, update the default value of "$(System.DefaultWorkingDirectory)/**/*.zip" to "$(System.DefaultWorkingDirectory)/**/Web.zip"

1. Notice an exclamation mark next to the Tasks tab. This is expected, as we need to configure the settings for the Production Stage.
2. On the All pipelines > New Release Pipeline pane, navigate to the Pipeline tab, and this time, within the Production Stage, click the 1 job, 2 tasks label. Similar to the Canary stage earlier, complete the pipeline settings. Under the Tasks tab / Production Deployment process, in the Azure subscription dropdown list, select the Azure subscription you used for the Canary Environment stage, shown under Available Azure Service connections, as we already created the service connection before when authorizing the subscription use.
3. In the App Service name dropdown list, select the name of the Prod web app.

### Select the Task Deploy Azure App Service. In the Package or Folder field, update the default value of "$(System.DefaultWorkingDirectory)//*.zip" to "$(System.DefaultWorkingDirectory)//Web.zip"**

1. On the All pipelines > New Release Pipeline pane, click Save and, in the Save dialog box, click OK.
2. You have now successfully configured the Release Pipeline.
3. In the browser window displaying the EshopOnWeb_ReleaseGates project, in the vertical navigational pane, in the Pipelines section, click Pipelines.
4. On the Pipelines pane, click the entry representing eShopOnWeb_ReleaseGates build pipeline and then, on the eShopOnWeb_ReleaseGates pane, click on Run Pipeline.
5. On the Run pipeline pane, accept the default settings and click Run to trigger the pipeline. Wait for the build pipeline to finish.
   > Note: After the build succeeds, the release will be triggered automatically and the application will be deployed to both the environments. Validate the release actions, once the build pipeline completed successfully.
6. In the vertical navigational pane, in the Pipelines section, click Releases and, on the eShopOnWeb_ReleaseGates pane, click the entry representing the most recent release.
7. On the eShopOnWeb_ReleaseGates > Release-1 blade, track the progress of the release and verify that the deployment to both web apps completed successfully.
8. Switch to the Azure portal interface, navigate to the resource group az400m04l09-RG, in the list of resources, click the Canary web app, on the web app blade, click Browse, and verify that the web page (E-commerce website) loads successfully in a new web browser tab.
9. Switch back to the Azure portal interface, this time navigating to the resource group az400m04l09-RG, in the list of resources, click the Production web app, on the web app blade, click Browse, and verify that the web page loads successfully in a new web browser tab.
10. Close the web browser tab displaying the EShopOnWeb web site.
    > Note: Now you have the application with CI/CD configured. In the next exercise we will set up Quality Gates as part of a more advanced Release pipeline.

## Exercise 3: Configure release gates

In this exercise, you will set up Quality Gates in the release pipeline.

### Task 1: Configure pre-deployment gates for approvals

1. In this task, you will configure pre-deployment gates.
2. Switch to the web browser window displaying the Azure DevOps portal, and open the EshopOnWeb_ReleaseGates project. In the vertical navigational pane, in the Pipelines section, click Releases and, on the New Release Pipeline pane, click Edit.
3. On the All pipelines > New Release Pipeline pane, on the left edge of the rectangle representing the Canary Environment stage, click the oval shape representing the Pre-deployment conditions.
4. On Pre-deployment conditions pane, set the Pre-deployment approvals slider to Enabled and, in the Approvers text box, type and select your Azure DevOps account name.
   > Note: In a real-life scenario, this should be a DevOps Team name alias instead of your own name.
5. Save the pre-approval settings and close the popup window.
6. Click Create Release and confirm by pressing the Create button from the popup window.
7. Notice the green confirmation message, saying "Release-2" has been created. Click the link of "Release-2" to navigate to its details.
8. Notice the Canary Stage is in a Pending Approval state. Click the Approve button. This sets off the Canary Stage again.

### Task 2: Configure post-deployment gates for Azure Monitor

1. In this task, you will enable the post-deployment gate for the Canary Environment.
2. Back on the All pipelines > New Release Pipeline pane, on the right edge of the rectangle representing the Canary Environment stage, click the oval shape representing the Post-deployment conditions.
3. On Post-deployment conditions pane, set the Gates slider to Enabled, click + Add, and, in the pop-up menu, click Query Azure Monitor Alerts.
4. On Post-deployment conditions pane, in the Query Azure Monitor Alerts section, in the Azure subscription dropdown list, select the service connection entry representing the connection to your Azure subscription, and, in the Resource group dropdown list, select the az400m04l09-RG entry.
5. On the Post-deployment conditions pane, expand the Advanced section and configure the following options:
6. Filter type: None
7. Severity: Sev0, Sev1, Sev2, Sev3, Sev4
8. Time Range: Past Hour
9. Alert State: Acknowledged, New
10. Monitor Condition: Fired
11. On Post-deployment conditions pane, expand the Evaluation options and configure the following options:
12. Set the value of Time between re-evaluation of gates to 5 Minutes.
13. Set the value of Timeout after which gates fail to 8 Minutes.
14. Select the On successful gates, ask for approvals option.
    > Note: The sampling interval and timeout work together so that the gates will call their functions at suitable intervals and reject the deployment if they don't succeed during the same sampling interval within the timeout period.
15. Close the Post-deployment conditions pane, by clicking the x mark in its upper right corner.
16. Back on the New Release Pipeline pane, click Save, and in the Save dialog box, click OK.

## Exercise 4: Test release gates

In this exercise, you will test the release gates by updating the application, which will trigger a deployment.

### Task 1: Update and deploy application after adding release gates

1. In this task, you will track the release process with the release gates enabled.
2. From the Azure Portal, in the "Search resources, services and docs" field, enter Alerts to open the Alerts Service of Azure Monitor.
3. Notice there should be at least 1 Alert with Severity 2 - Warning showing up in the list. This got trigger when you validated the website in the previous exercise.
   > Note: If no Alert shows up yet, wait another few minutes. Speeding up Alerts is possible by navigating to the Canary-URL again from a browser.
4. Return back to the Azure DevOps Portal, open the EShopOnWeb-Release Gates Project. Navigate to Pipelines, select Releases and select the New Release Pipeline.
5. Click the Create Release button.
6. Wait for the Release pipeline to kick off, and approve the Canary Stage release action.
7. Wait for the Canary release Stage to complete successfully. Notice how the Post-deployment Gates is switching to an Evaluation Gates status. Click the Evaluation Gates icon.
8. For the Query Azure Monitor Alerts, notice an initial failed state.
9. Let the Release pipeline in a pending state for the next 5 minutes. After the 5 minutes did pass, notice the 2nd evaluation failing again.
10. This is expected behavior, since there is an Application Insights Alerts triggered for the Canary Web App.
    > Note: Since there is an alert triggered by the exception, Query Azure Monitor gate will fail. This, in turn, will prevent deployment to the Production environment.
11. Wait another 3 minutes and validate the status of the Release Gates again. As it is now +8 minutes after the initial Release Gates got checked, and it's been more than 8 minutes since the initial Application Insight Alert got triggered with action "Fired", it should result in a successful Release Gate, having allowed the deployment of the Production Release Stage as well.

## Review

In this lab, you configured release pipelines and then configured and tested release gates.
