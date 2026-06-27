using MTSM.Archiver.Core.Abstractions.Interfaces;
using MTSM.Archiver.Core.Abstractions.Models;
using MTSM.Archiver.Core.Config.Models;
using MTSM.Archiver.Core.Providers.Naming;

namespace MTSM.Archiver.Core.Providers.FileSystem
{
    public sealed class FileSystemArchiveDestination : IArchiveDestination
    {
        private const int BufferSize = 1024 * 1024;

        private readonly IArchiveFileNameResolver _fileNameResolver;

        public FileSystemArchiveDestination()
            : this(new DefaultArchiveFileNameResolver())
        {
        }

        public FileSystemArchiveDestination(
            IArchiveFileNameResolver fileNameResolver)
        {
            _fileNameResolver = fileNameResolver
                ?? throw new ArgumentNullException(nameof(fileNameResolver));
        }

        public ValueTask<Stream> OpenWriteAsync(
            ArchiveDestinationContext context,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            cancellationToken.ThrowIfCancellationRequested();

            var target = context.ArchiveTarget;

            if (string.IsNullOrWhiteSpace(target.TargetDirectory))
                throw new InvalidOperationException("Archive target directory is not configured.");

            var targetDirectory = Path.GetFullPath(target.TargetDirectory);

            Directory.CreateDirectory(targetDirectory);

            var archiveFileName = _fileNameResolver.ResolveFileName(context);

            var fullPath = ResolveDestinationPath(
                targetDirectory,
                archiveFileName,
                target.ExistingArchiveBehavior);

            var stream = new FileStream(
                fullPath,
                ResolveFileMode(target.ExistingArchiveBehavior),
                FileAccess.Write,
                FileShare.None,
                bufferSize: BufferSize,
                options: FileOptions.Asynchronous);

            return ValueTask.FromResult<Stream>(stream);
        }

        private static string ResolveDestinationPath(
            string targetDirectory,
            string archiveFileName,
            ExistingArchiveBehavior behavior)
        {
            var fullPath = Path.Combine(targetDirectory, archiveFileName);

            switch (behavior)
            {
                case ExistingArchiveBehavior.Fail:
                case ExistingArchiveBehavior.Overwrite:
                    return fullPath;

                case ExistingArchiveBehavior.CreateUniqueName:

                    if (!File.Exists(fullPath))
                        return fullPath;

                    var fileNameWithoutExtension =
                        GetFileNameWithoutFullExtension(archiveFileName);

                    var extension = GetFullExtension(archiveFileName);

                    var counter = 1;

                    while (true)
                    {
                        var candidateFileName =
                            $"{fileNameWithoutExtension}_{counter}{extension}";

                        var candidatePath =
                            Path.Combine(targetDirectory, candidateFileName);

                        if (!File.Exists(candidatePath))
                            return candidatePath;

                        counter++;
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(behavior));
            }
        }

        private static string GetFullExtension(string fileName)
        {
            return fileName.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase)
                ? ".tar.gz"
                : Path.GetExtension(fileName);
        }

        private static string GetFileNameWithoutFullExtension(string fileName)
        {
            return fileName.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase)
                ? fileName[..^".tar.gz".Length]
                : Path.GetFileNameWithoutExtension(fileName);
        }

        private static FileMode ResolveFileMode(
            ExistingArchiveBehavior behavior)
        {
            return behavior switch
            {
                ExistingArchiveBehavior.Fail => FileMode.CreateNew,
                ExistingArchiveBehavior.Overwrite => FileMode.Create,
                ExistingArchiveBehavior.CreateUniqueName => FileMode.CreateNew,
                _ => throw new ArgumentOutOfRangeException(nameof(behavior))
            };
        }
    }
}