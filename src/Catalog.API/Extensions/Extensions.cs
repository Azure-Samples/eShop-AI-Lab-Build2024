using eShop.Catalog.API.Services;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Postgres;
using Microsoft.SemanticKernel.Memory;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<CatalogContext>("catalogdb");

        // REVIEW: This is done for development ease but shouldn't be here in production
        builder.Services.AddMigration<CatalogContext, CatalogContextSeed>();

        // Add the integration services that consume the DbContext
        builder.Services.AddTransient<IIntegrationEventLogService, IntegrationEventLogService<CatalogContext>>();

        builder.Services.AddTransient<ICatalogIntegrationEventService, CatalogIntegrationEventService>();

        builder.AddRabbitMqEventBus("eventbus")
               .AddSubscription<OrderStatusChangedToAwaitingValidationIntegrationEvent, OrderStatusChangedToAwaitingValidationIntegrationEventHandler>()
               .AddSubscription<OrderStatusChangedToPaidIntegrationEvent, OrderStatusChangedToPaidIntegrationEventHandler>();

        builder.Services.AddOptions<CatalogOptions>()
            .BindConfiguration(nameof(CatalogOptions));

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

        builder.Services.AddSingleton<ICatalogAI, CatalogAI>();
    }
}
