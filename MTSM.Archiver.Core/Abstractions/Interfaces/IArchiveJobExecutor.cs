using MTSM.Archiver.Core.Config.Models;
using MTSM.Archiver.Core.Execution.Models;

namespace MTSM.Archiver.Core.Abstractions.Interfaces
{
    /// <summary>
    /// Executes a configured archive job and coordinates the complete archive workflow.
    /// </summary>
    public interface IArchiveJobExecutor
    {
        Task<ArchiveExecutionResult> ExecuteAsync(
            ArchiveExecutionContext context,
            CancellationToken cancellationToken = default);
    }
}
