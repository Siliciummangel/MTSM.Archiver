using MTSM.Archiver.Core.Config.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MTSM.Archiver.Core.Config
{
    public sealed class ArchiveConfigLoader
    {
        private readonly IDeserializer _deserializer;

        public ArchiveConfigLoader()
        {
            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
        }

        public ArchiveRuntimeConfig Load(string rootConfigPath)
        {
            if (string.IsNullOrWhiteSpace(rootConfigPath))
                throw new ArgumentException("Root config path cannot be null or empty.", nameof(rootConfigPath));

            if (!File.Exists(rootConfigPath))
                throw new FileNotFoundException($"Root config file not found: {rootConfigPath}");

            var rootConfig = LoadYamlFile<RootConfig>(rootConfigPath);

            if (rootConfig.JobConfigDirectories is null)
            {
                throw new ConfigLoadException("Root config property 'jobConfigDirectories' cannot be null.");
            }

            var rootDirectory = Path.GetDirectoryName(Path.GetFullPath(rootConfigPath))
                                ?? Directory.GetCurrentDirectory();

            var jobConfigs = new List<ArchiveJobFile>();

            foreach (var directory in rootConfig.JobConfigDirectories)
            {
                var resolvedDirectory = ResolvePath(rootDirectory, directory);

                if (!Directory.Exists(resolvedDirectory))
                    throw new ConfigLoadException($"Job config directory not found: {resolvedDirectory}");

                var files = Directory.GetFiles(resolvedDirectory, "*.yaml", SearchOption.TopDirectoryOnly)
                    .Concat(Directory.GetFiles(resolvedDirectory, "*.yml", SearchOption.TopDirectoryOnly))
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                foreach (var file in files)
                {
                    var jobConfig = LoadYamlFile<ArchiveJobFile>(file);
                    jobConfigs.Add(jobConfig);
                }
            }

            return new ArchiveRuntimeConfig
            {
                Root = rootConfig,
                Jobs = jobConfigs
            };
        }

        private T LoadYamlFile<T>(string path)
        {
            try
            {
                var yaml = File.ReadAllText(path);
                var result = _deserializer.Deserialize<T>(yaml);

                return result ?? throw new ConfigLoadException($@"Config file is empty or invalid: {path})");
            }
            catch (ConfigLoadException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ConfigLoadException($"Failed to load config file: {path}", ex);
            }
        }

        private static string ResolvePath(string rootDirectory, string path)
        {
            if (Path.IsPathRooted(path))
                return Path.GetFullPath(path);

            return Path.GetFullPath(Path.Combine(rootDirectory, path));
        }
    }
}
