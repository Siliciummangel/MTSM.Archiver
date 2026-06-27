using MTSM.Archiver.Core.Config.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTSM.Archiver.Core.Abstractions.Models
{
    /// <summary>
    /// Contains runtime information for an archive job execution.
    /// </summary>
    public sealed class ArchiveExecutionContext
    {
        /// <summary>
        /// Gets the unique identifier of the execution run.
        /// </summary>
        public Guid RunId { get; init; } = Guid.NewGuid();

        /// <summary>
        /// Gets the archive job configuration being executed.
        /// </summary>
        public required ArchiveJobConfig Job { get; init; }

        /// <summary>
        /// Gets the UTC timestamp when the execution started.
        /// </summary>
        public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;
    }
}
