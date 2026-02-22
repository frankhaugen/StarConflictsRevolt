# Basic UI Description

This document describes the general ideas of how the UI should look and feel.

## Navigation Hiearchy

- Landing Sceen
    - Start new Single Player Game
        - Set Game Name
    - Start new Multiplayer Game
        - Set Game Name
    - Join existing Game
        - Select from a list
    - See leaderboards
        - Leaderboards
    - Exit
        - End application gracefully
    - (Start Debug Mode)
        > Start game that has proceedurally generated its state so all game screens can be explored witout connecting to server

## Screens

### Landing Screen

#### Description of UX

On game launch this is the first thing one sees. There are a list of buttons that can be selected by arrow keys (enter-key to trigger) or clicked on. User details are collected at startup using the IUserProfileProvider as a factory but in DI "(i)UserProfile" is sufficient to get access to the stored user inforrmation like NAME, which are used to identify the player implicitly.

#### Asci representation

```plaintext
+-------------------------------------------------------------+
|                  STAR CONFLICTS: REVOLT                     |
|-------------------------------------------------------------|
|                                                             |
|              [ Start New Single Player Game ]               |
|              [ Start New Multiplayer Game   ]               |
|              [ Join Existing Game           ]               |
|              [ See Leaderboards             ]               |
|              [ Exit                         ]               |
|                                                             |
|-------------------------------------------------------------|
|   (Press ↑/↓ to navigate, Enter to select)                  |
|                                                             |
|   (Start Debug Mode)                                        |
+-------------------------------------------------------------+

// User: <NAME>
//
// On launch, the user's name is shown (if available) and all menu items
// can be navigated with arrow keys or mouse. Enter triggers the selected action.
// Debug Mode is shown only in dev builds or with a special key.

```

### Start New Single-Player Game

#### Description of UX

When the user selects "Start New Single Player Game" from the landing screen, they are taken to a new screen where they can enter a name for their game session. The UI consists of a text input field (pre-filled with a default or last-used name, if available), and two buttons: "Start" and "Back". The user can type a name, use Tab/Shift+Tab to move focus between the input and buttons, and press Enter to activate the focused control. The "Back" button returns to the landing screen.

If the user presses "Start" with a valid name, a new single-player game session is created and the game transitions to the main gameplay view. If the name is empty or invalid, an error message is shown inline below the input.

#### Ascii representation

```plaintext
+-------------------------------------------------------------+
|                  START NEW SINGLE PLAYER GAME               |
|-------------------------------------------------------------|
|                                                             |
|   Enter session name: [ My Awesome Game           ]         |
|                                                             |
|                                                             |
|                 [   Start   ]                               |
|                 [   Back    ]                               |
|                                                             |
|-------------------------------------------------------------|
|   (Tab/Shift+Tab to move, Enter to select)                  |
|                                                             |
+-------------------------------------------------------------+

// If error:
|                  [Error: Name cannot be empty]              |


```

### Start New Multiplayer Game

#### Description of UX

When the user selects "Start New Multiplayer Game" from the landing screen, they are taken to a screen for setting up a multiplayer session. The UI includes a text input field for the session name and a toggle for "Public Game" (visible to other players). The user can navigate between controls using Tab/Shift+Tab and press Enter to activate the focused control.

If "Public Game" is enabled, the session will be visible to other players who can join. If disabled, only players with the session ID can join. The "Start" button creates the multiplayer session and transitions to a waiting room where the host can see connected players and start the game when ready.

#### ASCII representation

```plaintext
+-------------------------------------------------------------+
|                 START NEW MULTIPLAYER GAME                  |
|-------------------------------------------------------------|
|                                                             |
|   Enter session name: [ Galactic Conquest 2024    ]         |
|                                                             |
|   [✓] Public Game (visible to other players)                |
|                                                             |
|                 [   Start   ]                               |
|                 [   Back    ]                               |
|                                                             |
|-------------------------------------------------------------|
|   (Tab/Shift+Tab to move, Enter to select)                  |
|                                                             |
+-------------------------------------------------------------+

// If error:
|                  [Error: Name cannot be empty]              |

```

### Join Existing Game

#### Description of UX

When the user selects "Join Existing Game" from the landing screen, they are presented with a list of available public games and an option to join by session ID. The screen shows a scrollable list of active games with their names, player counts, and status (Waiting, In Progress, etc.). The user can navigate through the list using arrow keys and press Enter to join a selected game.

Alternatively, there's a "Join by ID" option that allows entering a specific session ID for private games. The "Refresh" button updates the list of available games, and "Back" returns to the landing screen.

#### ASCII representation

```plaintext
+-------------------------------------------------------------+
|                    JOIN EXISTING GAME                       |
|-------------------------------------------------------------|
|                                                             |
|   Available Games:                                          |
|                                                             |
|   > [1] Galactic Conquest 2024 (2/4 players) - Waiting      |
|     [2] Rebel Alliance (1/4 players) - Waiting              |
|     [3] Empire Strikes Back (3/4 players) - In Progress     |
|     [4] Clone Wars (0/4 players) - Waiting                  |
|                                                             |
|   Or join by Session ID: [ ABC123XYZ           ]            |
|                                                             |
|                 [  Refresh  ]                               |
|                 [   Back    ]                               |
|                                                             |
|-------------------------------------------------------------|
|   (↑/↓ to navigate list, Enter to join, Tab for ID input)   |
|                                                             |
+-------------------------------------------------------------+

// If no games available:
|                  [No public games available]                |

```

### See Leaderboards

#### Description of UX

The leaderboards screen displays player rankings and statistics in a tabular format. The screen shows multiple tabs for different categories: "Overall Rankings", "Recent Games", "Win Rate", and "Total Games Played". Each tab displays a scrollable list of players with their relevant statistics.

The user can switch between tabs using Tab/Shift+Tab or arrow keys, and scroll through the rankings using Up/Down arrows. The "Back" button returns to the landing screen. The current user's position is highlighted in the list.

#### ASCII representation

```plaintext
+-------------------------------------------------------------+
|                        LEADERBOARDS                         |
|-------------------------------------------------------------|
|                                                             |
|   [Overall Rankings] [Recent Games] [Win Rate] [Total Games]|
|-------------------------------------------------------------|
|                                                             |
|   Rank | Player Name        | Wins | Losses | Win Rate      |
|   ----- | ----------------- | ---- | ------ | ---------     |
|   > 1   | DarthVader        | 45   | 12     | 78.9%         |
|     2   | LukeSkywalker     | 38   | 15     | 71.7%         |
|     3   | HanSolo           | 32   | 18     | 64.0%         |
|     4   | LeiaOrgana        | 28   | 22     | 56.0%         |
|     5   | ObiWanKenobi      | 25   | 25     | 50.0%         |
|                                                             |
|   Your Rank: 12th (15 wins, 20 losses, 42.9%)               |
|                                                             |
|                 [   Back    ]                               |
|                                                             |
|-------------------------------------------------------------|
|   (Tab to switch categories, ↑/↓ to scroll, Enter to select)|
|                                                             |
+-------------------------------------------------------------+

```

### Exit

#### Description of UX

When the user selects "Exit" from the landing screen, a confirmation dialog appears to prevent accidental exits. The dialog asks "Are you sure you want to exit?" with "Yes" and "No" options. The user can navigate between options using arrow keys or Tab/Shift+Tab and press Enter to confirm their choice.

If the user has an active game session, the dialog also warns about unsaved progress and offers to save the game before exiting.

#### ASCII representation

```plaintext
+-------------------------------------------------------------+
|                        CONFIRM EXIT                         |
|-------------------------------------------------------------|
|                                                             |
|   Are you sure you want to exit?                            |
|                                                             |
|   ⚠️  Warning: You have an active game session.             |
|      Unsaved progress will be lost.                         |
|                                                             |
|                 [   Yes   ]                                 |
|                 [   No    ]                                 |
|                                                             |
|-------------------------------------------------------------|
|   (Tab/Shift+Tab to move, Enter to select)                  |
|                                                             |
+-------------------------------------------------------------+

// If no active session:
|                                                             |
|   Are you sure you want to exit?                            |
|                                                             |
|                 [   Yes   ]                                 |
|                 [   No    ]                                 |

```

### Debug Mode

#### Description of UX

Debug Mode is only available in development builds or when activated with a special key combination (e.g., Ctrl+Shift+D). When activated, it creates a procedurally generated game state that allows developers and testers to explore all game screens without connecting to a server.

The debug mode screen shows options for generating different types of game scenarios: "Empty Galaxy", "Pre-populated Galaxy", "Combat Scenario", "Resource Management", etc. Each option creates a different starting state for testing various game mechanics.

#### ASCII representation

```plaintext
+-------------------------------------------------------------+
|                        DEBUG MODE                           |
|-------------------------------------------------------------|
|                                                             |
|   Select debug scenario:                                    |
|                                                             |
|   > [ Empty Galaxy (no fleets, structures) ]                |
|     [ Pre-populated Galaxy (AI players) ]                   |
|     [ Combat Scenario (fleets in battle) ]                  |
|     [ Resource Management (rich planets) ]                  |
|     [ Technology Tree (all techs unlocked) ]                |
|     [ Victory Conditions (near completion) ]                |
|                                                             |
|   [✓] Enable AI players                                     |
|   [✓] Enable all technologies                               |
|   [ ] Infinite resources                                    |
|                                                             |
|                 [   Start   ]                               |
|                 [   Back    ]                               |
|                                                             |
|-------------------------------------------------------------|
|   (↑/↓ to navigate, Enter to select, Tab for options)       |
|                                                             |
+-------------------------------------------------------------+

```

### Galaxy Screen

#### Description of UX

The Galaxy Screen is the main gameplay view that appears after starting a game (single-player or multiplayer). It displays a top-down view of the galaxy with star systems, planets, fleets, and other game objects. The screen is divided into several areas: the main galaxy view, a top information bar, and a bottom control panel.

The galaxy view shows star systems as nodes connected by hyperspace lanes. Players can click on or navigate to star systems to view details, manage fleets, and perform actions. The top bar displays current resources, turn information, and game status. The bottom panel contains action buttons for fleet management, diplomacy, technology, and other game functions.

Navigation is primarily mouse-driven with keyboard shortcuts for common actions. The view can be zoomed and panned to explore different regions of the galaxy.

#### ASCII representation

```plaintext
+-------------------------------------------------------------+
| Resources: Credits: 1500 | Metal: 800 | Food: 1200 | Turn: 5 |
|-------------------------------------------------------------|
|                                                             |
|                    GALAXY VIEW                              |
|                                                             |
|                                                             |
|         ⭐ Coruscant (Capital)                              |
|            /     \                                          |
|           /       \                                         |
|    ⭐ Naboo    ⭐ Tatooine                                  |
|      [Fleet]      [Enemy]                                   |
|         |            |                                      |
|         |            |                                      |
|    ⭐ Alderaan   ⭐ Hoth                                    |
|      [Planet]     [Planet]                                  |
|                                                             |
|                                                             |
|-------------------------------------------------------------|
| [Fleets] [Planets] [Diplomacy] [Technology] [Missions] [Menu]|
|                                                             |
| Selected: Coruscant - Capital World                         |
| Actions: [Build Fleet] [Upgrade] [Trade] [Diplomacy]        |
+-------------------------------------------------------------+

// Fleet view:
| Selected: Imperial Fleet (5 ships)                          |
| Actions: [Move] [Attack] [Explore] [Return to Base]         |

// Planet view:
| Selected: Naboo - Rich in resources                         |
| Actions: [Build Structure] [Harvest] [Defend] [Trade]       |

```

## General UI Guidelines

### Navigation
- All screens support both keyboard and mouse navigation
- Arrow keys (↑/↓/←/→) for list and menu navigation
- Tab/Shift+Tab for moving between input fields and buttons
- Enter key to activate the currently focused control
- Escape key to go back or cancel current action

### Visual Design
- Consistent border style using ASCII characters (+, -, |)
- Clear section headers with underlines
- Highlighted selection indicators (> or [✓])
- Error messages displayed inline below relevant fields
- Status information shown in footer area

### Accessibility
- All interactive elements can be accessed via keyboard
- Clear visual indicators for current focus
- Descriptive labels for all controls
- Consistent navigation patterns across all screens

