using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Reflection;
using System.Text;

namespace MTSM.Archiver.Cli.Commands
{
    /// <summary>
    /// Provides version related CLI commands.
    /// </summary>
    public static class VersionCommand
    {
        /// <summary>
        /// Creates the version command.
        ///
        /// Example:
        /// <code>
        /// mtsm-archiver version
        /// </code>
        /// </summary>
        /// <returns>
        /// Version command instance.
        /// </returns>
        public static Command Create()
        {
            var command = new Command("version", "Displays the version of the MTSM.Archiver CLI");

            command.SetAction(_ =>
            {
                var version = Assembly
                    .GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion
                    ?? "unknown";

                var cleanVersion = version.Split('+')[0];

                Console.WriteLine($"MTSM.Archiver CLI version: {cleanVersion}");

                return CliExitCodes.Success;
            });

            return command;
        }
    }
}
