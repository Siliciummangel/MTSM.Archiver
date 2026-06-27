using System;
using System.Collections.Generic;
using System.Text;

namespace MTSM.Archiver.Core.Abstractions.Models
{
    /// <summary>
    /// Represents the result of an archive writer operation.
    /// </summary>
    public sealed class ArchiveWriteResult
    {
        /// <summary>
        /// Gets the provider-specific location of the generated archive artifact.
        /// This value may be a local file path, URI, object key, blob name,
        /// remote path, or another stable identifier understood by the matching provider.
        /// </summary>
        public string ArchiveLocation { get; init; } = string.Empty;

        /// <summary>
        /// Gets the size of the generated archive in bytes if known.
        /// </summary>
        public long? Size { get; init; }

        /// <summary>
        /// Gets an optional checksum of the generated archive.
        /// </summary>
        public string? Checksum { get; init; }
    }
}
