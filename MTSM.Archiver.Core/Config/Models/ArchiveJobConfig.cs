namespace MTSM.Archiver.Core.Config.Models
{
    /// <summary>
    /// Represents an archive job configuration file.
    /// This type contains the executable archive job definition together with
    /// configuration-file-level metadata such as schedules.
    /// </summary>
    public sealed class ArchiveJobFile
    {
        /// <summary>
        /// Gets the executable archive job definition.
        /// </summary>
        public required ArchiveJobConfig Job { get; init; }

        /// <summary>
        /// Gets the schedules that define when the archive job should be executed.
        /// Scheduling is handled outside of the archive job executor.
        /// </summary>
        public List<ScheduleConfig> Schedules { get; init; } = [];
    }

    /// <summary>
    /// Represents the executable definition of a single archive job.
    /// A job defines what should be archived, where the archive should be written,
    /// and what should happen to source files after a successful archive operation.
    /// </summary>
    public class ArchiveJobConfig
    {
        /// <summary>
        /// Gets the unique name of the archive job.
        /// </summary>
        public required string Name { get; init; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether this job is enabled and may be executed.
        /// </summary>
        public bool Enabled { get; init; } = true;

        /// <summary>
        /// Gets the source location settings.
        /// </summary>
        public required SourceConfig Source { get; init; } = new();

        /// <summary>
        /// Gets the file selection and filtering rules.
        /// </summary>
        public FileSelectionConfig Selection { get; init; } = new();

        /// <summary>
        /// Gets the archive output settings.
        /// </summary>
        public ArchiveTargetConfig Archive { get; init; } = new();

        /// <summary>
        /// Gets the behavior that should be applied to source files after archiving.
        /// </summary>
        public SourceFileBehaviorConfig SourceFileBehavior { get; init; } = new();

        /// <summary>
        /// Gets the execution-related settings for this archive job.
        /// </summary>
        public ExecutionConfig Execution { get; init; } = new();
    }

    /// <summary>
    /// Defines where files are collected from.
    /// </summary>
    public class SourceConfig
    {
        /// <summary>
        /// Source directory containing files to process.
        /// </summary>
        public string Path { get; init; } = string.Empty;

        /// <summary>
        /// Indicates whether subdirectories should be searched recursively.
        /// </summary>
        public bool Recursive { get; init; } = true;
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
        public int? MinimumAgeDays { get; init; }

        /// <summary>
        /// Maximum file age in days.
        /// Files older than this value are ignored.
        /// </summary>
        public int? MaximumAgeDays { get; init; }

        /// <summary>
        /// Allowed file extensions.
        /// Example: ".pdf", ".txt"
        /// </summary>
        public List<string> Extensions { get; init; } = [];

        /// <summary>
        /// Allowed filename prefixes.
        /// </summary>
        public List<string> StartsWith { get; init; } = [];

        /// <summary>
        /// Allowed filename suffixes.
        /// </summary>
        public List<string> EndsWith { get; init; } = [];

        /// <summary>
        /// Regular expression patterns applied to filenames.
        /// </summary>
        public List<string> RegexPatterns { get; init; } = [];

        /// <summary>
        /// When true, all configured rules must match.
        /// When false, a single matching rule is sufficient.
        /// </summary>
        public bool MatchAllRules { get; init; } = false;
    }

    /// <summary>
    /// Defines archive destination and naming settings.
    /// </summary>
    public class ArchiveTargetConfig
    {
        /// <summary>
        /// Destination directory where archives will be written.
        /// </summary>
        public string TargetDirectory { get; init; } = string.Empty;

        /// <summary>
        /// Archive filename pattern.
        /// Example: {jobName}_{yyyyMMdd_HHmmss}
        /// </summary>
        public string FileNamePattern { get; init; } = "{jobName}_{yyyyMMdd_HHmmss}";

        /// <summary>
        /// Archive format to create.
        /// </summary>
        public ArchiveFormat Format { get; init; } = ArchiveFormat.Zip;

        /// <summary>
        /// Behavior when the target archive already exists.
        /// </summary>
        public ExistingArchiveBehavior ExistingArchiveBehavior { get; init; } = ExistingArchiveBehavior.Fail;
    }

    /// <summary>
    /// Defines post-processing actions for source files.
    /// </summary>
    public class SourceFileBehaviorConfig
    {
        /// <summary>
        /// Action performed after a successful archive operation.
        /// </summary>
        public SourceFileAction AfterSuccessfulArchive { get; init; } = SourceFileAction.Keep;

        /// <summary>
        /// Destination directory when using the Move action.
        /// </summary>
        public string? MoveToDirectory { get; init; }
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
        public string? Name { get; init; }

        /// <summary>
        /// Indicates whether this schedule is active.
        /// Disabled schedules are ignored.
        /// </summary>
        public bool Enabled { get; init; } = true;

        /// <summary>
        /// Cron expression defining when the job should run.
        /// 
        /// Example:
        /// 0 2 * * *     = every day at 02:00
        /// 0 * * * *     = every hour
        /// 0 2 * * 0     = every Sunday at 02:00
        /// </summary>
        public string CronExpression { get; init; } = "0 2 * * *";
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
        public bool DryRun { get; init; } = true;
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
