using System;
using System.Collections.Generic;
using System.Text;

namespace MTSM.Archiver.Core.Config
{
    /// <summary>
    /// Represents a single archive job definition.
    /// A job describes which files should be processed,
    /// how they should be archived and what should happen afterwards.
    /// </summary>
    public class ArchiveJobConfig
    {
        /// <summary>
        /// Unique name of the archive job.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether this job is enabled and should be executed.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Source location settings.
        /// </summary>
        public SourceConfig Source { get; set; } = new();

        /// <summary>
        /// File selection and filtering rules.
        /// </summary>
        public FileSelectionConfig Selection { get; set; } = new();

        /// <summary>
        /// Archive output settings.
        /// </summary>
        public ArchiveTargetConfig Archive { get; set; } = new();

        /// <summary>
        /// Defines what should happen to source files after archiving.
        /// </summary>
        public SourceFileBehaviorConfig SourceFileBehavior { get; set; } = new();

        /// <summary>
        /// Defines when this archive job should be executed.
        /// Multiple schedules may be configured.
        /// </summary>
        public List<ScheduleConfig> Schedules { get; set; } = [];

        /// <summary>
        /// Execution-related settings.
        /// </summary>
        public ExecutionConfig Execution { get; set; } = new();
    }

    /// <summary>
    /// Defines where files are collected from.
    /// </summary>
    public class SourceConfig
    {
        /// <summary>
        /// Source directory containing files to process.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether subdirectories should be searched recursively.
        /// </summary>
        public bool Recursive { get; set; } = true;
    }

    /// <summary>
    /// Defines file filtering rules.
    /// </summary>
    public class FileSelectionConfig
    {
        /// <summary>
        /// Minimum file age in days.
        /// Files newer than this value are ignored.
        /// </summary>
        public int? MinimumAgeDays { get; set; }

        /// <summary>
        /// Maximum file age in days.
        /// Files older than this value are ignored.
        /// </summary>
        public int? MaximumAgeDays { get; set; }

        /// <summary>
        /// Allowed file extensions.
        /// Example: ".pdf", ".txt"
        /// </summary>
        public List<string> Extensions { get; set; } = [];

        /// <summary>
        /// Allowed filename prefixes.
        /// </summary>
        public List<string> StartsWith { get; set; } = [];

        /// <summary>
        /// Allowed filename suffixes.
        /// </summary>
        public List<string> EndsWith { get; set; } = [];

        /// <summary>
        /// Regular expression patterns applied to filenames.
        /// </summary>
        public List<string> RegexPatterns { get; set; } = [];

        /// <summary>
        /// When true, all configured rules must match.
        /// When false, a single matching rule is sufficient.
        /// </summary>
        public bool MatchAllRules { get; set; } = false;
    }

    /// <summary>
    /// Defines archive destination and naming settings.
    /// </summary>
    public class ArchiveTargetConfig
    {
        /// <summary>
        /// Destination directory where archives will be written.
        /// </summary>
        public string TargetDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Archive filename pattern.
        /// Example: {jobName}_{yyyyMMdd_HHmmss}
        /// </summary>
        public string FileNamePattern { get; set; } =
            "{jobName}_{yyyyMMdd_HHmmss}";

        /// <summary>
        /// Archive format to create.
        /// </summary>
        public ArchiveFormat Format { get; set; } = ArchiveFormat.Zip;

        /// <summary>
        /// Behavior when the target archive already exists.
        /// </summary>
        public ExistingArchiveBehavior ExistingArchiveBehavior { get; set; } = ExistingArchiveBehavior.Fail;
    }

    /// <summary>
    /// Defines post-processing actions for source files.
    /// </summary>
    public class SourceFileBehaviorConfig
    {
        /// <summary>
        /// Action performed after a successful archive operation.
        /// </summary>
        public SourceFileAction AfterSuccessfulArchive { get; set; } = SourceFileAction.Keep;

        /// <summary>
        /// Destination directory when using the Move action.
        /// </summary>
        public string? MoveToDirectory { get; set; }
    }

    /// <summary>
    /// Defines a schedule that can trigger execution of an archive job.
    /// 
    /// Scheduling is based on standard cron expressions.
    /// </summary>
    public class ScheduleConfig
    {
        /// <summary>
        /// Optional display name of the schedule.
        /// Useful when multiple schedules are configured.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Indicates whether this schedule is active.
        /// Disabled schedules are ignored.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Cron expression defining when the job should run.
        /// 
        /// Example:
        /// 0 2 * * *     = every day at 02:00
        /// 0 * * * *     = every hour
        /// 0 2 * * 0     = every Sunday at 02:00
        /// </summary>
        public string CronExpression { get; set; } = "0 2 * * *";
    }

    /// <summary>
    /// Controls execution behavior.
    /// </summary>
    public class ExecutionConfig
    {
        /// <summary>
        /// When enabled, no files are modified.
        /// Only log output and validation are performed.
        /// </summary>
        public bool DryRun { get; set; } = true;
    }

    /// <summary>
    /// Supported archive formats.
    /// </summary>
    public enum ArchiveFormat
    {
        Zip,
        Tar,
        TarGz
    }

    /// <summary>
    /// Action performed on source files after archiving.
    /// </summary>
    public enum SourceFileAction
    {
        Keep,
        Delete,
        Move
    }

    /// <summary>
    /// Behavior when the destination archive already exists.
    /// </summary>
    public enum ExistingArchiveBehavior
    {
        Fail,
        Overwrite,
        CreateUniqueName
    }
}
