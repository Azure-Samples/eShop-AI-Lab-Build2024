## Configuring the Aspire Host

Before beginning our journey to add Artificial Intelligence, we need to configure our .NET Aspire host to use the services when implement them later. If you don't know, [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/get-started/aspire-overview) is a cloud ready stack to build distributed applications using NuGet.

1. First, let's add some packages in `Directory.Packages.props`. Open it in `Solution Items -> Directory.Packages.props`

   ![Captura de tela 2024-05-20 064356.png](/docs/ai-lab/img/Captura%20de%20tela%202024-05-20%20064356.png)

1. On the `<!-- Version together with Aspire -->` block, add the following lines:

   ```
   <PackageVersion Include="Aspire.Hosting.Azure.CognitiveServices" Version="$(AspireVersion)" />
   <PackageVersion Include="Aspire.Azure.AI.OpenAI" Version="$(AspireVersion)" />
   ```

1. Add a new block for the Semantic Kernel NuGet packages:

   ```
   <!-- AI -->
   <PackageVersion Include="Microsoft.SemanticKernel" Version="1.11.0" />
   <PackageVersion Include="Microsoft.SemanticKernel.Connectors.Postgres" Version="1.11.0-alpha" />
   <PackageVersion Include="Microsoft.SemanticKernel.Plugins.Memory" Version="1.11.0-alpha" />
   ```

1. Save the `Directory.Packages.props` file.

   ![Captura de tela 2024-05-16 234722.png](/docs/ai-lab/img/Captura%20de%20tela%202024-05-16%20234722.png)

1. Open the on `eShop.AppHost` csproj. Add the following line to install Aspire Hosting Azure Congnitive Services. `<PackageReference Include="Aspire.Hosting.Azure.CognitiveServices" />`

1. Let's change `./eShop.AppHost/Program.cs` to add the AI connections to the AI capabilities that will be built soon.
   Open `Program.cs`.

1. Search for the `WebApp` container line in `var webApp = builder.AddProject<Projects.WebApp>("webapp", launchProfileName)`

   - After the configurations, add the following block of code:

     ```csharp
     // set to true if you want to use OpenAI
     bool useOpenAI = true;
     if (useOpenAI)
     {
         const string openAIName = "openai";
         const string textEmbeddingName = "text-embedding-3-small";
         const string chatModelName = "gpt-35-turbo-16k";
         // to use an existing OpenAI resource, add the following to the AppHost user secrets:
         // "ConnectionStrings": {
         //   "openai": "Key=<API Key>" (to use https://api.openai.com/)
         //     -or-
         //   "openai": "Endpoint=https://<name>.openai.azure.com/" (to use Azure OpenAI)
         // }
         IResourceBuilder<IResourceWithConnectionString> openAI;
         if (builder.Configuration.GetConnectionString(openAIName) is not null)
         {
             openAI = builder.AddConnectionString(openAIName);
         }
         else
         {
             // to use Azure provisioning, add the following to the AppHost user secrets:
             // "Azure": {
             //   "SubscriptionId": "<your subscription ID> "
             //   "Location": "<location>"
             // }
             openAI = builder.AddAzureOpenAI(openAIName)
                 .AddDeployment(new AzureOpenAIDeployment(chatModelName, "gpt-35-turbo", "0613"))
                 .AddDeployment(new AzureOpenAIDeployment(textEmbeddingName, "text-embedding-3-small", "1"));
         }
         catalogApi
             .WithReference(openAI)
             .WithEnvironment("AI__OPENAI__EMBEDDINGNAME", textEmbeddingName);
         webApp
             .WithReference(openAI)
             .WithEnvironment("AI__OPENAI__CHATMODEL", chatModelName); ;
     }
     ```

1. Now, we need to get the OpenAI credentials to use our set resources. Go to the [Azure Portal](https://portal.azure.com/#home).

1. Click in the search field at the top of the page and search for **openai**, which will open a list of all Azure OpenAI Service resources. Click on your recently deployed resource and access the Keys and Endpoint in `Resource Management -> Keys and Endpoint`.

![Captura de tela 2024-05-17 001028.png](/docs/ai-lab/img/Captura%20de%20tela%202024-05-17%20001028.png)
![Captura de tela 2024-05-17 001141.png](/docs/ai-lab/img/Captura%20de%20tela%202024-05-17%20001141.png)
![Captura de tela 2024-05-17 001158.png](/docs/ai-lab/img/Captura%20de%20tela%202024-05-17%20001158.png)
![Captura de tela 2024-05-17 001307.png](/docs/ai-lab/img/Captura%20de%20tela%202024-05-17%20001307.png)

1. Use the console to add the secrets to our application to access the resources.

   - Add the endpoint and key: `dotnet user-secrets set "ConnectionStrings:OpenAI" "Endpoint=<your-azure-openai-endpoint-here>;Key=<your-azure-openai-key-here>"`.
   - Can use notepad to change the variables above and paste on the console.

1. Press Ctrl + F5 to run the application.

1. Check on the `Resources` tab, look at the `WebApp project`.

![Captura de tela 2024-05-17 001633.png](/docs/ai-lab/img/Captura%20de%20tela%202024-05-17%20001633.png)

1. Scroll and check with the `ConnectionStrings_openai`, to check if it is correct configured and ready to connect with our services that will be implemented.

![Captura de tela 2024-05-17 002221.png](/docs/ai-lab/img/Captura%20de%20tela%202024-05-17%20002221.png)

With the Aspire with the variables set, we need to configure the Azure OpenAI component.

[Next Step: Configure Azure OpenAI Component](04-ConfigureAspireComponent.md)
