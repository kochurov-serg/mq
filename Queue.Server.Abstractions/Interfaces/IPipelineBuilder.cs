using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace Queue.Server.Abstractions.Interfaces;

/// <summary>
/// Virtual server
/// </summary>
public interface IPipelineBuilder
{
    /// <summary>
    /// start server
    /// </summary>
    /// <param name="applicationBuilder">Configuring pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    Task Build<T>(IApplicationBuilder applicationBuilder, CancellationToken cancellationToken) where T : IQueueServer;
}