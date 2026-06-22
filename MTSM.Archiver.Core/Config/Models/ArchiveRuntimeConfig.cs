namespace MTSM.Archiver.Core.Config.Models
{
    public sealed class ArchiveRuntimeConfig
    {
        public required RootConfig Root { get; init; }
        public required IReadOnlyList<ArchiveJobConfig> Jobs { get; init; }
    }
}
