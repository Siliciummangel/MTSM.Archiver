using MTSM.Archiver.Core.Config;
using System.CommandLine;
using MTSM.Archiver.Core.Abstractions.Models;
using MTSM.Archiver.Core.Providers.Naming;

namespace MTSM.Archiver.Cli.Commands
{
    /// <summary>
    /// Provides configuration related CLI commands.
    ///
    /// Current commands:
    /// - validate
    ///
    /// Future commands may include:
    /// - show
    /// - generate
    /// - migrate
    /// </summary>
    public static class ConfigCommand
    {
        /// <summary>
        /// Creates the root configuration command and all subcommands.
        ///
        /// Example:
        /// <code>
        /// mtsm-archiver config validate
        /// mtsm-archiver config validate --config root-config.yaml
        /// mtsm-archiver config validate --check-path-exists
        /// </code>
        /// </summary>
        /// <returns>
        /// Config command instance.
        /// </returns>
        public static Command Create()
        {
            var configCommand = new Command("config", "Configuration related commands");

            var validateCommand = new Command("validate", "Loads and validates the archive configuration");

            var configOption = new Option<FileInfo>("--config")
            {
                Description = "Path to the root configuration file",
                DefaultValueFactory = _ => new FileInfo("root-config.yaml")
            };

            var checkPathExistsOption = new Option<bool>("--check-path-exists")
            {
                Description = "Checks whether configured directories exist on the current system"
            };

            validateCommand.Options.Add(configOption);
            validateCommand.Options.Add(checkPathExistsOption);

            validateCommand.SetAction(parseResult =>
            {
                var configFile = parseResult.GetValue(configOption);
                var checkPathExists = parseResult.GetValue(checkPathExistsOption);

                return RunValidate(configFile!, checkPathExists);
            });

            configCommand.Subcommands.Add(validateCommand);

            var previewDestinationCommand = new Command(
                "preview-destination",
                "Displays the resolved archive destination path for a configured archive job without creating an archive file");

            var jobOption = new Option<string>("--job")
            {
                Description = "Name of the archive job to preview",
                Required = true
            };

            previewDestinationCommand.Options.Add(configOption);
            previewDestinationCommand.Options.Add(jobOption);

            previewDestinationCommand.SetAction(parseResult =>
            {
                var configFile = parseResult.GetValue(configOption);
                var jobName = parseResult.GetValue(jobOption);

                return RunPreviewDestination(configFile!, jobName!);
            });

            configCommand.Subcommands.Add(previewDestinationCommand);

            return configCommand;
        }

        /// <summary>
        /// Loads and validates the archive configuration.
        ///
        /// Validation consists of two stages:
        /// 1. Loading and deserializing YAML files.
        /// 2. Business validation using <see cref="ArchiveConfigValidator"/>.
        ///
        /// When <paramref name="checkPathExists"/> is enabled,
        /// configured source and destination paths are additionally
        /// checked against the current operating system.
        ///
        /// </summary>
        /// <param name="configFile">
        /// Root configuration file.
        /// </param>
        /// <param name="checkPathExists">
        /// Indicates whether configured paths should be verified
        /// against the current system.
        /// </param>
        /// <returns>
        /// CLI exit code.
        /// </returns>
        private static int RunValidate(FileInfo configFile, bool checkPathExists)
        {
            try
            {
                Console.WriteLine(checkPathExists
                    ? "Validation mode: configuration and environment"
                    : "Validation mode: configuration only");

                Console.WriteLine();

                var loader = new ArchiveConfigLoader();
                var config = loader.Load(configFile.FullName);

                var validationErrors = ArchiveConfigValidator.Validate(config, checkPathExists);

                if (validationErrors.Count > 0)
                {
                    Console.Error.WriteLine("Configuration is invalid.");
                    Console.Error.WriteLine();

                    foreach (var error in validationErrors)
                        Console.Error.WriteLine($"- {error}");

                    return CliExitCodes.Error;
                }

                Console.WriteLine("Configuration is valid.");
                Console.WriteLine($"Root config: {config.Root.Name}");
                Console.WriteLine($"Jobs loaded: {config.Jobs.Count}");

                return CliExitCodes.Success;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to load configuration.");
                Console.Error.WriteLine(ex.Message);

                if (ex.InnerException is not null)
                    Console.Error.WriteLine(ex.InnerException.Message);

                return CliExitCodes.Error;
            }
        }

        /// <summary>
        /// Loads the archive configuration and displays the resolved destination path
        /// for the specified archive job without creating an archive file.
        /// </summary>
        /// <param name="configFile">The root configuration file to load.</param>
        /// <param name="jobName">The name of the archive job to preview.</param>
        /// <returns>
        /// <see cref="CliExitCodes.Success"/> when the preview was created successfully;
        /// otherwise <see cref="CliExitCodes.Error"/>.
        /// </returns>
        private static int RunPreviewDestination(FileInfo configFile, string jobName)
        {
            try
            {
                var loader = new ArchiveConfigLoader();
                var config = loader.Load(configFile.FullName);

                var job = config.Jobs
                    .FirstOrDefault(x => string.Equals(
                        x.Job.Name,
                        jobName,
                        StringComparison.OrdinalIgnoreCase));

                if (job is null)
                {
                    Console.Error.WriteLine($"Archive job '{jobName}' was not found.");
                    return CliExitCodes.Error;
                }

                var context = new ArchiveDestinationContext
                {
                    RunId = Guid.NewGuid(),
                    StartedAt = DateTimeOffset.UtcNow,
                    ArchiveTarget = job.Job.Archive
                };

                var resolver = new DefaultArchiveFileNameResolver();
                var fileName = resolver.ResolveFileName(context);

                var targetDirectory = Path.GetFullPath(job.Job.Archive.TargetDirectory);
                var fullPath = Path.Combine(targetDirectory, fileName);

                Console.WriteLine("Archive destination preview");
                Console.WriteLine();
                Console.WriteLine($"Job:              {job.Job.Name}");
                Console.WriteLine($"Run ID:           {context.RunId}");
                Console.WriteLine($"Started at UTC:   {context.StartedAt:O}");
                Console.WriteLine($"Target directory: {targetDirectory}");
                Console.WriteLine($"File name:        {fileName}");
                Console.WriteLine($"Full path:        {fullPath}");

                return CliExitCodes.Success;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to preview archive destination.");
                Console.Error.WriteLine(ex.Message);

                if (ex.InnerException is not null)
                    Console.Error.WriteLine(ex.InnerException.Message);

                return CliExitCodes.Error;
            }
        }
    }
}
