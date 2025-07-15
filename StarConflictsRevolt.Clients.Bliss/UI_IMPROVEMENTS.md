# UI Improvements for StarConflictsRevolt Bliss Client

This document describes the UI improvements made to the Bliss client, including navigation bars, modals, and centering utilities.

## Overview

The UI system has been enhanced with the following components:

1. **UILayout** - Utility class for positioning and centering UI elements
2. **NavigationBar** - Horizontal navigation bar with Star Wars Rebellion styling
3. **ModalDialog** - Modal dialog system with animations and keyboard support
4. **Button** - Reusable button component with consistent styling
5. **UIManager** - Central manager coordinating all UI components

## Components

### UILayout

The `UILayout` class provides utility methods for positioning UI elements:

```csharp
// Center a rectangle on screen
var centeredRect = UILayout.CenteredRect(width, height, screenWidth, screenHeight);

// Center horizontally only
var horizontalCentered = UILayout.CenterHorizontal(rect, screenWidth);

// Position at top of screen
var topRect = UILayout.TopRect(width, height, screenWidth, yOffset);

// Position at bottom of screen
var bottomRect = UILayout.BottomRect(width, height, screenWidth, screenHeight, yOffset);

// Create grid layout
var grid = UILayout.CreateGrid(x, y, width, height, columns, rows, spacing);
```

### NavigationBar

A horizontal navigation bar with Star Wars Rebellion styling:

```csharp
var navBar = new NavigationBar();
navBar.SetBounds(UILayout.TopRect(screenWidth, 60f, screenWidth, 10f));

// Add navigation items
navBar.AddItem(new NavigationItem("Main Menu", "Main Menu"));
navBar.AddItem(new NavigationItem("Galaxy Overview", "Galaxy Overview"));

// Handle navigation
navBar.ViewRequested += (viewName) => SwitchToView(viewName);
```

**Features:**
- Animated selection indicators
- Keyboard navigation (Tab, Shift+Tab, Enter)
- Star Wars Rebellion corner decorations
- Glowing selection effects

### ModalDialog

A modal dialog system with animations and keyboard support:

```csharp
var modal = new ModalDialog();
modal.SetBounds(UILayout.CenteredRect(500f, 350f, screenWidth, screenHeight));

// Show simple message
modal.ShowMessage("Title", "Message", "OK");

// Show confirmation dialog
modal.ShowConfirmation("Confirm", "Are you sure?", onConfirm, onCancel);

// Show custom dialog
modal.ShowCustom("Title", "Message", 
    new ModalButton("Cancel", onCancel, false, true),
    new ModalButton("Confirm", onConfirm, true));
```

**Features:**
- Fade-in and scale animations
- Keyboard navigation (Arrow keys, Enter, Escape)
- Semi-transparent backdrop
- Star Wars Rebellion styling with corner decorations

### Button

A reusable button component with consistent styling:

```csharp
var button = new Button("Click Me", () => Console.WriteLine("Clicked!"));
button.SetBounds(new RectangleF(100, 100, 200, 50));
button.CenterOnScreen(screenWidth, screenHeight);

// Update and render
button.Update(deltaTime);
button.Render(primitiveBatch, commandList, framebuffer);
```

**Features:**
- Multiple states (normal, selected, hovered, disabled)
- Animated selection glow
- Selection arrow indicator
- Consistent Star Wars Rebellion styling

### UIManager

Central manager coordinating all UI components:

```csharp
var uiManager = new UIManager(screenWidth, screenHeight);

// Handle view requests
uiManager.ViewRequested += (viewName) => SwitchToView(viewName);

// Show dialogs
uiManager.ShowMessage("Title", "Message");
uiManager.ShowConfirmation("Confirm", "Are you sure?", onConfirm);

// Update and render
uiManager.Update(deltaTime);
uiManager.Render(immediateRenderer, primitiveBatch, spriteBatch, commandList, framebuffer);
```

## Integration

The UI system is integrated into the main render loop:

1. **Input Handling**: UI input is processed first, with modals taking priority
2. **Rendering**: UI components are rendered after the main view
3. **Updates**: UI components are updated each frame

## Keyboard Shortcuts

- **Tab**: Navigate to next navigation item
- **Shift+Tab**: Navigate to previous navigation item
- **Enter**: Activate selected item
- **F1**: Switch to Main Menu
- **F2**: Switch to Galaxy Overview
- **F3**: Switch to Tactical Battle
- **F12**: Toggle navigation bar visibility
- **M**: Show demo message modal
- **C**: Show demo confirmation modal
- **Escape**: Close modals or return to main menu

## Styling

All UI components use the `StarWarsTheme` color scheme:

- **Empire Colors**: Red/gray theme for primary elements
- **Rebellion Colors**: Blue/white theme for secondary elements
- **Neutral Colors**: Gray for disabled/inactive elements
- **Background Colors**: Dark space blue for backgrounds
- **Accent Colors**: Bright red/blue for highlights and borders

## Usage Examples

### Creating a Centered Menu

```csharp
public class CenteredMenuView : GameView
{
    private readonly List<Button> _buttons = new();
    
    public CenteredMenuView() : base("Centered Menu")
    {
        // Create buttons
        var startButton = new Button("Start Game", () => ViewRequested?.Invoke("Game"));
        var optionsButton = new Button("Options", () => ViewRequested?.Invoke("Options"));
        var exitButton = new Button("Exit", () => Environment.Exit(0));
        
        // Position buttons
        startButton.SetBounds(UILayout.CenteredRect(300, 60, 1920, 1080));
        startButton.Bounds.Y = 400;
        
        optionsButton.SetBounds(UILayout.CenteredRect(300, 60, 1920, 1080));
        optionsButton.Bounds.Y = 480;
        
        exitButton.SetBounds(UILayout.CenteredRect(300, 60, 1920, 1080));
        exitButton.Bounds.Y = 560;
        
        _buttons.AddRange(new[] { startButton, optionsButton, exitButton });
    }
    
    public override void Render(ImmediateRenderer immediateRenderer, PrimitiveBatch primitiveBatch, 
                               SpriteBatch spriteBatch, CommandList commandList, Framebuffer framebuffer)
    {
        // Render background
        DrawBackground(primitiveBatch, commandList, framebuffer);
        
        // Render buttons
        foreach (var button in _buttons)
        {
            button.Render(primitiveBatch, commandList, framebuffer);
        }
    }
}
```

### Using Modals for User Confirmation

```csharp
// In your view or game logic
uiManager.ShowConfirmation(
    "Delete Save Game", 
    "Are you sure you want to delete this save game? This action cannot be undone.",
    () => {
        // Delete the save game
        DeleteSaveGame();
        uiManager.ShowMessage("Success", "Save game deleted successfully.", "OK");
    },
    () => {
        // User cancelled
        uiManager.ShowMessage("Cancelled", "Save game deletion cancelled.", "OK");
    }
);
```

## Future Enhancements

Potential future improvements:

1. **Tooltips**: Hover tooltips for UI elements
2. **Animations**: More sophisticated animations and transitions
3. **Themes**: Multiple theme support (Empire, Rebellion, Neutral)
4. **Accessibility**: Screen reader support and keyboard navigation improvements
5. **Localization**: Multi-language support for UI text
6. **Customization**: User-configurable UI layouts and themes 