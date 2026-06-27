using System;
using System.Collections.Generic;
using System.Text;

namespace MTSM.Archiver.Core.Abstractions.Models
{
    /// <summary>
    /// Represents the result of an archive verification operation.
    /// </summary>
    public sealed class ArchiveVerificationResult
    {
        /// <summary>
        /// Gets a value indicating whether the archive verification succeeded.
        /// </summary>
        public bool IsValid { get; init; }

        /// <summary>
        /// Gets an optional message describing the verification result.
        /// </summary>
        public string? Message { get; init; }

        /// <summary>
        /// Gets the calculated checksum if available.
        /// </summary>
        public string? Checksum { get; init; }
    }
}
