using MTSM.Archiver.Core.Abstractions.Models;

namespace MTSM.Archiver.Core.Abstractions.Interfaces
{
    public interface IArchiveVerifier
    {
        /// <summary>
        /// Verifies the integrity and validity of a generated archive artifact.
        /// </summary>
        Task<ArchiveVerificationResult> VerifyAsync(
            ArchiveWriteResult result,
            CancellationToken cancellationToken = default);
    }
}
