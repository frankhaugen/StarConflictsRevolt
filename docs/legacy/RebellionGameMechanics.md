Based on the official *Star Wars Rebellion* manual, here's a full C# type model for the **Game Action Loop** and **Core Game Mechanics**, aligning with the gameplay structure:

---

## 🌀 Game Action Loop & Mechanics — C# Types

### ⏱ Game Loop Structure

```csharp
public class GameLoop
{
    public GameState CurrentState { get; private set; }
    public GameSpeed Speed { get; set; }
    public TimeSpan ElapsedGameTime { get; private set; }

    public void Tick();
    public void Pause();
    public void Resume();
}
```

```csharp
public enum GameSpeed { Paused, Slow, Normal, Fast }
public enum GameState { Strategic, Tactical, Paused, Observing }
```

---

### 🌌 Strategic Layer

```csharp
public class Galaxy
{
    public List<Sector> Sectors { get; set; } = [];
}

public class Sector
{
    public string Name { get; set; }
    public List<Planet> Planets { get; set; } = [];
}
```

```csharp
public class Planet
{
    public string Name { get; set; }
    public PlanetLoyalty Loyalty { get; set; }
    public bool IsBlockaded { get; set; }
    public List<Facility> Facilities { get; set; } = [];
    public List<Fleet> OrbitingFleets { get; set; } = [];
}
```

---

### 🧱 Economy, Production, Resources

```csharp
public class Facility
{
    public FacilityType Type { get; set; }
    public bool IsUnderConstruction { get; set; }
}

public enum FacilityType
{
    ConstructionYard, TrainingFacility, Shipyard,
    Mine, Refinery, ShieldGenerator
}
```

```csharp
public class ProductionOrder
{
    public string ItemName { get; set; }
    public ProductionType Type { get; set; }
    public int Quantity { get; set; }
    public TimeSpan TimeRemaining { get; set; }
}

public enum ProductionType { Ship, Troop, Facility }
```

---

### 🚀 Fleets, Ships, Hyperspace

```csharp
public class Fleet
{
    public string Name { get; set; }
    public List<Ship> Ships { get; set; } = [];
    public FleetStatus Status { get; set; }
    public Planet? OrbitingPlanet { get; set; }
}

public enum FleetStatus { Idle, EnRoute, Blockading, InCombat }
```

```csharp
public class Ship
{
    public string Model { get; set; }
    public HyperdriveRating Hyperdrive { get; set; }
    public bool IsUnderConstruction { get; set; }
}

public record HyperdriveRating(float Current, float Optimal);
```

---

### 🧑‍🚀 Characters, Missions, the Force

```csharp
public class Character
{
    public string Name { get; set; }
    public CharacterRank Rank { get; set; }
    public ForceAffinity Force { get; set; }
    public List<MissionAssignment> AssignedMissions { get; set; } = [];
}

public enum CharacterRank { None, Commander, General, Admiral }
public enum ForceAffinity { None, Latent, Trained, Jedi, Sith }

public class MissionAssignment
{
    public MissionType Mission { get; set; }
    public Planet TargetPlanet { get; set; }
    public DateTime StartTime { get; set; }
}
```

```csharp
public enum MissionType
{
    Diplomacy, Espionage, Sabotage, Abduction, Rescue,
    InciteUprising, Assassinate, Train, Research, Investigate
}
```

---

### 💥 Tactical Combat Layer

```csharp
public class TacticalBattle
{
    public Fleet Attacker { get; set; }
    public Fleet Defender { get; set; }
    public DeathStar? DeathStarInBattle { get; set; }
    public bool IsObserving { get; set; }
    public BattleStatus Status { get; set; }
}

public enum BattleStatus { Paused, Running, Victory, Defeat, Withdrawn }

public class DeathStar
{
    public float LaserCharge { get; set; }
    public TimeSpan RechargeTime { get; set; }

    public void FireAt(Ship target);
}
```

---

### 🌐 Multiplayer Sync & Time Control

```csharp
public class MultiplayerSession
{
    public List<PlayerConnection> Players { get; set; } = [];
    public bool IsPaused => Players.Any(p => p.IsPaused);
}

public class PlayerConnection
{
    public string PlayerName { get; set; }
    public string IpAddress { get; set; }
    public bool IsPaused { get; set; }
}
```

---

### 📈 Loyalty, Rebellion, Uprisings

```csharp
public class PlanetLoyalty
{
    public Faction Allegiance { get; set; }
    public int SupportLevel { get; set; } // Range: -100 to 100
}

public enum Faction { Empire, Alliance, Neutral }
```

---

Let me know if you'd like these bundled into a single `.cs` file, organized by namespace (e.g., `StarConflictsRevolt.Gameplay.Strategic`, etc.), or if you want interfaces or serialization attributes added.
