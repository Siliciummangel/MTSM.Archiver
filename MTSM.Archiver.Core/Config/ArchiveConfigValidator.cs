using MTSM.Archiver.Core.Config.Models;
using System.Text.RegularExpressions;

namespace MTSM.Archiver.Core.Config
{
    /// <summary>
    /// Performs business validation of a loaded archive configuration.
    ///
    /// This validator is responsible for ensuring that
    /// configuration values are logically correct and usable
    /// before archive execution begins.
    ///
    /// Validation is intentionally separated from the
    /// configuration loader.
    ///
    /// The loader ensures that YAML files can be parsed.
    ///
    /// The validator ensures that the resulting configuration
    /// is meaningful and executable.
    /// </summary>
    public static class ArchiveConfigValidator
    {
        /// <summary>
        /// Validates the complete runtime configuration.
        ///
        /// </summary>
        /// <param name="config">
        /// Runtime configuration returned by the configuration loader.
        /// </param>
        /// <param name="checkPathExists">
        /// When enabled, configured filesystem paths are checked
        /// against the current environment.
        ///
        /// When disabled, only syntactic and business validation
        /// is performed.
        /// </param>
        /// <returns>
        /// Collection of validation errors.
        ///
        /// An empty collection indicates a valid configuration.
        /// </returns>
        public static List<string> Validate(
            ArchiveRuntimeConfig config,
            bool checkPathExists = false)
        {
            var errors = new List<string>();

            ValidateRoot(config.Root, errors);

            if (config.Jobs.Count == 0)
                errors.Add("No archive jobs were loaded.");

            foreach (var job in config.Jobs)
                ValidateJob(job, errors, checkPathExists);

            return errors;
        }

        /// <summary>
        /// Validates global root configuration settings.
        /// </summary>
        private static void ValidateRoot(RootConfig root, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(root.Name))
                errors.Add("Root config: Name is missing.");

            if (root.JobConfigDirectories.Count == 0)
                errors.Add("Root config: No job config directories configured.");

            foreach (var directory in root.JobConfigDirectories)
            {
                if (string.IsNullOrWhiteSpace(directory))
                    errors.Add("Root config: Job config directory contains an empty entry.");
            }

            if (!string.IsNullOrWhiteSpace(root.DefaultJobConfigDirectory) &&
                !root.JobConfigDirectories.Contains(root.DefaultJobConfigDirectory))
            {
                errors.Add("Root config: Default job config directory is not part of the configured job config directories.");
            }
        }

        /// <summary>
        /// Validates a single archive job.
        /// </summary>
        private static void ValidateJob(
            ArchiveJobFile jobFile,
            List<string> errors,
            bool checkPathExists)
        {
            var jobName = string.IsNullOrWhiteSpace(jobFile.Job.Name)
                ? "<unnamed>"
                : jobFile.Job.Name;

            if (string.IsNullOrWhiteSpace(jobFile.Job.Name))
                errors.Add("Job config: Job name is missing.");

            if (string.IsNullOrWhiteSpace(jobFile.Job.Archive.FileNamePattern))
                errors.Add($"Job '{jobName}': Archive file name pattern is missing.");

            ValidateSelection(jobFile.Job, jobName, errors);
            ValidateSourceFileBehavior(jobFile.Job, jobName, errors);
            ValidateSchedules(jobFile, jobName, errors);

            ValidatePath(jobFile.Job.Source.Path, "Source path", jobName, errors, mustExist: checkPathExists);
            ValidatePath(jobFile.Job.Archive.TargetDirectory, "Archive target directory", jobName, errors, mustExist: false);
            if (jobFile.Job.SourceFileBehavior.AfterSuccessfulArchive == SourceFileAction.Move)
            {
                ValidatePath(
                    jobFile.Job.SourceFileBehavior.MoveToDirectory,
                    "Move target directory",
                    jobName,
                    errors,
                    mustExist: checkPathExists);
            }
        }

        /// <summary>
        /// Validates file selection and filtering rules.
        /// </summary>
        private static void ValidateSelection(ArchiveJobConfig job, string jobName, List<string> errors)
        {
            if (job.Selection.MinimumAgeDays is < 0)
                errors.Add($"Job '{jobName}': Minimum age days cannot be negative.");

            if (job.Selection.MaximumAgeDays is < 0)
                errors.Add($"Job '{jobName}': Maximum age days cannot be negative.");

            if (job.Selection.MinimumAgeDays.HasValue &&
                job.Selection.MaximumAgeDays.HasValue &&
                job.Selection.MinimumAgeDays > job.Selection.MaximumAgeDays)
            {
                errors.Add($"Job '{jobName}': Minimum age days cannot be greater than maximum age days.");
            }

            foreach (var extension in job.Selection.Extensions)
            {
                if (string.IsNullOrWhiteSpace(extension))
                    errors.Add($"Job '{jobName}': File extension filter contains an empty entry.");

                if (!string.IsNullOrWhiteSpace(extension) && !extension.StartsWith('.'))
                    errors.Add($"Job '{jobName}': File extension '{extension}' must start with '.'.");
            }

            foreach (var pattern in job.Selection.RegexPatterns)
            {
                if (string.IsNullOrWhiteSpace(pattern))
                {
                    errors.Add($"Job '{jobName}': Regex pattern contains an empty entry.");
                    continue;
                }

                try
                {
                    _ = new Regex(pattern);
                }
                catch
                {
                    errors.Add($"Job '{jobName}': Regex pattern '{pattern}' is invalid.");
                }
            }

            foreach (var startsWith in job.Selection.StartsWith)
            {
                if (string.IsNullOrWhiteSpace(startsWith))
                    errors.Add($"Job '{jobName}': StartsWith filter contains an empty entry.");
            }

            foreach (var endsWith in job.Selection.EndsWith)
            {
                if (string.IsNullOrWhiteSpace(endsWith))
                    errors.Add($"Job '{jobName}': EndsWith filter contains an empty entry.");
            }
        }

        /// <summary>
        /// Validates source file handling behavior after
        /// successful archive creation.
        /// </summary>
        private static void ValidateSourceFileBehavior(ArchiveJobConfig job, string jobName, List<string> errors)
        {
            if (job.SourceFileBehavior.AfterSuccessfulArchive == SourceFileAction.Move &&
                string.IsNullOrWhiteSpace(job.SourceFileBehavior.MoveToDirectory))
            {
                errors.Add($"Job '{jobName}': Move target directory is required when source file action is Move.");
            }
        }

        /// <summary>
        /// Validates configured execution schedules.
        /// </summary>
        private static void ValidateSchedules(ArchiveJobFile jobFile, string jobName, List<string> errors)
        {
            foreach (var schedule in jobFile.Schedules.Where(s => s.Enabled))
            {
                if (string.IsNullOrWhiteSpace(schedule.CronExpression))
                    errors.Add($"Job '{jobName}': Enabled schedule has no cron expression.");
            }
        }

        /// <summary>
        /// Validates a filesystem path.
        ///
        /// Validation includes:
        /// - Empty path checks
        /// - Path syntax validation
        /// - Optional existence checks
        /// </summary>
        /// <param name="path">
        /// Path to validate.
        /// </param>
        /// <param name="label">
        /// Human readable path description used in error messages.
        /// </param>
        /// <param name="jobName">
        /// Related job name.
        /// </param>
        /// <param name="errors">
        /// Validation error collection.
        /// </param>
        /// <param name="mustExist">
        /// Indicates whether the path must already exist.
        /// </param>
        private static void ValidatePath(
            string? path,
            string label,
            string jobName,
            List<string> errors,
            bool mustExist)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                errors.Add($"Job '{jobName}': {label} is missing.");
                return;
            }

            try
            {
                _ = Path.GetFullPath(path);
            }
            catch (Exception ex) when (
                ex is ArgumentException ||
                ex is NotSupportedException ||
                ex is PathTooLongException)
            {
                errors.Add($"Job '{jobName}': {label} is not a valid path.");
                return;
            }

            if (mustExist && !Directory.Exists(path))
                errors.Add($"Job '{jobName}': {label} does not exist.");
        }
    }
}
