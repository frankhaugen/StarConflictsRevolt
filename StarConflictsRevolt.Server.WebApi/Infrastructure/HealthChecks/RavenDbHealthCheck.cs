using Microsoft.Extensions.Diagnostics.HealthChecks;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;

namespace StarConflictsRevolt.Server.WebApi.Infrastructure.HealthChecks;

/// <summary>
/// Health check for RavenDB event store. Used by Aspire dashboard and readiness probes.
/// </summary>
public sealed class RavenDbHealthCheck(IDocumentStore store) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await store.Maintenance.SendAsync(new GetStatisticsOperation(), cancellationToken);
            return HealthCheckResult.Healthy("RavenDB is reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("RavenDB check failed.", ex, new Dictionary<string, object?>
            {
                ["exception"] = ex.Message
            }!);
        }
    }
}
