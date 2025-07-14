using StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Events;

public record DiplomacyEvent(Guid PlayerId, Guid TargetPlayerId, string ProposalType, string? Message) : IGameEvent
{
    public void ApplyTo(World.World world, ILogger logger)
    {
        // Find the players
        var player = world.Players?.FirstOrDefault(p => p.PlayerId == PlayerId);
        var targetPlayer = world.Players?.FirstOrDefault(p => p.PlayerId == TargetPlayerId);

        if (player == null)
        {
            logger.LogWarning("Player {PlayerId} not found for DiplomacyEvent", PlayerId);
            return;
        }

        if (targetPlayer == null)
        {
            logger.LogWarning("Target player {TargetPlayerId} not found for DiplomacyEvent", TargetPlayerId);
            return;
        }

        // Process the diplomacy proposal
        switch (ProposalType.ToLowerInvariant())
        {
            case "alliance":
                ProcessAllianceProposal(player, targetPlayer, world, logger);
                break;
            case "peace":
                ProcessPeaceProposal(player, targetPlayer, world, logger);
                break;
            case "trade":
                ProcessTradeProposal(player, targetPlayer, world, logger);
                break;
            case "war":
                ProcessWarDeclaration(player, targetPlayer, world, logger);
                break;
            default:
                logger.LogWarning("Unknown diplomacy proposal type: {ProposalType}", ProposalType);
                break;
        }

        logger.LogInformation("Diplomacy event processed: {PlayerId} -> {TargetPlayerId} ({ProposalType})",
            PlayerId, TargetPlayerId, ProposalType);
    }

    private static void ProcessAllianceProposal(PlayerController player, PlayerController targetPlayer, World.World world, ILogger logger)
    {
        // For now, automatically accept alliance proposals
        // In a full implementation, this would require acceptance from the target player
        logger.LogInformation("Alliance formed between {PlayerId} and {TargetPlayerId}", player.PlayerId, targetPlayer.PlayerId);

        // Update player relations (in a full implementation, this would be stored in a relations table)
        // For now, just log the alliance
    }

    private static void ProcessPeaceProposal(PlayerController player, PlayerController targetPlayer, World.World world, ILogger logger)
    {
        logger.LogInformation("Peace treaty proposed between {PlayerId} and {TargetPlayerId}", player.PlayerId, targetPlayer.PlayerId);
    }

    private static void ProcessTradeProposal(PlayerController player, PlayerController targetPlayer, World.World world, ILogger logger)
    {
        logger.LogInformation("Trade agreement proposed between {PlayerId} and {TargetPlayerId}", player.PlayerId, targetPlayer.PlayerId);
    }

    private static void ProcessWarDeclaration(PlayerController player, PlayerController targetPlayer, World.World world, ILogger logger)
    {
        logger.LogInformation("War declared by {PlayerId} against {TargetPlayerId}", player.PlayerId, targetPlayer.PlayerId);
    }
}