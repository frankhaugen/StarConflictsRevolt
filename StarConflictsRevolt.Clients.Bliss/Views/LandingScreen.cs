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
using StarConflictsRevolt.Clients.Shared.Player;

namespace StarConflictsRevolt.Clients.Bliss.Views;

/// <summary>
/// Landing screen that appears on game launch with navigation options.
/// Implements the specifications from BasicUiDescription.md.
/// </summary>
public class LandingScreen : BaseScreen
{
    private readonly IInputHandler _inputHandler;
    private readonly IPlayerProfileProvider _playerProfileProvider;
    private readonly UIScalingService _scalingService;
    private readonly TextRenderingService _textService;
    private readonly List<UIButton> _buttons = new();
    private int _selectedButtonIndex = 0;
    private float _time = 0f;
    private bool _showDebugMode = false;
    
    public LandingScreen(IInputHandler inputHandler, IPlayerProfileProvider playerProfileProvider, 
                        UIScalingService scalingService, TextRenderingService textService) 
        : base("landing", "STAR CONFLICTS: REVOLT")
    {
        _inputHandler = inputHandler ?? throw new ArgumentNullException(nameof(inputHandler));
        _playerProfileProvider = playerProfileProvider ?? throw new ArgumentNullException(nameof(playerProfileProvider));
        _scalingService = scalingService ?? throw new ArgumentNullException(nameof(scalingService));
        _textService = textService ?? throw new ArgumentNullException(nameof(textService));
        
        InitializeButtons();
    }
    
    public override void OnActivate()
    {
        base.OnActivate();
        _playerProfileProvider.LoadPlayerProfile();
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
        
        // Draw title text
        DrawTitleText(spriteBatch, commandList, framebuffer);
        
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
            var baseBounds = new RectangleF(
                _scalingService.CenterHorizontally(buttonWidth), // Center horizontally
                startY + i * spacing,
                buttonWidth,
                buttonHeight
            );
            
            // Scale the bounds to current window size
            var scaledBounds = _scalingService.ScaleRectangle(baseBounds);
            var button = new UIButton(_inputHandler, text, scaledBounds, action);
            _buttons.Add(button);
        }
        
        // Add debug mode button if enabled
        if (_showDebugMode)
        {
            var baseDebugBounds = new RectangleF(
                _scalingService.CenterHorizontally(buttonWidth),
                startY + buttonConfigs.Length * spacing,
                buttonWidth,
                buttonHeight
            );
            
            var scaledDebugBounds = _scalingService.ScaleRectangle(baseDebugBounds);
            var debugButton = new UIButton(_inputHandler, "Start Debug Mode", scaledDebugBounds, 
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
        
        var currentRes = _scalingService.CurrentResolution;
        
        // Draw animated stars
        for (int i = 0; i < 300; i++)
        {
            var baseX = (i * 12345) % 1920;
            var baseY = (i * 67890) % 1080;
            var scaledPos = _scalingService.ScalePosition(new Vector2(baseX, baseY));
            
            // Animate star brightness
            var brightness = (float)(Math.Sin(_time * 2 + i * 0.1f) * 0.5f + 0.5f);
            var color = new Color((byte)(brightness * 255), (byte)(brightness * 0.8f * 255), (byte)(brightness * 255), 255);
            
            var baseSize = 1 + (i % 3);
            var scaledSize = _scalingService.ScaleSize(new Vector2(baseSize, baseSize)).X;
            primitiveBatch.DrawFilledCircle(
                scaledPos, 
                scaledSize, 
                8, 
                0.5f, 
                color);
        }
        
        // Draw nebula-like effects
        for (int i = 0; i < 5; i++)
        {
            var baseCenter = new Vector2(
                200 + i * 300 + (float)Math.Sin(_time * 0.5f + i) * 50,
                200 + i * 150 + (float)Math.Cos(_time * 0.3f + i) * 30
            );
            var scaledCenter = _scalingService.ScalePosition(baseCenter);
            
            var nebulaColor = new Color(26, 51, 102, 26);
            var baseRadius = 100 + (float)Math.Sin(_time + i) * 20;
            var scaledRadius = _scalingService.ScaleSize(new Vector2(baseRadius, baseRadius)).X;
            primitiveBatch.DrawFilledCircle(
                scaledCenter, 
                scaledRadius, 
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
        var baseTitlePanel = new RectangleF(_scalingService.CenterHorizontally(1120f), 100f, 1120f, 200f);
        var scaledTitlePanel = _scalingService.ScaleRectangle(baseTitlePanel);
        primitiveBatch.DrawFilledRectangle(
            scaledTitlePanel, 
            Vector2.Zero, 
            0f, 
            0.5f, 
            new Color(26, 26, 51, 204));
        
        // Draw title border
        var baseBorderRect = new RectangleF(baseTitlePanel.X - 3, baseTitlePanel.Y - 3, baseTitlePanel.Width + 6, baseTitlePanel.Height + 6);
        var scaledBorderRect = _scalingService.ScaleRectangle(baseBorderRect);
        primitiveBatch.DrawFilledRectangle(
            scaledBorderRect, 
            Vector2.Zero, 
            0f, 
            0.5f, 
            StarWarsTheme.Border);
        
        // Draw subtitle line
        var baseLineStart = new Vector2(baseTitlePanel.X + 50, baseTitlePanel.Y + 180);
        var baseLineEnd = new Vector2(baseTitlePanel.X + baseTitlePanel.Width - 50, baseTitlePanel.Y + 180);
        var scaledLineStart = _scalingService.ScalePosition(baseLineStart);
        var scaledLineEnd = _scalingService.ScalePosition(baseLineEnd);
        var scaledLineWidth = _scalingService.ScaleSize(new Vector2(2f, 2f)).X;
        primitiveBatch.DrawLine(
            scaledLineStart, 
            scaledLineEnd, 
            scaledLineWidth, 
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
        var playerName = _playerProfileProvider.GetPlayerProfile()?.Name ?? "Unknown Player";
        if (!string.IsNullOrEmpty(playerName))
        {
            primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
            
            // Draw user info panel
            var baseUserPanel = new RectangleF(50f, 50f, 300f, 40f);
            var scaledUserPanel = _scalingService.ScaleRectangle(baseUserPanel);
            primitiveBatch.DrawFilledRectangle(
                scaledUserPanel, 
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
        var baseInstructionsPanel = new RectangleF(50f, 1000f, 400f, 60f);
        var scaledInstructionsPanel = _scalingService.ScaleRectangle(baseInstructionsPanel);
        primitiveBatch.DrawFilledRectangle(
            scaledInstructionsPanel, 
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
        var baseDebugPanel = new RectangleF(50f, 150f, 200f, 40f);
        var scaledDebugPanel = _scalingService.ScaleRectangle(baseDebugPanel);
        primitiveBatch.DrawFilledRectangle(
            scaledDebugPanel, 
            Vector2.Zero, 
            0f, 
            0.5f, 
            new Color(102, 26, 26, 153));
        
        primitiveBatch.End();
    }
    
    private void DrawTitleText(SpriteBatch spriteBatch, CommandList commandList, Framebuffer framebuffer)
    {
        // Calculate title position (centered in title panel)
        var baseTitlePos = new Vector2(_scalingService.CenterHorizontally(1120f) + 560f, 180f); // Center of title panel
        var scaledTitlePos = _scalingService.ScalePosition(baseTitlePos);
        var scaledFontSize = _scalingService.ScaleFontSize(48f);
        
        // Draw main title
        _textService.DrawText(spriteBatch, "STAR CONFLICTS: REVOLT", scaledTitlePos, commandList, framebuffer.OutputDescription, "Galaxy", scaledFontSize, (Color?)Color.White, TextAlignment.Center);
        
        // Draw subtitle
        var baseSubtitlePos = new Vector2(_scalingService.CenterHorizontally(1120f) + 560f, 220f);
        var scaledSubtitlePos = _scalingService.ScalePosition(baseSubtitlePos);
        var scaledSubtitleFontSize = _scalingService.ScaleFontSize(24f);
        
        _textService.DrawText(spriteBatch, "A New Hope Rises", scaledSubtitlePos, commandList, framebuffer.OutputDescription, "Galaxy", scaledSubtitleFontSize, (Color?)StarWarsTheme.EmpireAccent, TextAlignment.Center);
    }
} 