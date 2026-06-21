using System;
using System.Collections.Generic;
using System.Text;

namespace MTSM.Archiver.Core.Config
{
    /// <summary>
    /// Root configuration of MTSM.Archiver.
    /// 
    /// This configuration is loaded first and is responsible for
    /// discovering archive job configurations.
    /// </summary>
    public class RootConfig
    {
        /// <summary>
        /// Name of the archiver instance.
        /// Mainly used for identification and display purposes.
        /// </summary>
        public string Name { get; set; } = "MTSM.Archiver";

        /// <summary>
        /// Directories containing archive job configuration files.
        /// 
        /// All supported job configuration files found in these
        /// directories will be discovered and loaded by the application.
        /// </summary>
        public List<string> JobConfigDirectories { get; set; } = [];

        /// <summary>
        /// Default directory used when creating new archive job
        /// configurations through CLI commands or a future web interface.
        /// </summary>
        public string? DefaultJobConfigDirectory { get; set; }
    }
}
