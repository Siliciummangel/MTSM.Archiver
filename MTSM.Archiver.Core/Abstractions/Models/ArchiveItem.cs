using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace MTSM.Archiver.Core.Abstractions.Models
{
    /// <summary>
    /// Represents a logical item that can be archived.
    /// </summary>
    /// <remarks>
    /// An archive item may originate from a file system,
    /// database, S3 bucket, or any other supported source.
    /// </remarks>
    public sealed class ArchiveItem
    {
        /// <summary>
        /// Gets the unique identifier of the source item.
        /// </summary>
        public string SourceIdentifier { get; init; } = string.Empty;

        /// <summary>
        /// Gets the path or name the item should have inside the archive.
        /// </summary>
        public string ArchivePath { get; init; } = string.Empty;

        /// <summary>
        /// Gets the logical type of the archive item.
        /// </summary>
        public ArchiveItemKind Kind { get; init; } = ArchiveItemKind.File;

        /// <summary>
        /// Gets the size of the item in bytes if known.
        /// </summary>
        public long Size { get; init; } = 0;

        /// <summary>
        /// Gets optional metadata associated with the item.
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata { get; init; }
            = ImmutableDictionary<string, string>.Empty;

        /// <summary>
        /// Gets an optional delegate that opens a readable stream for the item's content.
        /// If not specified, the provider is responsible for resolving the content by using
        /// <see cref="SourceIdentifier"/>.
        /// </summary>
        public Func<CancellationToken, ValueTask<Stream>>? OpenReadAsync { get; init; }
    }
}
