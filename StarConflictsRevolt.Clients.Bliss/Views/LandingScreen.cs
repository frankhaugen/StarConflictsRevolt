using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Transformations;
using StarConflictsRevolt.Clients.Bliss.Core;
using StarConflictsRevolt.Clients.Bliss.Core.UI;
using StarConflictsRevolt.Clients.Bliss.Core.UI.Components;
using StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;
using System.Numerics;
using Veldrid;
using Color = Bliss.CSharp.Colors.Color;
using RectangleF = Bliss.CSharp.Transformations.RectangleF;

namespace StarConflictsRevolt.Clients.Bliss.Views;

/// <summary>
/// Landing screen that appears on game launch with navigation options.
/// Implements the specifications from BasicUiDescription.md.
/// </summary>
public class LandingScreen : BaseScreen
{
    private readonly IInputHandler _inputHandler;
    private readonly IUserProfileProvider _userProfileProvider;
    private readonly List<UIButton> _buttons = new();
    private int _selectedButtonIndex = 0;
    private float _time = 0f;
    private bool _showDebugMode = false;
    
    public LandingScreen(IInputHandler inputHandler, IUserProfileProvider userProfileProvider) 
        : base("landing", "STAR CONFLICTS: REVOLT")
    {
        _inputHandler = inputHandler ?? throw new ArgumentNullException(nameof(inputHandler));
        _userProfileProvider = userProfileProvider ?? throw new ArgumentNullException(nameof(userProfileProvider));
        
        InitializeButtons();
    }
    
    public override void OnActivate()
    {
        base.OnActivate();
        _userProfileProvider.LoadUserProfile();
        UpdateButtonSelection();
    }
    
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        _time += deltaTime;
        
        // Update all buttons
        foreach (var button in _buttons)
        {
            button.Update(deltaTime);
        }
        
        HandleKeyboardInput();
    }
    
    public override void Render(ImmediateRenderer immediateRenderer,
                               PrimitiveBatch primitiveBatch,
                               SpriteBatch spriteBatch,
                               CommandList commandList,
                               Framebuffer framebuffer)
    {
        // Draw animated starfield background
        DrawAnimatedStarfield(primitiveBatch, commandList, framebuffer);
        
        // Draw title panel
        DrawTitlePanel(primitiveBatch, commandList, framebuffer);
        
        // Draw menu buttons
        DrawMenuButtons(immediateRenderer, primitiveBatch, spriteBatch, commandList, framebuffer);
        
        // Draw user info
        DrawUserInfo(primitiveBatch, commandList, framebuffer);
        
        // Draw navigation instructions
        DrawNavigationInstructions(primitiveBatch, commandList, framebuffer);
        
        // Draw debug mode option if available
        if (_showDebugMode)
        {
            DrawDebugModeOption(primitiveBatch, commandList, framebuffer);
        }
    }
    
    public override void HandleInput()
    {
        base.HandleInput();
        
        // Check for debug mode activation (Ctrl+Shift+D)
        if (_inputHandler.IsKeyDown(KeyboardKey.ControlLeft) && 
            _inputHandler.IsKeyDown(KeyboardKey.ShiftLeft) && 
            _inputHandler.IsKeyPressed(KeyboardKey.D))
        {
            _showDebugMode = !_showDebugMode;
        }
    }
    
    private void InitializeButtons()
    {
        var buttonWidth = 400f;
        var buttonHeight = 60f;
        var startY = 400f;
        var spacing = 80f;
        
        // Create buttons as specified in the document
        var buttonConfigs = new (string text, Action action)[]
        {
            ("Start New Single Player Game", () => RequestNavigation("single-player-setup")),
            ("Start New Multiplayer Game", () => RequestNavigation("multiplayer-setup")),
            ("Join Existing Game", () => RequestNavigation("join-game")),
            ("See Leaderboards", () => RequestNavigation("leaderboards")),
            ("Exit", () => RequestExit())
        };
        
        for (int i = 0; i < buttonConfigs.Length; i++)
        {
            var (text, action) = buttonConfigs[i];
            var bounds = new RectangleF(
                (1920f - buttonWidth) / 2f, // Center horizontally
                startY + i * spacing,
                buttonWidth,
                buttonHeight
            );
            
            var button = new UIButton(_inputHandler, text, bounds, action);
            _buttons.Add(button);
        }
        
        // Add debug mode button if enabled
        if (_showDebugMode)
        {
            var debugBounds = new RectangleF(
                (1920f - buttonWidth) / 2f,
                startY + buttonConfigs.Length * spacing,
                buttonWidth,
                buttonHeight
            );
            
            var debugButton = new UIButton(_inputHandler, "Start Debug Mode", debugBounds, 
                () => RequestNavigation("debug-mode"));
            _buttons.Add(debugButton);
        }
    }
    
    private void HandleKeyboardInput()
    {
        // Handle arrow key navigation
        if (_inputHandler.IsKeyPressed(KeyboardKey.Up))
        {
            _selectedButtonIndex = (_selectedButtonIndex - 1 + _buttons.Count) % _buttons.Count;
            UpdateButtonSelection();
        }
        else if (_inputHandler.IsKeyPressed(KeyboardKey.Down))
        {
            _selectedButtonIndex = (_selectedButtonIndex + 1) % _buttons.Count;
            UpdateButtonSelection();
        }
        
        // Handle Enter key activation
        if (_inputHandler.IsKeyPressed(KeyboardKey.Enter))
        {
            if (_selectedButtonIndex >= 0 && _selectedButtonIndex < _buttons.Count)
            {
                _buttons[_selectedButtonIndex].Activate();
            }
        }
        
        // Handle Escape key
        if (_inputHandler.IsKeyPressed(KeyboardKey.Escape))
        {
            RequestExit();
        }
    }
    
    private void UpdateButtonSelection()
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            _buttons[i].SetSelected(i == _selectedButtonIndex);
        }
    }
    
    private void DrawAnimatedStarfield(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        // Draw animated stars
        for (int i = 0; i < 300; i++)
        {
            var x = (i * 12345) % 1920;
            var y = (i * 67890) % 1080;
            
            // Animate star brightness
            var brightness = (float)(Math.Sin(_time * 2 + i * 0.1f) * 0.5f + 0.5f);
            var color = new Color((byte)(brightness * 255), (byte)(brightness * 0.8f * 255), (byte)(brightness * 255), 255);
            
            var size = 1 + (i % 3);
            primitiveBatch.DrawFilledCircle(
                new Vector2(x, y), 
                size, 
                8, 
                0.5f, 
                color);
        }
        
        // Draw nebula-like effects
        for (int i = 0; i < 5; i++)
        {
            var center = new Vector2(
                200 + i * 300 + (float)Math.Sin(_time * 0.5f + i) * 50,
                200 + i * 150 + (float)Math.Cos(_time * 0.3f + i) * 30
            );
            
            var nebulaColor = new Color(26, 51, 102, 26);
            primitiveBatch.DrawFilledCircle(
                center, 
                100 + (float)Math.Sin(_time + i) * 20, 
                32, 
                0.1f, 
                nebulaColor);
        }
        
        primitiveBatch.End();
    }
    
    private void DrawTitlePanel(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        // Draw title background panel - centered
        var titlePanel = new RectangleF((1920f - 1120f) / 2f, 100f, 1120f, 200f);
        primitiveBatch.DrawFilledRectangle(
            titlePanel, 
            Vector2.Zero, 
            0f, 
            0.5f, 
            new Color(26, 26, 51, 204));
        
        // Draw title border
        primitiveBatch.DrawFilledRectangle(
            new RectangleF(titlePanel.X - 3, titlePanel.Y - 3, titlePanel.Width + 6, titlePanel.Height + 6), 
            Vector2.Zero, 
            0f, 
            0.5f, 
            StarWarsTheme.Border);
        
        // Draw subtitle line
        primitiveBatch.DrawLine(
            new Vector2(titlePanel.X + 50, titlePanel.Y + 180), 
            new Vector2(titlePanel.X + titlePanel.Width - 50, titlePanel.Y + 180), 
            2f, 
            0.5f, 
            StarWarsTheme.EmpireAccent);
        
        primitiveBatch.End();
    }
    
    private void DrawMenuButtons(ImmediateRenderer immediateRenderer, PrimitiveBatch primitiveBatch, SpriteBatch spriteBatch, CommandList commandList, Framebuffer framebuffer)
    {
        foreach (var button in _buttons)
        {
            button.Render(immediateRenderer, primitiveBatch, spriteBatch, commandList, framebuffer);
        }
    }
    
    private void DrawUserInfo(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        var userName = _userProfileProvider.GetUserName();
        if (!string.IsNullOrEmpty(userName))
        {
            primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
            
            // Draw user info panel
            var userPanel = new RectangleF(50f, 50f, 300f, 40f);
            primitiveBatch.DrawFilledRectangle(
                userPanel, 
                Vector2.Zero, 
                0f, 
                0.5f, 
                new Color(26, 26, 51, 153));
            
            primitiveBatch.End();
        }
    }
    
    private void DrawNavigationInstructions(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        // Draw instructions panel at bottom
        var instructionsPanel = new RectangleF(50f, 1000f, 400f, 60f);
        primitiveBatch.DrawFilledRectangle(
            instructionsPanel, 
            Vector2.Zero, 
            0f, 
            0.5f, 
            new Color(26, 26, 51, 153));
        
        primitiveBatch.End();
    }
    
    private void DrawDebugModeOption(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        // Draw debug mode indicator
        var debugPanel = new RectangleF(50f, 150f, 200f, 40f);
        primitiveBatch.DrawFilledRectangle(
            debugPanel, 
            Vector2.Zero, 
            0f, 
            0.5f, 
            new Color(102, 26, 26, 153));
        
        primitiveBatch.End();
    }
} 