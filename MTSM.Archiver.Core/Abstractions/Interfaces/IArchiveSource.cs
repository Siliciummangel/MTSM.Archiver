using MTSM.Archiver.Core.Config.Models;
using MTSM.Archiver.Core.Abstractions.Models;

namespace MTSM.Archiver.Core.Abstractions.Interfaces
{
    /// <summary>
    /// Provides archive items from a specific source implementation.
    /// </summary>
    /// <remarks>
    /// Sources may represent file systems, databases, S3 buckets,
    /// or any other supported data provider.
    /// </remarks>
    public interface IArchiveSource
    {
        IAsyncEnumerable<ArchiveItem> GetItemsAsync(
            SourceConfig source,
            FileSelectionConfig selection,
            CancellationToken cancellationToken = default);

        Task<Stream> OpenReadAsync(
            ArchiveItem item,
            CancellationToken cancellationToken = default);
    }
}
