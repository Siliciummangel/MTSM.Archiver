using MTSM.Archiver.Core.Abstractions.Models;
using MTSM.Archiver.Core.Execution.Models;
using MTSM.Archiver.Core.Writers.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTSM.Archiver.Core.PostActions.Models
{
    /// <summary>
    /// Contains all information required to execute a post-archive action.
    /// </summary>
    public sealed class PostArchiveActionContext
    {
        /// <summary>
        /// Gets the archive execution context.
        /// </summary>
        public required ArchiveExecutionContext Execution { get; init; }

        /// <summary>
        /// Gets the result of the archive writer operation.
        /// </summary>
        public required ArchiveWriteResult WriteResult { get; init; }

        /// <summary>
        /// Gets the items that were successfully archived.
        /// </summary>
        public IAsyncEnumerable<ArchiveItem> ProcessedItems { get; init; } = AsyncEnumerable.Empty<ArchiveItem>();
    }
}
