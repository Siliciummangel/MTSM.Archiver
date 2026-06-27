using MTSM.Archiver.Core.Abstractions.Models;

namespace MTSM.Archiver.Core.Abstractions.Interfaces
{
    /// <summary>
    /// Provides access to a destination where archive artifacts can be written.
    /// </summary>
    public interface IArchiveDestination
    {
        /// <summary>
        /// Opens a writable stream to the destination. 
        /// The caller is responsible for disposing the returned stream.
        /// </summary>
        ValueTask<Stream> OpenWriteAsync(
            ArchiveDestinationContext context,
            CancellationToken cancellationToken = default);
    }
}
