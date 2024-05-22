## Services Setup

Before we dive into the eShop solution, you’ll need to ensure you have the Prerequisites outlined in the [README](/README.md).

Next, you’ll want to open the eShop solution:

- Locate and click on the `eShop.Web.slnf` file to open it.
- Allow some time for the solution to fully load, which may take a few minutes depending on the solution size and your machines performance.

With the solution open, it’s time to set up your development environment:

- Open the Command Line Interface (CLI): Navigate to `View -> Terminal` or use the shortcut Ctrl + ` to open the integrated terminal within Visual Studio.

![Captura de tela 2024-05-09 114025.png](/docs/ai-lab/img/Captura%20de%20tela%202024-05-09%20114025.png)

- Navigate to the Project Folder: Use the command `cd .\src\eShop.AppHost\` to change directories to the eShop.AppHost folder, where you’ll be running commands to set up the project.

![Captura de tela 2024-05-09 124556.png](/docs/ai-lab/img/Captura%20de%20tela%202024-05-09%20124556.png)

To create our Azure AI services, we need to do the following commands:

1. Login to azure, use `az login` and your Azure credentials.

1. Set your Az CLI to our subscription: `az account set --subscription <your subscription id>`

1. Create our **Azure Resource Group**: `az group create --name rg-openai-services --location eastus2`

1. Create our **Azure OpenAI Resource**: `az cognitiveservices account create --name openai --resource-group rg-openai-services --location eastus --kind OpenAI --sku s0 --subscription <your subscription id>`

1. **Deploy our models** as follows:

   - Deploy the **text embedding model**: `az cognitiveservices account deployment create --name openai --resource-group rg-openai-services --deployment-name text-embedding-3-small --model-name text-embedding-3-small --model-version "1" --model-format OpenAI --sku-capacity "1" --sku-name "Standard"`

   - Deploy the **GPT model**: `az cognitiveservices account deployment create --name openai --resource-group rg-openai-services --deployment-name gpt-35-turbo-16k --model-name gpt-35-turbo --model-version "0613" --model-format OpenAI --sku-capacity "1" --sku-name "Standard"`

Finally, it’s time to see eShop in action:

- Press Ctrl + F5 to run the application without debugging.
- If Visual Studio asks you to run Docker, press `Yes` to run the background applications.

![Captura de tela 2024-05-09 114245.png](/docs/ai-lab/img/Captura%20de%20tela%202024-05-09%20114245.png)

- You are going to get greeted by a Console and a Login screen, follow the instructions to see the dashboard.

![Captura de tela 2024-05-09 115710.png](/docs/ai-lab/img/Captura%20de%20tela%202024-05-09%20115710.png)

![Captura de tela 2024-05-09 115537.png](/docs/ai-lab/img/Captura%20de%20tela%202024-05-09%20115537.png)

- At the dashboard, you will have access to all services and click on the WebApp to see our eShop without the AI features implemented.

![Captura de tela 2024-05-09 130427.png](/docs/ai-lab/img/Captura%20de%20tela%202024-05-09%20130427.png)

![Captura de tela 2024-05-09 115743.png](/docs/ai-lab/img/Captura%20de%20tela%202024-05-09%20115743.png)

- In case of it not loading properly, just kill the application and reload it again.

Now, let's configure eShop to receive AI Capabilities. Going to the next step to configure the application to use the AI features that we are going to implement.

[Next Step: Configure Aspire](03-ConfigureAspire.md)