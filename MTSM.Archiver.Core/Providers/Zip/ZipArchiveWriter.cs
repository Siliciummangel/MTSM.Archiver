using MTSM.Archiver.Core.Abstractions.Interfaces;
using MTSM.Archiver.Core.Abstractions.Models;
using MTSM.Archiver.Core.Config.Models;
using System.IO.Compression;

namespace MTSM.Archiver.Core.Providers.Zip
{
    /// <summary>
    /// Creates ZIP archive artifacts from streamable archive items.
    /// </summary>
    public sealed class ZipArchiveWriter :IArchiveWriter
    {
        private const int BufferSize = 1024 * 1024;

        public async Task<ArchiveWriteResult> WriteAsync(
            ArchiveWriteContext context,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            cancellationToken.ThrowIfCancellationRequested();

            var writtenDirectories = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            using (var archive = new ZipArchive(
                context.OutputStream,
                ZipArchiveMode.Create,
                leaveOpen: true))
            {
                await foreach (var item in context.Items.WithCancellation(cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    switch (item.Kind)
                    {
                        case ArchiveItemKind.Directory:
                            WriteDirectoryEntry(
                                archive,
                                item,
                                writtenDirectories);
                            break;

                        case ArchiveItemKind.File:
                        case ArchiveItemKind.GeneratedContent:
                            await WriteContentEntryAsync(
                                GetCompressionLevel(context.Job.Archive.CompressionLevel),
                                archive,
                                item,
                                writtenDirectories,
                                cancellationToken);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(
                                nameof(item.Kind),
                                item.Kind,
                                "Unsupported archive item kind.");
                    }
                }
            }

            return new ArchiveWriteResult
            {
                Size = context.OutputStream.CanSeek
                    ? context.OutputStream.Length
                    : null
            };
        }

        private static async Task WriteContentEntryAsync(
            CompressionLevel compressionLevel,
            ZipArchive archive,
            ArchiveItem item,
            HashSet<string> writtenDirectories,
            CancellationToken cancellationToken)
        {
            var entryPath = NormalizeArchivePath(item.ArchivePath);

            if (string.IsNullOrWhiteSpace(entryPath))
                throw new InvalidOperationException("Archive entry path is missing.");

            WriteParentDirectoryEntries(
                archive,
                entryPath,
                writtenDirectories);

            var entry = archive.CreateEntry(
                entryPath,
                compressionLevel);
            
            if (item.LastWriteTime is not null)
            {
                entry.LastWriteTime = item.LastWriteTime.Value;
            }

            await using var sourceStream = await OpenSourceStreamAsync(
                item,
                cancellationToken);

            await using var entryStream = entry.Open();

            await sourceStream.CopyToAsync(
                entryStream,
                BufferSize,
                cancellationToken);
        }

        private static async ValueTask<Stream> OpenSourceStreamAsync(
            ArchiveItem item,
            CancellationToken cancellationToken)
        {
            if (item.OpenReadAsync is not null)
                return await item.OpenReadAsync(cancellationToken);

            if (item.Kind == ArchiveItemKind.File)
            {
                return new FileStream(
                    item.SourceIdentifier,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: BufferSize,
                    options: FileOptions.Asynchronous | FileOptions.SequentialScan);
            }

            throw new InvalidOperationException(
                $"Archive item '{item.ArchivePath}' does not provide a readable content stream.");
        }

        private static void WriteDirectoryEntry(
            ZipArchive archive,
            ArchiveItem item,
            HashSet<string> writtenDirectories)
        {
            var entryPath = NormalizeDirectoryPath(item.ArchivePath);

            if (string.IsNullOrWhiteSpace(entryPath))
                return;

            WriteDirectoryEntryIfMissing(
                archive,
                entryPath,
                writtenDirectories);
        }

        private static void WriteParentDirectoryEntries(
            ZipArchive archive,
            string fileEntryPath,
            HashSet<string> writtenDirectories)
        {
            var lastSlashIndex = fileEntryPath.LastIndexOf('/');

            if (lastSlashIndex < 0)
                return;

            var parentPath = fileEntryPath[..(lastSlashIndex + 1)];
            var parts = parentPath.Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries);

            var currentPath = string.Empty;

            foreach (var part in parts)
            {
                currentPath += part + "/";

                WriteDirectoryEntryIfMissing(
                    archive,
                    currentPath,
                    writtenDirectories);
            }
        }

        private static void WriteDirectoryEntryIfMissing(
            ZipArchive archive,
            string entryPath,
            HashSet<string> writtenDirectories)
        {
            if (!writtenDirectories.Add(entryPath))
                return;

            archive.CreateEntry(entryPath);
        }

        private static string NormalizeArchivePath(string path)
        {
            return path
                .Replace('\\', '/')
                .Trim()
                .TrimStart('/');
        }

        private static string NormalizeDirectoryPath(string path)
        {
            var normalizedPath = NormalizeArchivePath(path);

            if (string.IsNullOrWhiteSpace(normalizedPath))
                return string.Empty;

            return normalizedPath.EndsWith('/')
                ? normalizedPath
                : normalizedPath + '/';
        }

        private static CompressionLevel GetCompressionLevel(
            ArchiveCompressionLevel level)
        {
            return level switch
            {
                ArchiveCompressionLevel.None => CompressionLevel.NoCompression,
                ArchiveCompressionLevel.Fastest => CompressionLevel.Fastest,
                _ => CompressionLevel.Optimal
            };
        }
    }
}
