using MTSM.Archiver.Core.Writers.Models;
using MTSM.Archiver.Core.Verification.Models;

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
