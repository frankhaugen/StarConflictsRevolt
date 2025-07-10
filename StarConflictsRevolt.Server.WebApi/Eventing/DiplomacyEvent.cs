namespace StarConflictsRevolt.Server.WebApi.Eventing;

public record DiplomacyEvent(Guid PlayerId, Guid TargetPlayerId, string ProposalType, string? Message) : IGameEvent
{
    public void ApplyTo(Models.World world, Microsoft.Extensions.Logging.ILogger logger)
    {
        // For demonstration, just log or store the proposal type/message (no-op)
        // In a real implementation, update player relations, etc.
        logger.LogInformation("Diplomacy event processed for session");
    }
}