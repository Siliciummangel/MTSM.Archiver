using MTSM.Archiver.Core.Config;
using MTSM.Archiver.Core.Config.Models;

namespace MTSM.Archiver.Tests.Config;

/// <summary>
/// Tests for <see cref="ArchiveConfigValidator"/>.
/// Covers success cases, validation failures, edge cases and null handling.
/// </summary>
public class ArchiveConfigValidatorTests
{
    #region Success Cases

    [Fact]
    public void Validate_ValidMinimalConfiguration_ReturnsNoErrors()
    {
        // Arrange
        var config = CreateValidMinimalConfig();

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ValidConfigurationWithMultipleJobs_ReturnsNoErrors()
    {
        // Arrange
        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig>
            {
                CreateValidJobConfig("Job1"),
                CreateValidJobConfig("Job2"),
                CreateValidJobConfig("Job3")
            }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ValidConfigurationWithAllSelectionFilters_ReturnsNoErrors()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig
            {
                Path = @"C:\Source",
                Recursive = true
            },
            Selection = new FileSelectionConfig
            {
                Extensions = new List<string> { ".txt", ".pdf", ".log" },
                StartsWith = new List<string> { "Report_", "Log_" },
                EndsWith = new List<string> { "_backup", "_archive" },
                RegexPatterns = new List<string> { "^[A-Z]{3}_\\d{8}$" },
                MinimumAgeDays = 7,
                MaximumAgeDays = 365
            },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}",
                Format = ArchiveFormat.Zip
            },
            SourceFileBehavior = new SourceFileBehaviorConfig
            {
                AfterSuccessfulArchive = SourceFileAction.Keep
            },
            Schedules = new List<ScheduleConfig>(),
            Execution = new ExecutionConfig { DryRun = true }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ValidConfigurationWithMoveAction_ReturnsNoErrors()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig
            {
                Path = @"C:\Source",
                Recursive = true
            },
            Selection = new FileSelectionConfig(),
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}",
                Format = ArchiveFormat.Zip
            },
            SourceFileBehavior = new SourceFileBehaviorConfig
            {
                AfterSuccessfulArchive = SourceFileAction.Move,
                MoveToDirectory = @"C:\Archive\Moved"
            },
            Schedules = new List<ScheduleConfig>(),
            Execution = new ExecutionConfig { DryRun = true }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ValidConfigurationWithSchedules_ReturnsNoErrors()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source", Recursive = true },
            Selection = new FileSelectionConfig(),
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}",
                Format = ArchiveFormat.Zip
            },
            SourceFileBehavior = new SourceFileBehaviorConfig { AfterSuccessfulArchive = SourceFileAction.Keep },
            Schedules = new List<ScheduleConfig>
            {
                new() { Enabled = true, CronExpression = "0 2 * * *" },
                new() { Enabled = true, CronExpression = "0 14 * * 5" },
                new() { Enabled = false, CronExpression = "" } // Disabled schedules should not be validated
            },
            Execution = new ExecutionConfig { DryRun = true }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ValidRootWithDefaultJobConfigDirectory_ReturnsNoErrors()
    {
        // Arrange
        var root = new RootConfig
        {
            Name = "Test Archiver",
            JobConfigDirectories = new List<string> { @"C:\Config\Jobs", @"C:\Config\Jobs2" },
            DefaultJobConfigDirectory = @"C:\Config\Jobs"
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = root,
            Jobs = new List<ArchiveJobConfig> { CreateValidJobConfig("TestJob") }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Empty(errors);
    }

    #endregion

    #region Root Configuration Validation Failures

    [Fact]
    public void Validate_RootNameMissing_ReturnsError()
    {
        // Arrange
        var config = new ArchiveRuntimeConfig
        {
            Root = new RootConfig
            {
                Name = string.Empty,
                JobConfigDirectories = new List<string> { @"C:\Config\Jobs" }
            },
            Jobs = new List<ArchiveJobConfig> { CreateValidJobConfig("TestJob") }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Root config: Name is missing.", errors);
    }

    [Fact]
    public void Validate_RootNameWhitespace_ReturnsError()
    {
        // Arrange
        var config = new ArchiveRuntimeConfig
        {
            Root = new RootConfig
            {
                Name = "   ",
                JobConfigDirectories = new List<string> { @"C:\Config\Jobs" }
            },
            Jobs = new List<ArchiveJobConfig> { CreateValidJobConfig("TestJob") }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Root config: Name is missing.", errors);
    }

    [Fact]
    public void Validate_NoJobConfigDirectories_ReturnsError()
    {
        // Arrange
        var config = new ArchiveRuntimeConfig
        {
            Root = new RootConfig
            {
                Name = "Test Archiver",
                JobConfigDirectories = new List<string>()
            },
            Jobs = new List<ArchiveJobConfig> { CreateValidJobConfig("TestJob") }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Root config: No job config directories configured.", errors);
    }

    [Fact]
    public void Validate_JobConfigDirectoryContainsEmptyEntry_ReturnsError()
    {
        // Arrange
        var config = new ArchiveRuntimeConfig
        {
            Root = new RootConfig
            {
                Name = "Test Archiver",
                JobConfigDirectories = new List<string> { @"C:\Config\Jobs", string.Empty }
            },
            Jobs = new List<ArchiveJobConfig> { CreateValidJobConfig("TestJob") }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Root config: Job config directory contains an empty entry.", errors);
    }

    [Fact]
    public void Validate_JobConfigDirectoryContainsWhitespaceEntry_ReturnsError()
    {
        // Arrange
        var config = new ArchiveRuntimeConfig
        {
            Root = new RootConfig
            {
                Name = "Test Archiver",
                JobConfigDirectories = new List<string> { @"C:\Config\Jobs", "   " }
            },
            Jobs = new List<ArchiveJobConfig> { CreateValidJobConfig("TestJob") }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Root config: Job config directory contains an empty entry.", errors);
    }

    [Fact]
    public void Validate_DefaultJobConfigDirectoryNotInList_ReturnsError()
    {
        // Arrange
        var config = new ArchiveRuntimeConfig
        {
            Root = new RootConfig
            {
                Name = "Test Archiver",
                JobConfigDirectories = new List<string> { @"C:\Config\Jobs" },
                DefaultJobConfigDirectory = @"C:\SomeOtherPath"
            },
            Jobs = new List<ArchiveJobConfig> { CreateValidJobConfig("TestJob") }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Root config: Default job config directory is not part of the configured job config directories.", errors);
    }

    [Fact]
    public void Validate_NoJobs_ReturnsError()
    {
        // Arrange
        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig>()
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("No archive jobs were loaded.", errors);
    }

    #endregion

    #region Job Configuration Validation Failures

    [Fact]
    public void Validate_JobNameMissing_ReturnsError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = string.Empty,
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Job config: Job name is missing.", errors);
    }

    [Fact]
    public void Validate_JobNameWhitespace_ReturnsError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "   ",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Job config: Job name is missing.", errors);
    }

    [Fact]
    public void Validate_ArchiveFileNamePatternMissing_ReturnsError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = string.Empty
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Job 'TestJob': Archive file name pattern is missing.", errors);
    }

    [Fact]
    public void Validate_SourcePathMissing_ReturnsError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = string.Empty },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Job 'TestJob': Source path is missing.", errors);
    }

    [Fact]
    public void Validate_ArchiveTargetDirectoryMissing_ReturnsError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = string.Empty,
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Job 'TestJob': Archive target directory is missing.", errors);
    }

    #endregion

    #region Selection Validation Failures

    [Fact]
    public void Validate_MinimumAgeDaysNegative_ReturnsError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Selection = new FileSelectionConfig { MinimumAgeDays = -1 },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Job 'TestJob': Minimum age days cannot be negative.", errors);
    }

    [Fact]
    public void Validate_MaximumAgeDaysNegative_ReturnsError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Selection = new FileSelectionConfig { MaximumAgeDays = -5 },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Job 'TestJob': Maximum age days cannot be negative.", errors);
    }

    [Fact]
    public void Validate_MinimumAgeGreaterThanMaximumAge_ReturnsError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Selection = new FileSelectionConfig { MinimumAgeDays = 30, MaximumAgeDays = 10 },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Job 'TestJob': Minimum age days cannot be greater than maximum age days.", errors);
    }

    [Fact]
    public void Validate_ExtensionMissingDot_ReturnsError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Selection = new FileSelectionConfig { Extensions = new List<string> { "txt" } },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Job 'TestJob': File extension 'txt' must start with '.'.", errors);
    }

    [Fact]
    public void Validate_ExtensionEmpty_ReturnsError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Selection = new FileSelectionConfig { Extensions = new List<string> { string.Empty } },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Job 'TestJob': File extension filter contains an empty entry.", errors);
    }

    [Fact]
    public void Validate_InvalidRegexPattern_ReturnsError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Selection = new FileSelectionConfig { RegexPatterns = new List<string> { "[invalid(regex" } },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains(errors, e => e.Contains("Job 'TestJob': Regex pattern '[invalid(regex' is invalid."));
    }

    [Fact]
    public void Validate_RegexPatternEmpty_ReturnsError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Selection = new FileSelectionConfig { RegexPatterns = new List<string> { string.Empty } },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Job 'TestJob': Regex pattern contains an empty entry.", errors);
    }

    [Fact]
    public void Validate_StartsWithEmpty_ReturnsError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Selection = new FileSelectionConfig { StartsWith = new List<string> { string.Empty } },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Job 'TestJob': StartsWith filter contains an empty entry.", errors);
    }

    [Fact]
    public void Validate_EndsWithEmpty_ReturnsError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Selection = new FileSelectionConfig { EndsWith = new List<string> { string.Empty } },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Job 'TestJob': EndsWith filter contains an empty entry.", errors);
    }

    #endregion

    #region Source File Behavior Validation Failures

    [Fact]
    public void Validate_MoveActionWithoutTargetDirectory_ReturnsError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            },
            SourceFileBehavior = new SourceFileBehaviorConfig
            {
                AfterSuccessfulArchive = SourceFileAction.Move,
                MoveToDirectory = null
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Job 'TestJob': Move target directory is required when source file action is Move.", errors);
    }

    [Fact]
    public void Validate_MoveActionWithEmptyTargetDirectory_ReturnsError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            },
            SourceFileBehavior = new SourceFileBehaviorConfig
            {
                AfterSuccessfulArchive = SourceFileAction.Move,
                MoveToDirectory = string.Empty
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Job 'TestJob': Move target directory is required when source file action is Move.", errors);
        Assert.Contains("Job 'TestJob': Move target directory is missing.", errors);
    }

    #endregion

    #region Schedule Validation Failures

    [Fact]
    public void Validate_EnabledScheduleWithoutCronExpression_ReturnsError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            },
            Schedules = new List<ScheduleConfig>
            {
                new() { Enabled = true, CronExpression = string.Empty }
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Job 'TestJob': Enabled schedule has no cron expression.", errors);
    }

    [Fact]
    public void Validate_EnabledScheduleWithWhitespaceCronExpression_ReturnsError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            },
            Schedules = new List<ScheduleConfig>
            {
                new() { Enabled = true, CronExpression = "   " }
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Job 'TestJob': Enabled schedule has no cron expression.", errors);
    }

    #endregion

    #region Path Validation Failures

    [Fact]
    public void Validate_InvalidSourcePath_ReturnsError()
    {
        // Arrange - Using a path with null characters which is always invalid
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = "C:\\Test\0Invalid" },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Job 'TestJob': Source path is not a valid path.", errors);
    }

    [Fact]
    public void Validate_InvalidArchiveTargetDirectory_ReturnsError()
    {
        // Arrange - Using a path with null characters which is always invalid
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = "C:\\Archive\0Invalid",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Job 'TestJob': Archive target directory is not a valid path.", errors);
    }

    [Fact]
    public void Validate_InvalidMoveToDirectory_ReturnsError()
    {
        // Arrange - Using a path with null characters which is always invalid
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            },
            SourceFileBehavior = new SourceFileBehaviorConfig
            {
                AfterSuccessfulArchive = SourceFileAction.Move,
                MoveToDirectory = "C:\\Move\0Invalid"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains("Job 'TestJob': Move target directory is not a valid path.", errors);
    }

    #endregion

    #region Path Existence Validation

    [Fact]
    public void Validate_NonExistentSourcePathWithCheckEnabled_ReturnsError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\ThisPathDoesNotExist12345" },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: true);

        // Assert
        Assert.Contains("Job 'TestJob': Source path does not exist.", errors);
    }

    [Fact]
    public void Validate_NonExistentSourcePathWithCheckDisabled_ReturnsNoError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\ThisPathDoesNotExist12345" },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.DoesNotContain(errors, e => e.Contains("Source path does not exist"));
    }

    [Fact]
    public void Validate_NonExistentMoveDirectoryWithCheckEnabled_ReturnsError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            },
            SourceFileBehavior = new SourceFileBehaviorConfig
            {
                AfterSuccessfulArchive = SourceFileAction.Move,
                MoveToDirectory = @"C:\ThisPathDoesNotExist12345"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: true);

        // Assert
        Assert.Contains("Job 'TestJob': Move target directory does not exist.", errors);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Validate_MinimumAndMaximumAgeEqual_ReturnsNoError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Selection = new FileSelectionConfig { MinimumAgeDays = 10, MaximumAgeDays = 10 },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_MinimumAgeZero_ReturnsNoError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Selection = new FileSelectionConfig { MinimumAgeDays = 0 },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_MaximumAgeZero_ReturnsNoError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Selection = new FileSelectionConfig { MaximumAgeDays = 0 },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_UnnamedJobShowsPlaceholderInErrors()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = string.Empty,
            Enabled = true,
            Source = new SourceConfig { Path = string.Empty },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Contains(errors, e => e.Contains("Job '<unnamed>': Source path is missing."));
    }

    [Fact]
    public void Validate_MultipleValidationErrors_ReturnsAllErrors()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = string.Empty,
            Enabled = true,
            Source = new SourceConfig { Path = string.Empty },
            Selection = new FileSelectionConfig
            {
                MinimumAgeDays = -5,
                MaximumAgeDays = -10,
                Extensions = new List<string> { "txt", string.Empty }
            },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = string.Empty,
                FileNamePattern = string.Empty
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.True(errors.Count >= 8, $"Expected at least 8 errors, but got {errors.Count}");
        Assert.Contains("Job config: Job name is missing.", errors);
        Assert.Contains(errors, e => e.Contains("Source path is missing"));
        Assert.Contains(errors, e => e.Contains("Archive file name pattern is missing"));
        Assert.Contains(errors, e => e.Contains("Archive target directory is missing"));
        Assert.Contains(errors, e => e.Contains("Minimum age days cannot be negative"));
        Assert.Contains(errors, e => e.Contains("Maximum age days cannot be negative"));
        Assert.Contains(errors, e => e.Contains("File extension 'txt' must start with '.'"));
        Assert.Contains(errors, e => e.Contains("File extension filter contains an empty entry"));
    }

    [Fact]
    public void Validate_DisabledScheduleWithoutCronExpression_ReturnsNoError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            },
            Schedules = new List<ScheduleConfig>
            {
                new() { Enabled = false, CronExpression = string.Empty }
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_KeepActionWithMoveDirectory_ReturnsNoError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            },
            SourceFileBehavior = new SourceFileBehaviorConfig
            {
                AfterSuccessfulArchive = SourceFileAction.Keep,
                MoveToDirectory = @"C:\Some\Path" // Should be ignored when action is Keep
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_DeleteActionWithMoveDirectory_ReturnsNoError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source" },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}"
            },
            SourceFileBehavior = new SourceFileBehaviorConfig
            {
                AfterSuccessfulArchive = SourceFileAction.Delete,
                MoveToDirectory = @"C:\Some\Path" // Should be ignored when action is Delete
            }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_EmptyDefaultJobConfigDirectory_ReturnsNoError()
    {
        // Arrange
        var config = new ArchiveRuntimeConfig
        {
            Root = new RootConfig
            {
                Name = "Test Archiver",
                JobConfigDirectories = new List<string> { @"C:\Config\Jobs" },
                DefaultJobConfigDirectory = null
            },
            Jobs = new List<ArchiveJobConfig> { CreateValidJobConfig("TestJob") }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_WhitespaceDefaultJobConfigDirectory_ReturnsNoError()
    {
        // Arrange
        var config = new ArchiveRuntimeConfig
        {
            Root = new RootConfig
            {
                Name = "Test Archiver",
                JobConfigDirectories = new List<string> { @"C:\Config\Jobs" },
                DefaultJobConfigDirectory = "   "
            },
            Jobs = new List<ArchiveJobConfig> { CreateValidJobConfig("TestJob") }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_EmptySelectionFiltersLists_ReturnsNoError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source", Recursive = true },
            Selection = new FileSelectionConfig
            {
                Extensions = new List<string>(),
                StartsWith = new List<string>(),
                EndsWith = new List<string>(),
                RegexPatterns = new List<string>()
            },
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}",
                Format = ArchiveFormat.Zip
            },
            SourceFileBehavior = new SourceFileBehaviorConfig { AfterSuccessfulArchive = SourceFileAction.Keep },
            Schedules = new List<ScheduleConfig>(),
            Execution = new ExecutionConfig { DryRun = true }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_NoSchedules_ReturnsNoError()
    {
        // Arrange
        var job = new ArchiveJobConfig
        {
            Name = "TestJob",
            Enabled = true,
            Source = new SourceConfig { Path = @"C:\Source", Recursive = true },
            Selection = new FileSelectionConfig(),
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}",
                Format = ArchiveFormat.Zip
            },
            SourceFileBehavior = new SourceFileBehaviorConfig { AfterSuccessfulArchive = SourceFileAction.Keep },
            Schedules = new List<ScheduleConfig>(),
            Execution = new ExecutionConfig { DryRun = true }
        };

        var config = new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { job }
        };

        // Act
        var errors = ArchiveConfigValidator.Validate(config, checkPathExists: false);

        // Assert
        Assert.Empty(errors);
    }

    #endregion

    #region Helper Methods

    private static ArchiveRuntimeConfig CreateValidMinimalConfig()
    {
        return new ArchiveRuntimeConfig
        {
            Root = CreateValidRootConfig(),
            Jobs = new List<ArchiveJobConfig> { CreateValidJobConfig("TestJob") }
        };
    }

    private static RootConfig CreateValidRootConfig()
    {
        return new RootConfig
        {
            Name = "Test Archiver",
            JobConfigDirectories = new List<string> { @"C:\Config\Jobs" }
        };
    }

    private static ArchiveJobConfig CreateValidJobConfig(string name)
    {
        return new ArchiveJobConfig
        {
            Name = name,
            Enabled = true,
            Source = new SourceConfig
            {
                Path = @"C:\Source",
                Recursive = true
            },
            Selection = new FileSelectionConfig(),
            Archive = new ArchiveTargetConfig
            {
                TargetDirectory = @"C:\Archive",
                FileNamePattern = "{jobName}_{yyyyMMdd_HHmmss}",
                Format = ArchiveFormat.Zip
            },
            SourceFileBehavior = new SourceFileBehaviorConfig
            {
                AfterSuccessfulArchive = SourceFileAction.Keep
            },
            Schedules = new List<ScheduleConfig>(),
            Execution = new ExecutionConfig { DryRun = true }
        };
    }

    #endregion
}
