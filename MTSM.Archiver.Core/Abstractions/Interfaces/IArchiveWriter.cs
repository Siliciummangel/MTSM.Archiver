using MTSM.Archiver.Core.Abstractions.Models;

namespace MTSM.Archiver.Core.Abstractions.Interfaces
{
    /// <summary>
    /// Creates an archive or export artifact from a collection of archive items.
    /// </summary>
    /// <remarks>
    /// Implementations may produce ZIP archives, TAR archives,
    /// SQL dumps, CSV exports, Parquet files, or other formats.
    /// </remarks>
    public interface IArchiveWriter
    {
        Task<ArchiveWriteResult> WriteAsync(
            ArchiveWriteContext context,
            CancellationToken cancellationToken = default);
    }
}
