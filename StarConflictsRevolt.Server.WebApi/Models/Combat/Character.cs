namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class Character
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public CharacterType Type { get; set; }
    public int Level { get; set; } = 1;
    public int Experience { get; set; }
    public Guid OwnerId { get; set; }

    // Core stats
    public int Leadership { get; set; } = 10;
    public int Combat { get; set; } = 10;
    public int Diplomacy { get; set; } = 10;
    public int Espionage { get; set; } = 10;
    public int Intelligence { get; set; } = 10;

    // Special abilities
    public List<CharacterAbility> Abilities { get; set; } = new();
    public bool IsForceSensitive { get; set; } = false;
    public ForceAlignment ForceAlignment { get; set; } = ForceAlignment.None;

    // Current state
    public bool IsAlive { get; set; } = true;
    public bool IsAvailable { get; set; } = true;
    public Guid? CurrentMissionId { get; set; }
    public int Loyalty { get; set; } = 100;

    // Relationships
    public List<CharacterRelationship> Relationships { get; set; } = new();

    public int GetSkillForMission(MissionType missionType)
    {
        return missionType switch
        {
            MissionType.Diplomacy => Diplomacy,
            MissionType.Espionage => Espionage,
            MissionType.Sabotage => Espionage + Combat,
            MissionType.Rescue => Combat + Leadership,
            MissionType.Assassination => Combat + Espionage,
            MissionType.Infiltration => Espionage + Intelligence,
            MissionType.Reconnaissance => Intelligence + Espionage,
            MissionType.Smuggling => Espionage + Diplomacy,
            MissionType.BountyHunting => Combat + Intelligence,
            MissionType.Training => Leadership + Intelligence,
            _ => (Leadership + Combat + Diplomacy + Espionage + Intelligence) / 5
        };
    }

    public void GainExperience(int amount)
    {
        Experience += amount;
        var experienceForNextLevel = Level * 100;

        if (Experience >= experienceForNextLevel)
        {
            Level++;
            Experience -= experienceForNextLevel;

            // Increase stats based on character type
            IncreaseStatsOnLevelUp();
        }
    }

    private void IncreaseStatsOnLevelUp()
    {
        switch (Type)
        {
            case CharacterType.Diplomat:
                Diplomacy += 2;
                Leadership += 1;
                break;
            case CharacterType.Warrior:
                Combat += 2;
                Leadership += 1;
                break;
            case CharacterType.Spy:
                Espionage += 2;
                Intelligence += 1;
                break;
            case CharacterType.Commander:
                Leadership += 2;
                Combat += 1;
                break;
            case CharacterType.Scientist:
                Intelligence += 2;
                Espionage += 1;
                break;
            case CharacterType.Jedi:
                if (IsForceSensitive)
                {
                    Combat += 2;
                    Leadership += 1;
                    Intelligence += 1;
                }

                break;
            case CharacterType.Sith:
                if (IsForceSensitive)
                {
                    Combat += 2;
                    Espionage += 1;
                    Intelligence += 1;
                }

                break;
        }
    }
}