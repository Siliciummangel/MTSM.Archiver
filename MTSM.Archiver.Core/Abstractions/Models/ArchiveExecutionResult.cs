using System;
using System.Collections.Generic;
using System.Text;

namespace MTSM.Archiver.Core.Abstractions.Models
{
    /// <summary>
    /// Represents the result of an archive job execution.
    /// </summary>
    public sealed class ArchiveExecutionResult
    {
        /// <summary>
        /// Gets the name of the executed job.
        /// </summary>
        public string JobName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the final execution status.
        /// </summary>
        public ArchiveExecutionStatus Status { get; init; }

        /// <summary>
        /// Gets the generated archive path if available.
        /// </summary>
        public string? ArchivePath { get; init; }

        /// <summary>
        /// Gets informational or error messages produced during execution.
        /// </summary>
        public List<string> Messages { get; init; } = [];
    }
}
