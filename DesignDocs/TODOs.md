# TODO List: Starting Conditions, AI Strategy, and Game Content

## 📋 Overview

This document outlines the implementation plan for adding essential game mechanics to make StarConflictsRevolt playable. The focus is on three key areas:

1. **Starting Conditions for New Games** - Proper session initialization and balanced starting positions
2. **AI Default Strategy** - Intelligent computer opponents with different personalities
3. **Game Content (Placeholders)** - Ships, structures, planets, and basic game systems

---

## 🎯 **1. Starting Conditions for New Games**

### **High Priority**
- [x] **Create `GameSetupService`** to handle session initialization
  - [x] Add method `CreateNewGameSession(string sessionName, List<PlayerSetup> players)`
  - [x] Support different game modes (1v1, 2v2, FFA, Human vs AI)
  - [x] Add session configuration options (galaxy size, starting resources, etc.)

- [x] **Enhance `WorldFactory`** with proper starting conditions
  - [x] Add `CreateStartingWorld(GameSetup setup)` method
  - [x] Create multiple star systems with strategic positioning
  - [x] Add starting fleets for each player (3-5 ships per player)
  - [x] Add starting structures (1-2 per starting planet)
  - [x] Add starting resources (credits, materials, fuel)

- [x] **Add `PlayerSetup` model**
  - [x] `PlayerType` enum (Human, AI)
  - [x] `StartingPosition` (which star system/planet)
  - [x] `AiStrategy` (for AI players)
  - [x] `PlayerName` and `PlayerColor`
  - [x] `Difficulty` level for AI players

- [x] **Create starting fleet templates**
  - [x] Small fleet: 1-2 ships (scout/patrol)
  - [x] Medium fleet: 3-5 ships (combat)
  - [x] Large fleet: 6-10 ships (invasion)
  - [x] Balanced fleet: Mix of ship types

### **Medium Priority**
- [x] **Add starting structure templates**
  - [x] Basic: Mine + Construction Yard
  - [x] Military: Training Facility + Shield Generator
  - [x] Economic: Mine + Refinery + Shipyard
  - [x] Defensive: Shield Generator + Training Facility

- [x] **Create balanced starting positions**
  - [x] 2-player: Opposite sides of galaxy
  - [x] 4-player: Four corners
  - [x] FFA: Distributed evenly
  - [x] Team games: Allies positioned together

- [x] **Add resource distribution**
  - [x] Starting credits: 1000 per player
  - [x] Starting materials: 500 per player
  - [x] Starting fuel: 200 per player
  - [x] Resource generation rates per planet

---

## 🤖 **2. AI Default Strategy Improvements**

### **High Priority**
- [x] **Enhance `DefaultAiStrategy`** with proper decision making
  - [x] Add resource management logic
  - [x] Implement fleet positioning strategy
  - [x] Add building priority system
  - [x] Create threat assessment
  - [x] Add economic planning

- [x] **Add AI personality types**
  - [x] `AggressiveAiStrategy`: Focuses on combat and expansion
  - [x] `EconomicAiStrategy`: Focuses on building and resources
  - [x] `DefensiveAiStrategy`: Focuses on fortification
  - [x] `BalancedAiStrategy`: Mix of all approaches
  - [x] `RandomAiStrategy`: Current implementation (for testing)

- [x] **Implement AI decision framework**
  - [x] Add `AiDecision` class with priority scoring
  - [x] Create `AiGoal` system (expand, defend, attack, build)
  - [x] Add `AiMemory` to track previous decisions
  - [x] Implement basic pathfinding for fleet movement
  - [x] Add strategic planning (short-term vs long-term goals)

### **Medium Priority**
- [ ] **Add AI difficulty levels**
  - [ ] Easy: Random actions, slow response (5-10 second delays)
  - [ ] Normal: Basic strategy, moderate response (3-5 second delays)
  - [ ] Hard: Advanced strategy, fast response (1-3 second delays)
  - [ ] Expert: Complex strategy, aggressive (immediate response)

- [ ] **Create AI learning system**
  - [ ] Track successful vs failed actions
  - [ ] Adjust strategy based on opponent behavior
  - [ ] Learn from player tactics
  - [ ] Adapt to different play styles

- [ ] **Add AI cooperation for team games**
  - [ ] Coordinate attacks with allies
  - [ ] Share resources when beneficial
  - [ ] Defend allied planets
  - [ ] Strategic positioning to support allies

---

## 🎮 **3. Game Content (Placeholders)**

### **High Priority**
- [x] **Create `GameContentService`** for managing game data
  - [x] Add ship templates with stats (health, attack, speed, cost)
  - [x] Add structure templates with effects
  - [x] Add planet types with bonuses
  - [ ] Add technology tree (basic)
  - [ ] Add resource types and conversion rates

- [x] **Add ship types and stats**
  - [x] Scout: Fast (speed 3), weak (attack 1, health 10), cheap (cost 50)
  - [x] Fighter: Balanced (speed 2, attack 2, health 20), medium cost (cost 100)
  - [x] Destroyer: Strong (speed 1, attack 4, health 40), expensive (cost 200)
  - [x] Cruiser: Heavy (speed 1, attack 6, health 60), very expensive (cost 400)
  - [x] Transport: High capacity (cargo 50), weak combat (attack 1, health 30), medium cost (cost 150)

- [x] **Add structure effects**
  - [x] Mine: +10 materials per turn
  - [x] Refinery: +5 fuel per turn
  - [x] Shipyard: Can build ships (cost: 100 materials + 50 fuel per ship)
  - [x] Training Facility: Can train troops (cost: 50 materials per troop)
  - [x] Shield Generator: +50% planet defense, +20% structure health
  - [x] Construction Yard: Reduces building costs by 25%

- [x] **Add planet types**
  - [x] Terran: Balanced, good for starting (+5 all resources)
  - [x] Desert: +materials (+15), -food (-5)
  - [x] Ice: +fuel (+15), -materials (-5)
  - [x] Gas Giant: +fuel (+20), can't build structures
  - [x] Asteroid: +materials (+20), -habitability (reduced population)
  - [x] Ocean: +food (+15), -materials (-5)

### **Medium Priority**
- [ ] **Create technology system**
  - [ ] Basic tech tree with 3-5 levels
  - [ ] Ship upgrades (weapons, armor, engines)
  - [ ] Structure upgrades (efficiency, capacity)
  - [ ] Research costs and time
  - [ ] Technology prerequisites

- [ ] **Add victory conditions**
  - [ ] Military victory: Control 75% of planets
  - [ ] Economic victory: Accumulate 10,000 credits
  - [ ] Technology victory: Research all techs
  - [ ] Diplomatic victory: Form alliances (future feature)
  - [ ] Time victory: Survive for 100 turns

- [ ] **Create game scenarios**
  - [ ] Tutorial scenario: 1v1 with AI (small galaxy)
  - [ ] Quick battle: 2v2 small galaxy (10-15 planets)
  - [ ] Epic battle: 4-player large galaxy (25-30 planets)
  - [ ] Campaign: Series of connected scenarios (future feature)

- [ ] **Add resource management**
  - [ ] Resource storage limits per planet
  - [ ] Resource transfer between planets
  - [ ] Resource consumption for maintenance
  - [ ] Resource bonuses from structures

---

## 🔧 **4. Implementation Order**

### **Week 1: Foundation**
1. Create `GameSetupService` and `PlayerSetup` model
2. Enhance `WorldFactory` with starting conditions
3. Add basic ship and structure templates
4. Create starting fleet and structure collections
5. Add resource system basics

### **Week 2: AI Enhancement**
1. Improve `DefaultAiStrategy` with proper decision making
2. Add AI personality types
3. Implement basic AI difficulty levels
4. Add AI memory and learning system
5. Create AI decision framework

### **Week 3: Game Content**
1. Create `GameContentService`
2. Add planet types and effects
3. Implement basic technology system
4. Add victory conditions
5. Create game scenarios

### **Week 4: Polish & Testing**
1. Balance gameplay mechanics
2. Add comprehensive tests
3. Performance optimization
4. UI improvements for new features
5. Documentation updates

---

## 🧪 **5. Testing Requirements**

### **Unit Tests**
- [ ] `GameSetupService` tests
- [ ] `WorldFactory` starting condition tests
- [ ] AI strategy behavior tests
- [ ] `GameContentService` data validation tests
- [ ] Resource management tests

### **Integration Tests**
- [ ] Complete game setup flow
- [ ] AI vs AI game simulation
- [ ] Human vs AI game flow
- [ ] Multi-player session creation
- [ ] Resource generation and consumption

### **Performance Tests**
- [ ] Large galaxy performance (50+ planets)
- [ ] Multiple AI players (4+ AI)
- [ ] Long-running games (100+ turns)
- [ ] Memory usage optimization
- [ ] Network bandwidth usage

### **Balance Tests**
- [ ] Starting condition fairness
- [ ] AI difficulty scaling
- [ ] Resource economy balance
- [ ] Combat system balance
- [ ] Victory condition achievability

---

## 📁 **6. File Structure Changes**

### **New Files to Create**
```
StarConflictsRevolt.Server.WebApi/
├── Services/
│   ├── GameSetupService.cs
│   ├── GameContentService.cs
│   └── AiStrategies/
│       ├── AggressiveAiStrategy.cs
│       ├── EconomicAiStrategy.cs
│       ├── DefensiveAiStrategy.cs
│       └── BalancedAiStrategy.cs
├── Models/
│   ├── PlayerSetup.cs
│   ├── GameSetup.cs
│   ├── ShipTemplate.cs
│   ├── StructureTemplate.cs
│   └── PlanetType.cs
└── Enums/
    ├── PlayerType.cs
    ├── AiDifficulty.cs
    ├── VictoryCondition.cs
    └── GameMode.cs
```

### **Files to Modify**
- [ ] `WorldFactory.cs` - Add starting condition methods
- [ ] `DefaultAiStrategy.cs` - Enhance with proper logic
- [ ] `SessionService.cs` - Add game setup integration
- [ ] `World.cs` - Add resource properties
- [ ] `PlayerController.cs` - Add AI strategy support

---

## 🎯 **7. Success Criteria**

### **Minimal Viable Game**
- [ ] Players can start a new game with balanced conditions
- [ ] AI makes reasonable decisions and provides challenge
- [ ] Basic resource management works
- [ ] Victory conditions are achievable
- [ ] Game can be completed in 30-60 minutes

### **Quality Metrics**
- [ ] All tests pass
- [ ] No memory leaks in long-running games
- [ ] AI response time < 5 seconds
- [ ] Balanced win rates (40-60% for human vs AI)
- [ ] Smooth real-time updates

---

## 📝 **8. Notes**

- **Priority**: Focus on High Priority items first to achieve minimal playability
- **Testing**: Write tests alongside implementation, not after
- **Balance**: Iterate on game balance based on playtesting
- **Performance**: Monitor performance impact of new features
- **Documentation**: Update design docs as features are implemented

---

*Last Updated: [Current Date]*
*Status: Planning Phase* 