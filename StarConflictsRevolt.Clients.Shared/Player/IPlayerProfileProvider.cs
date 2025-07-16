namespace StarConflictsRevolt.Clients.Shared.Player;

public interface IPlayerProfileProvider
{
    PlayerProfile? GetPlayerProfile();
    void SetPlayerName(string name);
    bool HasPlayerProfile();
    void CreatePlayerProfile(string name);
    void SavePlayerProfile();
    void LoadPlayerProfile();
} 