using StarConflictsRevolt.Server.EventStorage.Abstractions;
using StarConflictsRevolt.Server.Domain;

namespace StarConflictsRevolt.Server.Domain.Events;

public record DiplomacyEvent(Guid PlayerId, Guid TargetPlayerId, string ProposalType, string? Message) : IGameEvent
{
    public void ApplyTo(object world, ILogger logger)
    {
        var w = (Domain.World.World)world;
        // Find the players
        var player = w.Players?.FirstOrDefault(p => p.PlayerId == PlayerId);
        var targetPlayer = w.Players?.FirstOrDefault(p => p.PlayerId == TargetPlayerId);

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
                ProcessAllianceProposal(player, targetPlayer, w, logger);
                break;
            case "peace":
                ProcessPeaceProposal(player, targetPlayer, w, logger);
                break;
            case "trade":
                ProcessTradeProposal(player, targetPlayer, w, logger);
                break;
            case "war":
                ProcessWarDeclaration(player, targetPlayer, w, logger);
                break;
            default:
                logger.LogWarning("Unknown diplomacy proposal type: {ProposalType}", ProposalType);
                break;
        }

        logger.LogInformation("Diplomacy event processed: {PlayerId} -> {TargetPlayerId} ({ProposalType})",
            PlayerId, TargetPlayerId, ProposalType);
    }

    private static void ProcessAllianceProposal(IPlayerController player, IPlayerController targetPlayer, Domain.World.World world, ILogger logger)
    {
        // For now, automatically accept alliance proposals
        // In a full implementation, this would require acceptance from the target player
        logger.LogInformation("Alliance formed between {PlayerId} and {TargetPlayerId}", player.PlayerId, targetPlayer.PlayerId);

        // Update player relations (in a full implementation, this would be stored in a relations table)
        // For now, just log the alliance
    }

    private static void ProcessPeaceProposal(IPlayerController player, IPlayerController targetPlayer, Domain.World.World world, ILogger logger)
    {
        logger.LogInformation("Peace treaty proposed between {PlayerId} and {TargetPlayerId}", player.PlayerId, targetPlayer.PlayerId);
    }

    private static void ProcessTradeProposal(IPlayerController player, IPlayerController targetPlayer, Domain.World.World world, ILogger logger)
    {
        logger.LogInformation("Trade agreement proposed between {PlayerId} and {TargetPlayerId}", player.PlayerId, targetPlayer.PlayerId);
    }

    private static void ProcessWarDeclaration(IPlayerController player, IPlayerController targetPlayer, Domain.World.World world, ILogger logger)
    {
        logger.LogInformation("War declared by {PlayerId} against {TargetPlayerId}", player.PlayerId, targetPlayer.PlayerId);
    }
}