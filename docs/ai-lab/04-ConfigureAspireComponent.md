## Configuring Azure OpenAI Component

For our Azure OpenAI to work properly, we need to add Semantic Kernel to the equation for getting the answers in our RAG Pattern.

1. Navigate to the WebApp folder using `cd ..\WebApp\` in the Terminal.

1. Now, let us install some packages to our WebApp.

   - Add to the WebApp csproj the following lines:

     ```csproj
     <PackageReference Include="Aspire.Azure.AI.OpenAI" />
     <PackageReference Include="Microsoft.SemanticKernel" />
     ```

   - Aspire AOAI component: This library is used to register an OpenAIClient in the dependency injection (DI) container for consuming Azure AI OpenAI or OpenAI functionality.
   - Installing the Semantic Kernel package for SK functions.

1. Adding the extensions to add some AI from the services builder.

   - Open the WebApp Extensions file in `./WebApp/Extensions/Extensions.cs`.

     ![Captura de tela 2024-05-17 152736.png](/docs/ai-lab/img/Captura%20de%20tela%202024-05-17%20152736.png)

   - Add the following dependencies:

     ```csharp
     using Azure.AI.OpenAI;
     using Microsoft.SemanticKernel;
     using Microsoft.SemanticKernel.ChatCompletion;
     using Microsoft.SemanticKernel.Connectors.OpenAI;
     using Microsoft.SemanticKernel.TextGeneration;
     ```

   - Search the following `// TODO - Register AI` in the `AddAIServices`
   - Replace with the following block of code:

     ```csharp
     var openAIOptions = builder.Configuration.GetSection("AI").Get<AIOptions>()?.OpenAI;
     var deploymentName = openAIOptions?.ChatModel;

     if (!string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("openai")) && !string.IsNullOrWhiteSpace(deploymentName))
     {
         builder.Services.AddKernel();
         builder.AddAzureOpenAIClient("openai");
         builder.Services.AddAzureOpenAIChatCompletion(deploymentName);
     }
     ```

   - The block adds AI Services to our WebApp container in the application, with Aspire, the Chat Model and Semantic Kernel.

Great work! Now, let's add some UI to have our chat to pop!

[Next Step: Setting up the WebApp AI Chat](05-WebUI.md)
