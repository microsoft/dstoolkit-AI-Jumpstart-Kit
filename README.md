# About this Toolkit

The "AI Orchestration Jumpstart Kit" is a toolkit designed to streamline and accelerate the adoption of AI technologies. It provides pre-built components, best practices, and documentation links for easier integration of AI solutions into various environments.

The package provides a sample [Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/overview/) SDK-based prototype implementation with configurable parameters and settings.

- Allows for quick start use of Semantic Kernel(SK) without the need to develop code
- Enables team members to work on the solution independently
- Includes:
  - Endpoints for basic SK requests
  - Endpoints for creating more SK semantic functions prompts and configuration files
  - Implementation of an in-memory Vector Database
  - Reference source code 

# Project Wiki

Project [Wiki](https://github.com/microsoft/dstoolkit-AI-Jumpstart-Kit/wiki)

# C# WebAPI SK Connector

The C# WebAPI Connector Class is an interface for interacting with various Semantic Kernel AI functions.

Below are details about each endpoint, its purpose, and the possible responses.

## Function Execute
Description: Executes a specified function.

URL: /Function/Execute  
Method: POST  
Parameters:  
pluginName (string): The name of the plugin containing the function.  
functionName (string): The name of the function to execute.  
input (string): The input data for the function.  
Response:  
Success: 200 OK, with the function execution result.  
Error: 400 Bad Request if an error occurs during execution.  

## Function Execute With Memory
Description: Executes a function with memory search.  

URL: /Function/ExecuteWithMemory  
Method: POST  
Parameters:  
pluginName (string): The name of the plugin containing the function.  
functionName (string): The name of the function to execute.  
Relevance (double): The relevance threshold for memory search.  
input (string): The input data for the function.  
Response:   
Success: 200 OK, with the function execution result.  
Error: 400 Bad Request if an error occurs during execution.  
Not Found: 404 Not Found if the input is not found within the relevance threshold.  

## List Plugins
Description: Lists all available plugins and their functions.  

URL: /SK/ListPlugins  
Method: GET  
Response:  
Success: 200 OK, with a JSON document containing plugin information.  
Error: 400 Bad Request if an error occurs during execution.  

## Get
Description: Retrieves the prompt and configuration for a specified function.  

URL: /SK/Get  
Method: GET  
Parameters:  
pluginName (string): The name of the plugin containing the function.  
functionName (string): The name of the function.  
Response:  
Success: 200 OK, with a JSON document containing the prompt and configuration.  
Not Found: 404 Not Found if the prompt file is not found.  
Error: 400 Bad Request if an error occurs during execution.  

## Insert
Description: Inserts a new prompt and configuration for a specified function.  

URL: /SK/Post  
Method: POST  
Parameters:  
pluginName (string): The name of the plugin containing the function.  
functionName (string): The name of the function.  
SKPrompt (string): The prompt for the function.  
SkConfigjson (string, optional): The configuration JSON for the function.  
Response:  
Success: 200 OK if the prompt and configuration are successfully inserted.  
Error: 400 Bad Request if an error occurs during execution.  

## Delete
Description: Deletes the prompt and configuration for a specified function.  

URL: /SK/Delete  
Method: DELETE  
Parameters:  
pluginName (string): The name of the plugin containing the function.  
functionName (string): The name of the function.  
Response:  
Success: 200 OK if the prompt and configuration are successfully deleted.  
Not Found: 404 Not Found if the prompt file is not found.  
Error: 400 Bad Request if an error occurs during execution.  

## Memory List
Description: Lists items in the memory.  

URL: /Memory/List  
Method: GET  
Response:  
Success: 200 OK, with a JSON document containing memory items.  
Error: 400 Bad Request if an error occurs during execution.  
 
## Memory Search
Description: Searches for an item in the memory.  

URL: /Memory/Search  
Method: POST   
Parameters:  
Input (string): The input data for memory search.  
Response:  
Success: 200 OK, with the search result and relevance information.  
Error: 400 Bad Request if an error occurs during execution.  

# Configuring OAuth 2.0 with Azure Identity

## Prerequisites
Before you begin, ensure you have the following prerequisites in place:

- Microsoft Azure account.
- Access to the API you want to connect to.
- A registered application in Azure AD (Azure Active Directory)
- Understanding of Azure AD B2C and OAuth 2.0.

## Registering the Application
To configure OAuth 2.0 with Azure Identity, you need to register your application in Azure AD. Here are the steps:

- Go to [Azure Portal](https://portal.azure.com/) and sign in using your Azure account.
- In the Azure Portal, search for "Azure Active Directory" and select it.
- In the Azure AD blade, click on "App registrations" on the left-hand menu.
- Click the "New registration" button.
- Enter a name for your application.
- Choose the supported account types (e.g., single-tenant or multi-tenant).
- Enter the Redirect URI, which is the callback URL for OAuth 2.0 (e.g., `https://yourapp.com/callback`).
- Click the "Register" button to create the application.
- After registration, make note of the `Application (client) ID` and `Directory (tenant) ID`. You'll need these values later.

## Configuring OAuth 2.0 Settings
Now that you have registered your application, configure the OAuth 2.0 settings for it.

- In your application's Azure AD settings, go to "Authentication."
- Under "Platform configurations," add the Redirect URI you specified earlier.
- Save the changes.

## Generate Client Secret
- In your application's Azure AD settings, go to "Certificates & secrets."
- Under "Client secrets," click on "+ New client secret."
- Enter a description, select an expiration, and click "Add."
- Note the generated client secret. This will be used for authentication.

## Implementing OAuth 2.0 with Azure Identity in Your Application
To connect to the API using OAuth 2.0 and Azure Identity, your application must implement the OAuth 2.0 authorization flow with Azure Identity. Refer to the official Microsoft documentation for your specific programming language or platform for detailed implementation steps.

For .NET applications using Azure Identity: [Microsoft documentation](https://learn.microsoft.com/en-us/azure/active-directory-b2c/enable-authentication-web-api?tabs=csharpclient)

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

# Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
