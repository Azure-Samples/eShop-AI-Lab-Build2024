using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;

namespace eShop.Catalog.API.Services;

/// <param name="environment">The web host environment.</param>
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
