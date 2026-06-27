using MTSM.Archiver.Core.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTSM.Archiver.Core.Abstractions.Interfaces
{
    /// <summary>
    /// Resolves archive file names from destination context information.
    /// </summary>
    public interface IArchiveFileNameResolver
    {
        /// <summary>
        /// Resolves the archive file name including the archive format extension.
        /// </summary>
        string ResolveFileName(ArchiveDestinationContext context);
    }
}
