using MTSM.Archiver.Core.Config.Models;

namespace MTSM.Archiver.Core.Abstractions.Models
{
    /// <summary>
    /// Contains the information required to open an archive destination for writing.
    /// </summary>
    public sealed class ArchiveDestinationContext
    {
        /// <summary>
        /// Gets the unique identifier of the current archive execution run.
        /// </summary>
        public Guid RunId { get; init; }

        /// <summary>
        /// Gets the archive target configuration.
        /// </summary>
        public required ArchiveTargetConfig ArchiveTarget { get; init; }
    }
}
