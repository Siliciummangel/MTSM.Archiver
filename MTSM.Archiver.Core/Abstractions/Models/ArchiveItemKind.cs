using System;
using System.Collections.Generic;
using System.Text;

namespace MTSM.Archiver.Core.Abstractions.Models
{
    /// <summary>
    /// Defines the logical type of an archive item.
    /// </summary>
    public enum ArchiveItemKind
    {
        /// <summary>
        /// Represents a file.
        /// </summary>
        File,

        /// <summary>
        /// Represents a directory.
        /// </summary>
        Directory,

        /// <summary>
        /// Represents generated content that does not directly originate
        /// from a physical file system object.
        /// </summary>
        GeneratedContent
    }
}
