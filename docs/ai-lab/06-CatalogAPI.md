## Configuring CatalogAPI

With all the settings done, we need now to implement some of the AI features for our application, to do that we need to configure the CatalogAPI to add the items to be easily retrievable for our RAG pattern.

1.  Navigate to the CatalogAPI folder using `cd ..\Catalog.API\` in the Terminal.

1.  Now, let’s install some packages to our Catalog API.

    - Aspire AOAI component: This library is used to register an OpenAIClient in the dependency injection (DI) container for consuming Azure AI OpenAI or OpenAI functionality.
    - Semantic Kernel components: Three libraries are needed to SemanticKernel functionality in our API.
      1. Installing the Semantic Kernel package for SK functions.
      1. Installing the Semantic Kernel Connectors package for PostgreSQL to connect to the database
      1. Installing the Semantic Kernel plugins for memory management package:
    - Add the packages, going to the .csproj and adding.

      ```csproj

      <PackageReference Include="Aspire.Azure.AI.OpenAI" />
      <PackageReference Include="Microsoft.SemanticKernel" />
      <PackageReference Include="Microsoft.SemanticKernel.Connectors.Postgres" />
      <PackageReference Include="Microsoft.SemanticKernel.Plugins.Memory" />
      ```

    - To end our package adventure, add the following into the Catalog.API .csproj's PropertyGroup : `<NoWarn>$(NoWarn);SKEXP0001;SKEXP0010;SKEXP0020</NoWarn>`.

      ![Captura de tela 2024-05-17 155704.png](/docs/ai-lab/img/Captura%20de%20tela%202024-05-17%20155704.png)

1.  With this, we can now create some services for our application in Catalog.

    - First, let's add functionality to our services interface. Go to `Catalog.API -> Services -> ICatalogAI.cs`, open the file in Visual Studio.

    ![Captura de tela 2024-05-20 070013.png](/docs/ai-lab/img/Captura%20de%20tela%202024-05-20%20070013.png)

    - Let's import our Semantic Kernel Memory package. `using Microsoft.SemanticKernel.Memory;`.
    - Add the following code:

      ```csharp
      /// <summary>
      /// Saved an item to the available memory store.
      /// </summary>
      /// <param name="item">The <see cref="CatalogItem"/> to generate the vector for.</param>
      /// <returns>The memory ID of the vector.</returns>
      ValueTask<string> SaveToMemoryAsync(CatalogItem item);

      /// <summary>
      /// Searches the memory store for items similar to the query.
      /// </summary>
      /// <param name="query">The query.</param>
      /// <param name="pageSize">The amount of records per page.</param>
      /// <returns>The matched memory records.</returns>
      IAsyncEnumerable<MemoryQueryResult> SearchMemoryAsync(string query, int pageSize);
      ```

    - We are adding two functions, `SaveToMemoryAsync` and `SearchMemoryAsync`, these are to save a catalog item embedding to memory and to search items according to their similarity.

    - Implementing the services into `CatalogAI.cs`, delete the current code and copy and paste the following:

      ```csharp
      using Microsoft.SemanticKernel.Embeddings;
      using Microsoft.SemanticKernel.Memory;
      namespace eShop.Catalog.API.Services;
      public sealed class CatalogAI(
          ISemanticTextMemory memory = null,
          ITextEmbeddingGenerationService embeddingGenerator = null) : ICatalogAI
      {
          internal static int EmbeddingDimensions = 384;
          internal static string MemoryCollection = "catalogMemory";
          /// <inheritdoc/>
          public bool IsEnabled => embeddingGenerator is not null;
          /// <inheritdoc/>
          public ValueTask<string> SaveToMemoryAsync(CatalogItem item) =>
              IsEnabled ?
                  new(memory.SaveInformationAsync(MemoryCollection, $"{item.Name} {item.Description}", item.Id.ToString()))
                  : new(string.Empty);
          /// <inheritdoc/>
          public IAsyncEnumerable<MemoryQueryResult> SearchMemoryAsync(string query, int pageSize) =>
              IsEnabled ? memory.SearchAsync(MemoryCollection, query, pageSize, minRelevanceScore: 0.5) : throw new InvalidOperationException("Search can't be performed when AI is disabled");
      }
      ```

    - This implements into our Catalog the SemanticKernel functions, that we will use to change the service container.

1.  With the services developed, we can implement to our Catalog container the AI functions to get the data.

        - For the application to use the RAG (Retrieval Augmented Generation) pattern, it needs the information to be stored in the Vector Database so it can be retrieved to add context to our prompt. To do this, the Catalog API needs some updates in some functions and classes.
        - First, in `./Catalog.API/Apis/CatalogApi.cs`.

          - Search for `// TODO - AI features` with a `throw new NotImplementedException();`
          - We are going to implement the `GetItemsBySemanticRelevance` function to search the relevance of the items depending on similarity from the request.
          - Delete the `throw new NotImplementedException();` and replace with:

            ```csharp
            List<CatalogItem> itemsOnPage = [];

            // Get the total number of items
            var totalItems = await services.Context.CatalogItems
                .LongCountAsync();
            var itemsWithDistance = services.CatalogAI.SearchMemoryAsync(text, pageSize);

            await foreach (var item in itemsWithDistance)
            {
                var catalogItem = await services.Context.CatalogItems.FindAsync(int.Parse(item.Metadata.Id));
                itemsOnPage.Add(catalogItem);
            }

            return TypedResults.Ok(new PaginatedItems<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage));
            ```

          - Next, search for `// TODO: Update the AI with data change`.
          - Add the following line: `await services.CatalogAI.SaveToMemoryAsync(catalogItem);`
          - Lastly, search for `// TODO: Update AI with new catalog item`.
          - Add the following line: `await services.CatalogAI.SaveToMemoryAsync(item);`
          - Both itens deal when an item updated in `UpdateItem` or a new item is added `CreateItem`, updating the AI to add the description to be searchable for our Prompt augmentation with Semantic Kernel.

        - Updating `./Catalog.API/Extensions/Extensions.cs`.
          ![Captura de tela 2024-05-20 070448-2.png](/docs/ai-lab/img/Captura%20de%20tela%202024-05-20%20070448-2.png)

          - This adds the Catalog API our extensions for AI.
          - First, let's add the libraries.

            ```csharp
            using Microsoft.SemanticKernel;
            using Microsoft.SemanticKernel.Connectors.Postgres;
            using Microsoft.SemanticKernel.Memory;
            ```

          - Lastly under `builder.Services.AddOptions<CatalogOptions>()

    .BindConfiguration(nameof(CatalogOptions));` Add the funcionality to generate the information communicating with the AzureOpenAI API, Semantic Kernel and the models with the following.

                 ```csharp
                 if (!string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("openai")))
                 {
                     builder.AddAzureOpenAIClient("openai");
                     builder.Services.AddAzureOpenAITextEmbeddingGeneration(
                         builder.Configuration["AIOptions:OpenAI:EmbeddingName"] ?? "text-embedding-3-small",
                         dimensions: CatalogAI.EmbeddingDimensions);
                     builder.AddKeyedNpgsqlDataSource("catalogdb", null, builder => builder.UseVector());
                     builder.Services.AddSingleton<IMemoryStore, PostgresMemoryStore>(provider =>
                     {
                         var dataSource = provider.GetRequiredKeyedService<NpgsqlDataSource>("catalogdb");
                         return new(dataSource, CatalogAI.EmbeddingDimensions);
                     });
                     builder.Services.AddSingleton<ISemanticTextMemory, SemanticTextMemory>();
                 }
                 ```

1.  Now, extend the CatalogContextSeed to seed the Semantic Kernel memory into our database and use it when needed:

    - Go to `./Catalog.API/Infrastructure/CatalogContextSeed.cs`
      ![Captura de tela 2024-05-20 070619.png](/docs/ai-lab/img/Captura%20de%20tela%202024-05-20%20070619.png)
    - Search for `// TODO - seed AI features in database` and replace it with:

      ```csharp
      foreach (var catalogItem in catalogItems)
      {
          var id = await catalogAI.SaveToMemoryAsync(catalogItem);
          logger.LogInformation("Created memory record for Catalog item with Id {Id}", id);
      }
      ```

**Congratulations**, we added a lot of functionality on the Catalog part of this application. Creating and updating the Vector items, adding them to the Database, retrieving to use them as comparison to our prompt and getting the closest item.

**Run the application with Ctrl + F5**. While our application is indexing our forty element catalog into the database, it will not open the store WebApp. Check the status on **Logs -> catalog-api**.

![Captura de tela 2024-05-20 071200.png](/docs/ai-lab/img/Captura%20de%20tela%202024-05-20%20071200.png)

Wait for the catalog elements (Forty) to be indexed, after this, the WebApp with the Chat RAG application is online!

![Captura de tela 2024-05-20 071504.png](/docs/ai-lab/img/Captura%20de%20tela%202024-05-20%20071504.png)
