## Setting up the WebApp AI Chat

This is the interface for the "chat" endpoint in the CatalogAPI and adding some Semantic Kernel functionality and Prompt Engineering.

1. We need to create the ChatState class to hold the conversation in the UI, track history and uses SK to create a Kernel and Augment our Prompt.

1. For this, we need to create a new component in the `Components` folder at `.\WebApp\Components\`. For this, create a new folder in `.\WebApp\` called 'Chatbot'

   !IMAGE[Captura de tela 2024-05-17 152932.png](/docs/ai-lab/img/Captura de tela 2024-05-17 152932.png)

1. Add the UI Elements from the folder [Chatbot.zip](Chatbot.zip) asset. Copy and paste them in the solution in the folder `.src/Components/Chatbot/`

   !IMAGE[Captura de tela 2024-05-17 153022.png](/docs/ai-lab/img/Captura de tela 2024-05-17 153022.png)

1. Change the file `/WebApp/Components/Layout/MainLayout.razor` with the following code:

   ```html
   @using eShop.WebApp.Components.Chatbot @inherits LayoutComponentBase

   <HeaderBar />
   @Body
   <ShowChatbotButton />
   <FooterBar />

   <div id="blazor-error-ui">
     An unhandled error has occurred.
     <a href="" class="reload">Reload</a>
     <a class="dismiss">🗙</a>
   </div>
   ```

1. Run the application with **Ctrl + F5**

Now, our eShop has a chatbot, but without communicating to our Catalog Application data. We need to create Azure OpenAi connections for our data with Embeddings. Let's configure our CatalogAPI to do it.
