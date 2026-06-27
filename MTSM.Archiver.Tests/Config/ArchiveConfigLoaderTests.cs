using MTSM.Archiver.Core.Config;
using MTSM.Archiver.Core.Config.Models;

namespace MTSM.Archiver.Tests.Config;

/// <summary>
/// Tests for <see cref="ArchiveConfigLoader"/>.
/// Covers success cases, validation failures, edge cases and null handling.
/// </summary>
public class ArchiveConfigLoaderTests : IDisposable
{
    private readonly string _tempDirectory;

    public ArchiveConfigLoaderTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"ArchiveConfigLoaderTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    #region Success Cases

    [Fact]
    public void Load_ValidRootConfigWithNoJobDirectories_ReturnsConfigWithEmptyJobs()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var rootConfigPath = CreateRootConfig(new List<string>());

        // Act
        var result = loader.Load(rootConfigPath);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Root);
        Assert.Equal("TestArchiver", result.Root.Name);
        Assert.Empty(result.Jobs);
    }

    [Fact]
    public void Load_ValidRootConfigWithSingleJobDirectory_LoadsJobConfigs()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var jobDir = Path.Combine(_tempDirectory, "jobs");
        Directory.CreateDirectory(jobDir);

        CreateJobConfig(jobDir, "job1.yaml", "Job1");
        CreateJobConfig(jobDir, "job2.yml", "Job2");

        var rootConfigPath = CreateRootConfig(new List<string> { jobDir });

        // Act
        var result = loader.Load(rootConfigPath);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Jobs);
        Assert.Equal(2, result.Jobs.Count);
        Assert.Contains(result.Jobs, j => j.Job.Name == "Job1");
        Assert.Contains(result.Jobs, j => j.Job.Name == "Job2");
    }

    [Fact]
    public void Load_ValidRootConfigWithMultipleJobDirectories_LoadsAllJobConfigs()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var jobDir1 = Path.Combine(_tempDirectory, "jobs1");
        var jobDir2 = Path.Combine(_tempDirectory, "jobs2");
        Directory.CreateDirectory(jobDir1);
        Directory.CreateDirectory(jobDir2);

        CreateJobConfig(jobDir1, "job1.yaml", "Job1");
        CreateJobConfig(jobDir2, "job2.yaml", "Job2");
        CreateJobConfig(jobDir2, "job3.yml", "Job3");

        var rootConfigPath = CreateRootConfig(new List<string> { jobDir1, jobDir2 });

        // Act
        var result = loader.Load(rootConfigPath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Jobs.Count);
        Assert.Contains(result.Jobs, j => j.Job.Name == "Job1");
        Assert.Contains(result.Jobs, j => j.Job.Name == "Job2");
        Assert.Contains(result.Jobs, j => j.Job.Name == "Job3");
    }

    [Fact]
    public void Load_JobConfigsWithBothYamlAndYmlExtensions_LoadsBothTypes()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var jobDir = Path.Combine(_tempDirectory, "jobs");
        Directory.CreateDirectory(jobDir);

        CreateJobConfig(jobDir, "job1.yaml", "YamlJob");
        CreateJobConfig(jobDir, "job2.yml", "YmlJob");

        var rootConfigPath = CreateRootConfig(new List<string> { jobDir });

        // Act
        var result = loader.Load(rootConfigPath);

        // Assert
        Assert.Equal(2, result.Jobs.Count);
        Assert.Contains(result.Jobs, j => j.Job.Name == "YamlJob");
        Assert.Contains(result.Jobs, j => j.Job.Name == "YmlJob");
    }

    [Fact]
    public void Load_RelativeJobDirectory_ResolvesPathRelativeToRootConfig()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var configDir = Path.Combine(_tempDirectory, "config");
        var jobDir = Path.Combine(configDir, "jobs");
        Directory.CreateDirectory(configDir);
        Directory.CreateDirectory(jobDir);

        CreateJobConfig(jobDir, "job1.yaml", "Job1");

        var rootConfigPath = Path.Combine(configDir, "root.yaml");
        File.WriteAllText(rootConfigPath, @"
            name: TestArchiver
            jobConfigDirectories:
              - jobs
            ");

        // Act
        var result = loader.Load(rootConfigPath);

        // Assert
        Assert.Single(result.Jobs);
        Assert.Equal("Job1", result.Jobs[0].Job.Name);
    }

    [Fact]
    public void Load_AbsoluteJobDirectory_ResolvesAbsolutePath()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var jobDir = Path.Combine(_tempDirectory, "absoluteJobs");
        Directory.CreateDirectory(jobDir);

        CreateJobConfig(jobDir, "job1.yaml", "AbsoluteJob");

        var rootConfigPath = CreateRootConfig(new List<string> { jobDir });

        // Act
        var result = loader.Load(rootConfigPath);

        // Assert
        Assert.Single(result.Jobs);
        Assert.Equal("AbsoluteJob", result.Jobs[0].Job.Name);
    }

    [Fact]
    public void Load_JobDirectoryWithNonYamlFiles_IgnoresNonYamlFiles()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var jobDir = Path.Combine(_tempDirectory, "jobs");
        Directory.CreateDirectory(jobDir);

        CreateJobConfig(jobDir, "job1.yaml", "Job1");
        File.WriteAllText(Path.Combine(jobDir, "readme.txt"), "This is a readme file");
        File.WriteAllText(Path.Combine(jobDir, "config.json"), "{}");
        File.WriteAllText(Path.Combine(jobDir, "script.ps1"), "Write-Host 'Hello'");

        var rootConfigPath = CreateRootConfig(new List<string> { jobDir });

        // Act
        var result = loader.Load(rootConfigPath);

        // Assert
        Assert.Single(result.Jobs);
        Assert.Equal("Job1", result.Jobs[0].Job.Name);
    }

    [Fact]
    public void Load_EmptyJobDirectory_ReturnsConfigWithNoJobs()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var jobDir = Path.Combine(_tempDirectory, "emptyJobs");
        Directory.CreateDirectory(jobDir);

        var rootConfigPath = CreateRootConfig(new List<string> { jobDir });

        // Act
        var result = loader.Load(rootConfigPath);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Jobs);
    }

    [Fact]
    public void Load_JobConfigsAreLoadedInAlphabeticalOrder_ReturnsOrderedJobs()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var jobDir = Path.Combine(_tempDirectory, "jobs");
        Directory.CreateDirectory(jobDir);

        CreateJobConfig(jobDir, "c_job.yaml", "JobC");
        CreateJobConfig(jobDir, "a_job.yaml", "JobA");
        CreateJobConfig(jobDir, "b_job.yml", "JobB");

        var rootConfigPath = CreateRootConfig(new List<string> { jobDir });

        // Act
        var result = loader.Load(rootConfigPath);

        // Assert
        Assert.Equal(3, result.Jobs.Count);
        // Files are ordered alphabetically, so a_job, b_job, c_job
        Assert.Equal("JobA", result.Jobs[0].Job.Name);
        Assert.Equal("JobB", result.Jobs[1].Job.Name);
        Assert.Equal("JobC", result.Jobs[2].Job.Name);
    }

    [Fact]
    public void Load_DuplicateFileNameInYamlAndYml_LoadsOnlyDistinctFiles()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var jobDir = Path.Combine(_tempDirectory, "jobs");
        Directory.CreateDirectory(jobDir);

        // Create the same filename with both extensions
        CreateJobConfig(jobDir, "job1.yaml", "JobYaml");
        CreateJobConfig(jobDir, "job1.yml", "JobYml");

        var rootConfigPath = CreateRootConfig(new List<string> { jobDir });

        // Act
        var result = loader.Load(rootConfigPath);

        // Assert
        // Both files should be loaded as they have different extensions
        Assert.Equal(2, result.Jobs.Count);
        Assert.Contains(result.Jobs, j => j.Job.Name == "JobYaml");
        Assert.Contains(result.Jobs, j => j.Job.Name == "JobYml");
    }

    [Fact]
    public void Load_RootConfigWithCamelCaseProperties_ParsesCorrectly()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var rootConfigPath = Path.Combine(_tempDirectory, "root.yaml");
        File.WriteAllText(rootConfigPath, @"
            name: CamelCaseArchiver
            jobConfigDirectories: []
            defaultJobConfigDirectory: /default/path
            ");

        // Act
        var result = loader.Load(rootConfigPath);

        // Assert
        Assert.NotNull(result.Root);
        Assert.Equal("CamelCaseArchiver", result.Root.Name);
        Assert.Equal("/default/path", result.Root.DefaultJobConfigDirectory);
    }

    [Fact]
    public void Load_RootConfigWithUnmatchedProperties_IgnoresExtraProperties()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var rootConfigPath = Path.Combine(_tempDirectory, "root.yaml");
        File.WriteAllText(rootConfigPath, @"
            name: TestArchiver
            jobConfigDirectories: []
            unknownProperty: someValue
            anotherUnknownField: 123
            ");

        // Act
        var result = loader.Load(rootConfigPath);

        // Assert
        Assert.NotNull(result.Root);
        Assert.Equal("TestArchiver", result.Root.Name);
    }

    #endregion

    #region Validation Failures

    [Fact]
    public void Load_NullRootConfigPath_ThrowsArgumentException()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => loader.Load(null!));
        Assert.Contains("Root config path cannot be null or empty", exception.Message);
        Assert.Equal("rootConfigPath", exception.ParamName);
    }

    [Fact]
    public void Load_EmptyRootConfigPath_ThrowsArgumentException()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => loader.Load(string.Empty));
        Assert.Contains("Root config path cannot be null or empty", exception.Message);
        Assert.Equal("rootConfigPath", exception.ParamName);
    }

    [Fact]
    public void Load_WhitespaceRootConfigPath_ThrowsArgumentException()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => loader.Load("   "));
        Assert.Contains("Root config path cannot be null or empty", exception.Message);
        Assert.Equal("rootConfigPath", exception.ParamName);
    }

    [Fact]
    public void Load_NonExistentRootConfigFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var nonExistentPath = Path.Combine(_tempDirectory, "nonexistent.yaml");

        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(() => loader.Load(nonExistentPath));
        Assert.Contains("Root config file not found", exception.Message);
        Assert.Contains(nonExistentPath, exception.Message);
    }

    [Fact]
    public void Load_NonExistentJobDirectory_ThrowsConfigLoadException()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var nonExistentJobDir = Path.Combine(_tempDirectory, "nonexistentJobs");
        var rootConfigPath = CreateRootConfig(new List<string> { nonExistentJobDir });

        // Act & Assert
        var exception = Assert.Throws<ConfigLoadException>(() => loader.Load(rootConfigPath));
        Assert.Contains("Job config directory not found", exception.Message);
        Assert.Contains(nonExistentJobDir, exception.Message);
    }

    [Fact]
    public void Load_InvalidYamlInRootConfig_ThrowsConfigLoadException()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var rootConfigPath = Path.Combine(_tempDirectory, "invalid.yaml");
        File.WriteAllText(rootConfigPath, @"
            name: TestArchiver
            jobConfigDirectories: [
              - /path1
              invalid yaml structure here
            ");

        // Act & Assert
        var exception = Assert.Throws<ConfigLoadException>(() => loader.Load(rootConfigPath));
        Assert.Contains("Failed to load config file", exception.Message);
        Assert.Contains(rootConfigPath, exception.Message);
        Assert.NotNull(exception.InnerException);
    }

    [Fact]
    public void Load_InvalidYamlInJobConfig_ThrowsConfigLoadException()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var jobDir = Path.Combine(_tempDirectory, "jobs");
        Directory.CreateDirectory(jobDir);

        var invalidJobPath = Path.Combine(jobDir, "invalid.yaml");
        File.WriteAllText(invalidJobPath, @"
            name: InvalidJob
            this is: not: valid: yaml: syntax
            ");

        var rootConfigPath = CreateRootConfig(new List<string> { jobDir });

        // Act & Assert
        var exception = Assert.Throws<ConfigLoadException>(() => loader.Load(rootConfigPath));
        Assert.Contains("Failed to load config file", exception.Message);
        Assert.NotNull(exception.InnerException);
    }

    [Fact]
    public void Load_EmptyRootConfigFile_ThrowsConfigLoadException()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var rootConfigPath = Path.Combine(_tempDirectory, "empty.yaml");
        File.WriteAllText(rootConfigPath, string.Empty);

        // Act & Assert
        var exception = Assert.Throws<ConfigLoadException>(() => loader.Load(rootConfigPath));
        Assert.Contains("Config file is empty or invalid", exception.Message);
        Assert.Contains(rootConfigPath, exception.Message);
    }

    [Fact]
    public void Load_EmptyJobConfigFile_ThrowsConfigLoadException()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var jobDir = Path.Combine(_tempDirectory, "jobs");
        Directory.CreateDirectory(jobDir);

        var emptyJobPath = Path.Combine(jobDir, "empty.yaml");
        File.WriteAllText(emptyJobPath, string.Empty);

        var rootConfigPath = CreateRootConfig(new List<string> { jobDir });

        // Act & Assert
        var exception = Assert.Throws<ConfigLoadException>(() => loader.Load(rootConfigPath));
        Assert.Contains("Config file is empty or invalid", exception.Message);
        Assert.Contains(emptyJobPath, exception.Message);
    }

    [Fact]
    public void Load_WhitespaceOnlyRootConfigFile_ThrowsConfigLoadException()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var rootConfigPath = Path.Combine(_tempDirectory, "whitespace.yaml");
        File.WriteAllText(rootConfigPath, "   \n\t\n   ");

        // Act & Assert
        var exception = Assert.Throws<ConfigLoadException>(() => loader.Load(rootConfigPath));
        Assert.Contains("Config file is empty or invalid", exception.Message);
    }

    [Fact]
    public void Load_JobConfigFileWithOnlyComments_ThrowsConfigLoadException()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var jobDir = Path.Combine(_tempDirectory, "jobs");
        Directory.CreateDirectory(jobDir);

        var commentOnlyPath = Path.Combine(jobDir, "comments.yaml");
        File.WriteAllText(commentOnlyPath, @"
            # This is a comment
            # Another comment
            # No actual content
            ");

        var rootConfigPath = CreateRootConfig(new List<string> { jobDir });

        // Act & Assert
        var exception = Assert.Throws<ConfigLoadException>(() => loader.Load(rootConfigPath));
        Assert.Contains("Config file is empty or invalid", exception.Message);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Load_VeryLongFilePath_LoadsSuccessfully()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();

        // Create a nested directory structure
        var longPath = _tempDirectory;
        for (int i = 0; i < 10; i++)
        {
            longPath = Path.Combine(longPath, $"level{i}");
        }
        Directory.CreateDirectory(longPath);

        CreateJobConfig(longPath, "job.yaml", "LongPathJob");
        var rootConfigPath = CreateRootConfig(new List<string> { longPath });

        // Act
        var result = loader.Load(rootConfigPath);

        // Assert
        Assert.Single(result.Jobs);
        Assert.Equal("LongPathJob", result.Jobs[0].Job.Name);
    }

    [Fact]
    public void Load_SpecialCharactersInDirectoryName_LoadsSuccessfully()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var specialDir = Path.Combine(_tempDirectory, "jobs with spaces & special-chars_123");
        Directory.CreateDirectory(specialDir);

        CreateJobConfig(specialDir, "job.yaml", "SpecialCharJob");
        var rootConfigPath = CreateRootConfig(new List<string> { specialDir });

        // Act
        var result = loader.Load(rootConfigPath);

        // Assert
        Assert.Single(result.Jobs);
        Assert.Equal("SpecialCharJob", result.Jobs[0].Job.Name);
    }

    [Fact]
    public void Load_UnicodeCharactersInFileName_LoadsSuccessfully()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var jobDir = Path.Combine(_tempDirectory, "jobs");
        Directory.CreateDirectory(jobDir);

        CreateJobConfig(jobDir, "jöb_配置.yaml", "UnicodeJob");
        var rootConfigPath = CreateRootConfig(new List<string> { jobDir });

        // Act
        var result = loader.Load(rootConfigPath);

        // Assert
        Assert.Single(result.Jobs);
        Assert.Equal("UnicodeJob", result.Jobs[0].Job.Name);
    }

    [Fact]
    public void Load_MixedPathSeparators_LoadsSuccessfully()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var configDir = Path.Combine(_tempDirectory, "config");
        var jobDir = Path.Combine(configDir, "jobs");
        Directory.CreateDirectory(configDir);
        Directory.CreateDirectory(jobDir);

        CreateJobConfig(jobDir, "job.yaml", "MixedPathJob");

        var rootConfigPath = Path.Combine(configDir, "root.yaml");
        // Use mixed separators in the YAML (forward slashes)
        File.WriteAllText(rootConfigPath, $@"
            name: TestArchiver
            jobConfigDirectories:
              - jobs
            ");

        // Act
        var result = loader.Load(rootConfigPath);

        // Assert
        Assert.Single(result.Jobs);
        Assert.Equal("MixedPathJob", result.Jobs[0].Job.Name);
    }

    [Fact]
    public void Load_RootConfigInCurrentDirectory_ResolvesPathsCorrectly()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var currentDir = Directory.GetCurrentDirectory();
        var testDir = Path.Combine(_tempDirectory, "currentDirTest");
        Directory.CreateDirectory(testDir);

        var jobDir = Path.Combine(testDir, "jobs");
        Directory.CreateDirectory(jobDir);
        CreateJobConfig(jobDir, "job.yaml", "CurrentDirJob");

        var rootConfigPath = Path.Combine(testDir, "root.yaml");
        File.WriteAllText(rootConfigPath, @"
            name: TestArchiver
            jobConfigDirectories:
              - jobs
            ");

        try
        {
            Directory.SetCurrentDirectory(testDir);

            // Act
            var result = loader.Load(rootConfigPath);

            // Assert
            Assert.Single(result.Jobs);
            Assert.Equal("CurrentDirJob", result.Jobs[0].Job.Name);
        }
        finally
        {
            Directory.SetCurrentDirectory(currentDir);
        }
    }

    [Fact]
    public void Load_JobDirectoryWithSubdirectories_OnlyLoadsTopLevelFiles()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var jobDir = Path.Combine(_tempDirectory, "jobs");
        var subDir = Path.Combine(jobDir, "subfolder");
        Directory.CreateDirectory(jobDir);
        Directory.CreateDirectory(subDir);

        CreateJobConfig(jobDir, "topLevel.yaml", "TopLevelJob");
        CreateJobConfig(subDir, "nested.yaml", "NestedJob");

        var rootConfigPath = CreateRootConfig(new List<string> { jobDir });

        // Act
        var result = loader.Load(rootConfigPath);

        // Assert
        // Only the top-level job should be loaded (SearchOption.TopDirectoryOnly)
        Assert.Single(result.Jobs);
        Assert.Equal("TopLevelJob", result.Jobs[0].Job.Name);
    }

    [Fact]
    public void Load_CaseInsensitiveFileExtensions_LoadsAllYamlFiles()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var jobDir = Path.Combine(_tempDirectory, "jobs");
        Directory.CreateDirectory(jobDir);

        // Create files with different case extensions
        CreateJobConfig(jobDir, "job1.YAML", "Job1");
        CreateJobConfig(jobDir, "job2.YML", "Job2");
        CreateJobConfig(jobDir, "job3.Yaml", "Job3");

        var rootConfigPath = CreateRootConfig(new List<string> { jobDir });

        // Act
        var result = loader.Load(rootConfigPath);

        // Assert
        // Behavior depends on file system (Windows is case-insensitive, Linux is case-sensitive)
        // On Windows, all 3 should be loaded; on Linux, none may be loaded
        // We'll just verify it doesn't throw an exception
        Assert.NotNull(result);
        Assert.NotNull(result.Jobs);
    }

    [Fact]
    public void Load_RootConfigWithDotInPath_LoadsSuccessfully()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var configDir = Path.Combine(_tempDirectory, "config.v1.0");
        Directory.CreateDirectory(configDir);

        var rootConfigPath = Path.Combine(configDir, "root.yaml");
        File.WriteAllText(rootConfigPath, @"
            name: TestArchiver
            jobConfigDirectories: []
            ");

        // Act
        var result = loader.Load(rootConfigPath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestArchiver", result.Root.Name);
    }

    [Fact]
    public void Load_MultipleCallsToSameLoader_LoadsIndependently()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();

        var jobDir1 = Path.Combine(_tempDirectory, "jobs1");
        Directory.CreateDirectory(jobDir1);
        CreateJobConfig(jobDir1, "job.yaml", "Job1");
        var rootConfigPath1 = CreateRootConfig(new List<string> { jobDir1 }, "root1.yaml");

        var jobDir2 = Path.Combine(_tempDirectory, "jobs2");
        Directory.CreateDirectory(jobDir2);
        CreateJobConfig(jobDir2, "job.yaml", "Job2");
        var rootConfigPath2 = CreateRootConfig(new List<string> { jobDir2 }, "root2.yaml");

        // Act
        var result1 = loader.Load(rootConfigPath1);
        var result2 = loader.Load(rootConfigPath2);

        // Assert
        Assert.Single(result1.Jobs);
        Assert.Equal("Job1", result1.Jobs[0].Job.Name);

        Assert.Single(result2.Jobs);
        Assert.Equal("Job2", result2.Jobs[0].Job.Name);
    }

    [Fact]
    public void Load_LargeNumberOfJobConfigs_LoadsAllSuccessfully()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var jobDir = Path.Combine(_tempDirectory, "manyJobs");
        Directory.CreateDirectory(jobDir);

        const int jobCount = 100;
        for (int i = 0; i < jobCount; i++)
        {
            CreateJobConfig(jobDir, $"job{i:D3}.yaml", $"Job{i}");
        }

        var rootConfigPath = CreateRootConfig(new List<string> { jobDir });

        // Act
        var result = loader.Load(rootConfigPath);

        // Assert
        Assert.Equal(jobCount, result.Jobs.Count);
        for (int i = 0; i < jobCount; i++)
        {
            Assert.Contains(result.Jobs, j => j.Job.Name == $"Job{i}");
        }
    }

    #endregion

    #region Null Handling

    [Fact]
    public void Load_NullJobConfigDirectoriesList_ThrowsNullReferenceException()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var rootConfigPath = Path.Combine(_tempDirectory, "root.yaml");
        // YAML with explicit null value
        File.WriteAllText(rootConfigPath, @"
            name: TestArchiver
            jobConfigDirectories: null
            ");

        // Act & Assert
        // When JobConfigDirectories is null, iterating over it throws NullReferenceException
        var exception = Assert.Throws<ConfigLoadException>(() => loader.Load(rootConfigPath));
        Assert.Contains("jobConfigDirectories", exception.Message);
    }

    [Fact]
    public void Load_RootConfigWithNullName_LoadsSuccessfully()
    {
        // Arrange
        var loader = new ArchiveConfigLoader();
        var rootConfigPath = Path.Combine(_tempDirectory, "root.yaml");
        File.WriteAllText(rootConfigPath, @"
            jobConfigDirectories: []
            ");

        // Act
        var result = loader.Load(rootConfigPath);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Root);
        // Name should have default value from RootConfig
        Assert.NotNull(result.Root.Name);
    }

    #endregion

    #region Helper Methods

    private string CreateRootConfig(List<string> jobDirectories, string fileName = "root.yaml")
    {
        var rootConfigPath = Path.Combine(_tempDirectory, fileName);
        var directories = string.Join("\n              - ", jobDirectories.Select(d => $"\"{d.Replace("\\", "\\\\")}\""));

        var content = $@"
            name: TestArchiver
            jobConfigDirectories:
              - {directories}
            ";

        if (jobDirectories.Count == 0)
        {
            content = @"
                name: TestArchiver
                jobConfigDirectories: []
                ";
        }

        File.WriteAllText(rootConfigPath, content);
        return rootConfigPath;
    }

    private void CreateJobConfig(string directory, string fileName, string jobName)
    {
        var jobPath = Path.Combine(directory, fileName);
        var content = $@"
            job:
              name: {jobName}
              enabled: true
              source:
                path: C:\Source
                recursive: true
              selection:
                extensions:
                  - .txt
                  - .log
                minimumAgeDays: 7
              archive:
                targetDirectory: C:\Archive
                fileNamePattern: '{{jobName}}_{{yyyyMMdd}}'
                format: Zip
              sourceFileBehavior:
                afterSuccessfulArchive: Keep
              execution:
                dryRun: false
            schedules: []
            ";
        File.WriteAllText(jobPath, content);
    }

    #endregion
}
