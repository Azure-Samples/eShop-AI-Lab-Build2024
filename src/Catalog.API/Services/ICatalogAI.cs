using Microsoft.SemanticKernel.Memory;

namespace eShop.Catalog.API.Services;

public interface ICatalogAI
{
    /// <summary>Gets whether the AI system is enabled.</summary>
    bool IsEnabled { get; }

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
}
