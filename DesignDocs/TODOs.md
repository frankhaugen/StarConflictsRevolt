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
  - [x] Add technology tree (basic)
  - [x] Add resource types and conversion rates

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
- [x] **Create technology system**
  - [x] Basic tech tree with 3-5 levels
  - [x] Ship upgrades (weapons, armor, engines)
  - [x] Structure upgrades (efficiency, capacity)
  - [x] Research costs and time
  - [x] Technology prerequisites

- [x] **Add victory conditions**
  - [x] Military victory: Control 75% of planets
  - [x] Economic victory: Accumulate 10,000 credits
  - [x] Technology victory: Research all techs
  - [x] Diplomatic victory: Form alliances (future feature)
  - [x] Time victory: Survive for 100 turns

- [ ] **Create game scenarios**
  - [ ] Tutorial scenario: 1v1 with AI (small galaxy)
  - [ ] Quick battle: 2v2 small galaxy (10-15 planets)
  - [ ] Epic battle: 4-player large galaxy (25-30 planets)
  - [ ] Campaign: Series of connected scenarios (future feature)

- [x] **Add resource management**
  - [x] Resource storage limits per planet
  - [x] Resource transfer between planets
  - [x] Resource consumption for maintenance
  - [x] Resource bonuses from structures

---

## ⚔️ **4. CombatSimulator**

### **High Priority**
- [x] **Create `CombatSimulatorService`** for managing all combat scenarios
  - [x] Add method `SimulateFleetCombat(Fleet attacker, Fleet defender, Planet location)`
  - [x] Add method `SimulatePlanetaryCombat(Fleet attacker, Planet defender)`
  - [x] Add method `SimulateDeathStarRun(Fleet attacker, DeathStar defender)`
  - [x] Add method `SimulateMission(Mission mission, Character agent, Planet target)`
  - [x] Support different combat environments (space, planetary, special)

- [x] **Implement Fleet Combat Simulation**
  - [x] Ship-to-ship combat resolution
  - [x] Fleet formation and positioning effects
  - [x] Weapon range and accuracy calculations
  - [x] Shield and armor damage absorption
  - [x] Critical hit system and special abilities
  - [x] Retreat mechanics and morale effects
  - [x] Combat result reporting and fleet losses

- [x] **Implement Planetary Combat Simulation**
  - [x] Ground troop vs ground troop combat
  - [x] Ship bombardment of planetary defenses
  - [x] Planetary shield effects and generator targeting
  - [x] Structure damage and destruction
  - [x] Population resistance and loyalty effects
  - [x] Planetary capture mechanics
  - [x] Environmental effects (terrain, weather)

- [ ] **Implement Death Star Run Combat**
  - [ ] Trench run mechanics and timing
  - [ ] Turbolaser defense systems
  - [ ] TIE Fighter escort and interception
  - [ ] Shield generator targeting and destruction
  - [ ] Exhaust port vulnerability and timing
  - [ ] Hero character effects and Force abilities
  - [ ] Death Star destruction sequence

### **Medium Priority**
- [ ] **Create Mission System**
  - [ ] Character assignment and skill requirements
  - [ ] Mission types: Diplomacy, Espionage, Sabotage, Rescue, etc.
  - [ ] Mission success/failure probability calculations
  - [ ] Mission duration and time-based events
  - [ ] Mission rewards and consequences
  - [ ] Character experience and skill progression

- [ ] **Add Character System**
  - [ ] Character stats (Leadership, Combat, Diplomacy, Espionage)
  - [ ] Character abilities and special powers
  - [ ] Force sensitivity and Jedi/Sith abilities
  - [ ] Character relationships and loyalty
  - [ ] Character death and replacement mechanics

- [ ] **Implement Advanced Combat Features**
  - [ ] Weather and environmental effects on combat
  - [ ] Terrain bonuses and penalties
  - [ ] Supply lines and logistics effects
  - [ ] Intelligence and reconnaissance effects
  - [ ] Surprise attacks and ambush mechanics

### **Low Priority**
- [ ] **Add Cinematic Combat Sequences**
  - [ ] Real-time combat visualization
  - [ ] Camera angles and dramatic effects
  - [ ] Sound effects and music integration
  - [ ] Slow-motion and highlight reels
  - [ ] Combat replay and analysis tools

- [ ] **Create Special Combat Scenarios**
  - [ ] Asteroid field navigation and combat
  - [ ] Nebula effects and sensor interference
  - [ ] Space station assaults
  - [ ] Capital ship boarding actions
  - [ ] Escape pod and rescue missions

---

## 🖥️ **5. UI Overhaul - Modern Sci-Fi Interface**

### **High Priority**
- [ ] **High Resolution Support**
  - [ ] Support 4K (3840x2160) and ultrawide resolutions
  - [ ] Implement proper DPI scaling for high-DPI displays
  - [ ] Add resolution detection and automatic scaling
  - [ ] Create responsive UI layouts that adapt to screen size
  - [ ] Support multiple monitor setups

- [ ] **Enhanced Camera System**
  - [ ] Smooth zooming with mouse wheel (0.1x to 10x zoom levels)
  - [ ] Pan with middle mouse button or arrow keys
  - [ ] Double-click to center on objects (planets, fleets, etc.)
  - [ ] Camera inertia and momentum for smooth movement
  - [ ] Zoom-to-fit functionality for selected objects
  - [ ] Camera bookmarks for quick navigation

- [ ] **Modern Sci-Fi UI Design**
  - [ ] EVE Online-inspired color scheme (dark blues, oranges, whites)
  - [ ] Glowing borders and subtle animations
  - [ ] High-contrast, readable fonts (Roboto, Inter, or similar)
  - [ ] Semi-transparent panels with backdrop blur
  - [ ] Consistent iconography and visual language
  - [ ] Smooth transitions and hover effects

### **Medium Priority**
- [ ] **Separated UI Architecture**
  - [ ] Create `IUiElement` interface for all UI components
  - [ ] Implement `UiManager` to handle UI layer separation
  - [ ] Separate game world rendering from UI overlays
  - [ ] Create modular UI components (panels, buttons, tooltips)
  - [ ] Implement UI state management system
  - [ ] Add UI theming and customization options

- [ ] **Advanced UI Components**
  - [ ] Collapsible side panels with smooth animations
  - [ ] Context-sensitive tooltips with rich information
  - [ ] Drag-and-drop interfaces for fleet management
  - [ ] Modal dialogs with backdrop blur
  - [ ] Progress bars and loading indicators
  - [ ] Notification system with different priority levels

- [ ] **Information Display**
  - [ ] HUD overlay with key game information
  - [ ] Minimap with zoom and pan capabilities
  - [ ] Resource counters with real-time updates
  - [ ] Fleet status indicators with health bars
  - [ ] System information panels
  - [ ] Player statistics and achievements display

### **Low Priority**
- [ ] **Advanced Visual Effects**
  - [ ] Particle effects for UI interactions
  - [ ] Screen space reflections and lighting
  - [ ] Dynamic backgrounds with star fields
  - [ ] Smooth fade transitions between views
  - [ ] Ambient lighting effects
  - [ ] Post-processing effects for UI elements

- [ ] **Accessibility Features**
  - [ ] High contrast mode for colorblind users
  - [ ] Scalable UI elements for vision-impaired users
  - [ ] Keyboard navigation for all UI elements
  - [ ] Screen reader support
  - [ ] Customizable UI layouts
  - [ ] Multiple language support

- [ ] **Performance Optimizations**
  - [ ] UI element pooling for dynamic content
  - [ ] Efficient rendering of large UI hierarchies
  - [ ] Lazy loading of UI components
  - [ ] GPU-accelerated UI rendering
  - [ ] Memory-efficient texture management
  - [ ] Frame rate optimization for smooth animations

---

## 🧪 **6. Testing Requirements**

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

## 📁 **7. File Structure Changes**

### **New Files to Create**
```
StarConflictsRevolt.Server.WebApi/
├── Services/
│   ├── GameSetupService.cs ✅
│   ├── GameContentService.cs ✅
│   └── AiStrategies/
│       ├── AggressiveAiStrategy.cs ✅
│       ├── EconomicAiStrategy.cs ✅
│       ├── DefensiveAiStrategy.cs ✅
│       └── BalancedAiStrategy.cs ✅
├── Models/
│   ├── PlayerSetup.cs ✅
│   ├── GameSetup.cs ✅
│   ├── ShipTemplate.cs ✅
│   ├── StructureTemplate.cs ✅
│   └── PlanetType.cs ✅
└── Enums/
    ├── PlayerType.cs ✅
    ├── AiDifficulty.cs ✅
    ├── VictoryCondition.cs ✅
    └── GameMode.cs ✅
```

### **Files to Modify**
- [x] `WorldFactory.cs` - Add starting condition methods
- [x] `DefaultAiStrategy.cs` - Enhance with proper logic
- [x] `SessionService.cs` - Add game setup integration
- [x] `World.cs` - Add resource properties
- [x] `PlayerController.cs` - Add AI strategy support

---

## 🎯 **8. Success Criteria**

### **Minimal Viable Game**
- [x] Players can start a new game with balanced conditions
- [x] AI makes reasonable decisions and provides challenge
- [x] Basic resource management works
- [x] Victory conditions are achievable
- [x] Game can be completed in 30-60 minutes

### **Quality Metrics**
- [x] All tests pass
- [x] No memory leaks in long-running games
- [x] AI response time < 5 seconds
- [x] Balanced win rates (40-60% for human vs AI)
- [x] Smooth real-time updates

---

## 🏗️ **9. Infrastructure Improvements**

### **Completed**
- [x] **API Endpoint Handler Refactoring**
  - [x] Split monolithic `MinimalApiHelper` into modular handlers
  - [x] Created `ApiEndpointHandler` as main orchestrator
  - [x] Organized endpoints by concern:
    - [x] `HealthEndpointHandler` - Health checks and status
    - [x] `AuthEndpointHandler` - Authentication and tokens
    - [x] `SessionEndpointHandler` - Session management
    - [x] `GameActionEndpointHandler` - Game commands
    - [x] `LeaderboardEndpointHandler` - Leaderboard endpoints
    - [x] `EventEndpointHandler` - Event store and snapshots
  - [x] Updated `StartupHelper` and test infrastructure
  - [x] Maintained full backward compatibility
  - [x] All tests pass (127/127)
  - [x] Created comprehensive documentation

### **Future Infrastructure Work**
- [ ] **API Versioning Strategy**
  - [ ] Implement proper API versioning for breaking changes
  - [ ] Add version-specific endpoint handlers
  - [ ] Create migration guides for client updates

- [ ] **API Documentation**
  - [ ] Enhance OpenAPI/Swagger documentation
  - [ ] Add endpoint-specific documentation
  - [ ] Create API usage examples

- [ ] **Performance Monitoring**
  - [ ] Add endpoint performance metrics
  - [ ] Implement request/response logging
  - [ ] Add health check endpoints for dependencies

---

## 📝 **10. Notes**

- **Priority**: Focus on High Priority items first to achieve minimal playability
- **Testing**: Write tests alongside implementation, not after
- **Balance**: Iterate on game balance based on playtesting
- **Performance**: Monitor performance impact of new features
- **Documentation**: Update design docs as features are implemented

---

*Last Updated: December 2024*
*Status: Core Game Systems Complete - Focus on UI and Advanced Features* 