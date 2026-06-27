using MTSM.Archiver.Core.Abstractions.Interfaces;
using MTSM.Archiver.Core.Abstractions.Models;
using MTSM.Archiver.Core.Config.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTSM.Archiver.Core.Providers.Naming
{
    public sealed class DefaultArchiveFileNameResolver : IArchiveFileNameResolver
    {
        public string ResolveFileName(ArchiveDestinationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var target = context.ArchiveTarget;

            var pattern = string.IsNullOrWhiteSpace(target.FileNamePattern)
                ? "{runId}"
                : target.FileNamePattern;

            var fileName = pattern
                .Replace("{runId}", context.RunId.ToString("N"))
                .Replace("{yyyyMMdd}", context.StartedAt.ToString("yyyyMMdd"))
                .Replace("{yyyyMMdd_HHmmss}", context.StartedAt.ToString("yyyyMMdd_HHmmss"));

            return fileName + GetExtension(target.Format);
        }

        private static string GetExtension(ArchiveFormat format)
        {
            return format switch
            {
                ArchiveFormat.Zip => ".zip",
                ArchiveFormat.Tar => ".tar",
                ArchiveFormat.TarGz => ".tar.gz",
                _ => throw new NotSupportedException(
                    $"Archive format '{format}' is not supported.")
            };
        }
    }
}
