using System;
using System.Collections.Generic;
using System.Text;

namespace MTSM.Archiver.Cli
{
    /// <summary>
    /// Standard exit codes used by the command line interface.
    ///
    /// Exit codes are intended for:
    /// - Interactive usage
    /// - Automation
    /// - Scheduled execution
    /// - CI/CD pipelines
    /// </summary>
    public static class CliExitCodes
    {
        /// <summary>
        /// Command completed successfully.
        /// </summary>
        public const int Success = 0;
        /// <summary>
        /// Command failed.
        /// </summary>
        public const int Error = 1;
    }
}
