using MTSM.Archiver.Core.Execution.Models;
using MTSM.Archiver.Core.PostActions.Models;

namespace MTSM.Archiver.Core.Abstractions.Interfaces
{
    /// <summary>
    /// Executes follow-up actions after a successful archive operation.
    /// </summary>
    public interface IPostArchiveAction
    {
        Task ExecuteAsync(
            PostArchiveActionContext context,
            CancellationToken cancellationToken = default);
    }
}
