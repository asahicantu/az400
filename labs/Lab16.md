# Lab 16 - Implement Security and Compliance in an Azure DevOps pipeline

## Overview

In this lab, you will use Mend Bolt (formerly WhiteSource) to automatically detect vulnerable open source components, outdated libraries, and license compliance issues in your code. You will leverage WebGoat, an intentionally insecure web application, maintained by OWASP designed to illustrate common web application security issues.
Mend is the leader in continuous open source software security and compliance management. WhiteSource integrates into your build process, irrespective of your programming languages, build tools, or development environments. It works automatically, continuously, and silently in the background, checking the security, licensing, and quality of your open source components against WhiteSource constantly-updated definitive database of open source repositories.
Mend provides Mend Bolt, a lightweight open source security and management solution developed specifically for integration with Azure DevOps and Azure DevOps Server. Note that Mend Bolt works per project and does not offer real-time alert capabilities, which requires Full platform, generally recommended for larger development teams that want to automate their open source management throughout the entire software development lifecycle (from the repositories to post-deployment stages) and across all projects and products.
Azure DevOps integration with Mend Bolt will enable you to:

1. Detect and remedy vulnerable open source components.
2. Generate comprehensive open source inventory reports per project or build.
3. Enforce open source license compliance, including dependencies’ licenses.
4. Identify outdated open source libraries with recommendations to update.

## Objectives

After you complete this lab, you will be able to:

- Activate Mend Bolt
- Run a build pipeline and review Mend security and compliance report

## Exercise 0: Configure the lab prerequisites

    In this exercise, you will set up the prerequisites for the lab, which consist of a new Azure DevOps project with a repository based on the https://dev.azure.com/unhueteb/_git/eshopweb-az400.

### Task 0: Associate your subscription with your project

1. Navigate to the following URL <https://dev.azure.com> and login
2. Click Start free.
3. On the Get started with Azure DevOps page click Continue.
4. In the vertical navigational pane of the Azure DevOps portal, select Azure Devops Logo and then click the Organization settings icon.
5. In the navigational pane, select Billing.
6. Under Billing select Set up billing.
7. In the Set up billing pane, once your susbscription is valid in a green box select Save.
8. Set the Paid parallel jobs for MS Hosted CI/CD and Self-Hosted CI/CD to 1.
9. Scroll down to the bottom of the page and click Save.
10. In the navigational pane, select policies under Security.
11. Under Application connection policies, toggle Third-party application access via OAuth to On.
12. Navigate back to the project.

### Task 1: (skip if done) Create and configure the team project

In this task, you will create an eShopOnWeb Azure DevOps project to be used by several labs.
On your lab computer, in a browser window open your Azure DevOps organization. Click on New Project. Give your project the name eShopOnWeb and leave the other fields with defaults. Click on Create.

### Task 2: (skip if done) Import eShopOnWeb Git Repository

In this task you will import the eShopOnWeb Git repository that will be used by several labs.

1. On your lab computer, in a browser window open your Azure DevOps organization and the previously created eShopOnWeb project. Click on Repos>Files , Import. On the Import a Git Repository window, paste the following URL <https://github.com/MicrosoftLearning/eShopOnWeb.git> and click Import:
2. The repository is organized the following way:

    ```bash
    .ado folder contains Azure DevOps YAML pipelines
    .devcontainer folder container setup to develop using containers (either locally in VS Code or GitHub Codespaces)
    .azure folder contains Bicep&ARM infrastructure as code templates used in some lab scenarios.
    .github folder container YAML GitHub workflow definitions.
    ```

3. src folder contains the .NET 6 website used on the lab scenarios.

## Exercise 1: Implement Security and Compliance in an Azure DevOps pipeline by using Mend Bolt

In this exercise, leverage Mend Bolt to scan the project code for security vulnerabilities and licensing compliance issues, and view the resulting report.

### Task 1: Activate Mend Bolt extension

In this task, you will activate WhiteSource Bolt in the newly generated Azure Devops project.

1. On your lab computer, in the web browser window displaying the Azure DevOps portal with the eShopOnWeb project open, click on the marketplace icon > Browse Marketplace.
2. On the MarketPlace, search for Mend Bolt (formerly WhiteSource) and open it. Mend Bolt is the free version of the previously known WhiteSource tool, which scans all your projects and detects open source components, their license and known vulnerabilities.
   > Warning: make sure you select the Mend Bolt option (the free one)!
3. On the Mend Bolt (formerly WhiteSource) page, click on Get it for free.
4. On the next page, select the desired Azure DevOps organization and Install. Proceed to organization once installed.
5. In your Azure DevOps navigate to Organization Settings and select Mend under Extensions. Provide your Work Email (your lab personal account, e.g. using AZ400learner@outlook.com instead of student@microsoft.com ), Company Name and other details and click Create Account button to start using the Free version.

### Task 2: Create and Trigger a build

In this task, you will create and trigger a CI build pipeline within Azure DevOps project. You will use Mend Bolt extension to identify vulnerable OSS components present in this code.

1. On your lab computer, from the eShopOnWeb Azure DevOps project, in the vertical menu bar on the left side, navigate to the Pipelines>Pipelines section, click Create Pipeline (or New Pipeline).
2. On the Where is your code? window, select Azure Repos Git (YAML) and select the eShopOnWeb repository.
3. On the Configure section, choose Existing Azure Pipelines YAML file. Provide the following path /.ado/eshoponweb-ci-mend.yml and click Continue.
4. Review the pipeline and click on Run. It will take a few minutes to run successfully.
    > Note: The build may take a few minutes to complete. The build definition consists of the following tasks:
5. DotnetCLI task for restoring, building, testing and publishing the dotnet project.
6. Whitesource task (still keeps the old name), to run the Mend tool analysis of OSS libraries.
7. Publish Artifacts the agents running this pipeline will upload the published web project.
8. While the pipeline is executing, lets rename it to identify it easier (as the project may be used for multiple labs). Go to Pipelines/Pipelines section in Azure DevOps project, click on the executing Pipeline name (it will get a default name), and look for Rename/move option on the ellipsis icon. Rename it to eshoponweb-ci-mend and click Save.
9. Once the pipeline execution has finished, you can review the results. Open the latest execution for eshoponweb-ci-mend pipeline. The summary tab will show the logs of the execution, together with related details such as the repository version(commit) used, trigger type, published artifacts, test coverage, etc.
10. On the Mend Bolt tab, you can review the OSS security analysis. It will show you details around the inventory used, vulnerabilities found (and how to solve them), and an interesting report around library related Licenses. Take some time to review the report.

## Review

In this lab, you will use Mend Bolt with Azure DevOps to automatically detect vulnerable open source components, outdated libraries, and license compliance issues in your code.
