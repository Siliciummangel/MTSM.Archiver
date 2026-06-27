using MTSM.Archiver.Core.Config.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTSM.Archiver.Core.Abstractions.Models
{
    /// <summary>
    /// Provides all information required to create an archive artifact.
    /// </summary>
    public sealed class ArchiveWriteContext
    {
        /// <summary>
        /// Gets the archive job configuration.
        /// </summary>
        public required ArchiveJobConfig Job { get; init; }

        /// <summary>
        /// Gets the streamable collection of items that should be written to the archive.
        /// </summary>
        public IAsyncEnumerable<ArchiveItem> Items { get; init; } = AsyncEnumerable.Empty<ArchiveItem>();

        /// <summary>
        /// Gets the target output stream where the archive data should be written.
        /// The caller retains ownership of this stream and is responsible for its disposal.
        /// </summary>
        public Stream OutputStream { get; init; } = Stream.Null;
    }
}
