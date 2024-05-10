namespace eShop.Catalog.API.Services;

/// <param name="environment">The web host environment.</param>
public sealed class CatalogAI() : ICatalogAI
{
    internal static int EmbeddingDimensions = 384;
    internal static string MemoryCollection = "catalogMemory";

    /// <inheritdoc/>
    public bool IsEnabled => false;
}
