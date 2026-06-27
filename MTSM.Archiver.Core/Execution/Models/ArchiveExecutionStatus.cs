using System;
using System.Collections.Generic;
using System.Text;

namespace MTSM.Archiver.Core.Execution.Models
{
    /// <summary>
    /// Defines the possible outcomes of an archive job execution.
    /// </summary>
    public enum ArchiveExecutionStatus
    {
        /// <summary>
        /// The execution completed successfully.
        /// </summary>
        Succeeded,

        /// <summary>
        /// The execution failed.
        /// </summary>
        Failed,

        /// <summary>
        /// The execution completed with partial success.
        /// </summary>
        PartiallySucceeded,

        /// <summary>
        /// The execution was skipped.
        /// </summary>
        Skipped
    }
}
