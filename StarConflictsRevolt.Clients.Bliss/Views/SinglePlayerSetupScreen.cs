using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Interact.Keyboards;
using StarConflictsRevolt.Clients.Bliss.Core;
using StarConflictsRevolt.Clients.Bliss.Core.UI;
using StarConflictsRevolt.Clients.Bliss.Core.UI.Components;
using StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;
using Veldrid;
using Color = Bliss.CSharp.Colors.Color;
using RectangleF = Bliss.CSharp.Transformations.RectangleF;
using StarConflictsRevolt.Clients.Shared.Player;
using StarConflictsRevolt.Clients.Shared.Http;
using StarConflictsRevolt.Clients.Shared.Communication;
using StarConflictsRevolt.Clients.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace StarConflictsRevolt.Clients.Bliss.Views;

/// <summary>
/// Screen for setting up a new single-player game session.
/// Implements the specifications from BasicUiDescription.md.
/// </summary>
public class SinglePlayerSetupScreen : BaseScreen
{
    private readonly IInputHandler _inputHandler;
    private readonly IPlayerProfileProvider _playerProfileProvider;
    private readonly SimpleTextRenderer _textRenderer;
    private readonly IServiceProvider _serviceProvider;
    private readonly List<UIButton> _buttons = new();
    private readonly List<UIComponent> _inputFields = new();
    private int _selectedComponentIndex = 0;
    private string _sessionName = "";
    private string _errorMessage = "";
    private bool _hasError = false;
    
    public SinglePlayerSetupScreen(IInputHandler inputHandler, IPlayerProfileProvider playerProfileProvider, SimpleTextRenderer textRenderer, IServiceProvider serviceProvider) 
        : base("single-player-setup", "START NEW SINGLE PLAYER GAME")
    {
        _inputHandler = inputHandler ?? throw new ArgumentNullException(nameof(inputHandler));
        _playerProfileProvider = playerProfileProvider ?? throw new ArgumentNullException(nameof(playerProfileProvider));
        _textRenderer = textRenderer ?? throw new ArgumentNullException(nameof(textRenderer));
        this._serviceProvider = serviceProvider;

        InitializeComponents();
    }
    
    public override void OnActivate()
    {
        base.OnActivate();
        
        // Pre-fill with default name or last used name
        _playerProfileProvider.LoadPlayerProfile();
        var playerName = _playerProfileProvider.GetPlayerProfile()?.Name;
        if (!string.IsNullOrEmpty(playerName))
        {
            _sessionName = $"{playerName}'s Game";
        }
        else
        {
            _sessionName = "My Awesome Game";
        }
        
        UpdateComponentSelection();
    }
    
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        
        // Update all components
        foreach (var component in _inputFields.Concat(_buttons.Cast<UIComponent>()))
        {
            component.Update(deltaTime);
        }
        
        HandleKeyboardInput();
    }
    
    public override void Render(ImmediateRenderer immediateRenderer,
                               PrimitiveBatch primitiveBatch,
                               SpriteBatch spriteBatch,
                               CommandList commandList,
                               Framebuffer framebuffer)
    {
        // Draw background
        DrawBackground(primitiveBatch, commandList, framebuffer);
        
        // Draw title
        DrawTitle(primitiveBatch, commandList, framebuffer);
        
        // Draw input field
        DrawInputField(immediateRenderer, primitiveBatch, spriteBatch, commandList, framebuffer);
        
        // Draw buttons
        DrawButtons(immediateRenderer, primitiveBatch, spriteBatch, commandList, framebuffer);
        
        // Draw error message if any
        if (_hasError)
        {
            DrawErrorMessage(primitiveBatch, commandList, framebuffer);
        }
        
        // Draw navigation instructions
        DrawNavigationInstructions(primitiveBatch, commandList, framebuffer);
    }
    
    public override void HandleInput()
    {
        base.HandleInput();
        
        // Handle text input for session name
        HandleTextInput();
    }
    
    private void InitializeComponents()
    {
        var componentWidth = 400f;
        var componentHeight = 60f;
        var startY = 300f;
        var spacing = 80f;
        
        // Create input field for session name
        var inputBounds = new RectangleF(
            (1920f - componentWidth) / 2f,
            startY,
            componentWidth,
            componentHeight
        );
        
        var inputField = new UITextInput(_inputHandler, "Enter session name", inputBounds, 
            () => _sessionName, (value) => _sessionName = value, _textRenderer);
        _inputFields.Add(inputField);
        
        // Create buttons
        var buttonConfigs = new (string text, Action action)[]
        {
            ("Start", () => StartGame()),
            ("Back", () => RequestNavigation("landing"))
        };
        
        for (int i = 0; i < buttonConfigs.Length; i++)
        {
            var (text, action) = buttonConfigs[i];
            var bounds = new RectangleF(
                (1920f - componentWidth) / 2f,
                startY + spacing + i * spacing,
                componentWidth,
                componentHeight
            );
            
            var button = new UIButton(_inputHandler, text, bounds, action, _textRenderer);
            _buttons.Add(button);
        }
    }
    
    private void HandleKeyboardInput()
    {
        var totalComponents = _inputFields.Count + _buttons.Count;
        
        // Handle Tab/Shift+Tab navigation
        if (_inputHandler.IsKeyPressed(KeyboardKey.Tab))
        {
            if (_inputHandler.IsKeyDown(KeyboardKey.ShiftLeft))
            {
                _selectedComponentIndex = (_selectedComponentIndex - 1 + totalComponents) % totalComponents;
            }
            else
            {
                _selectedComponentIndex = (_selectedComponentIndex + 1) % totalComponents;
            }
            UpdateComponentSelection();
        }
        
        // Handle Enter key activation
        if (_inputHandler.IsKeyPressed(KeyboardKey.Enter))
        {
            ActivateSelectedComponent();
        }
        
        // Handle Escape key
        if (_inputHandler.IsKeyPressed(KeyboardKey.Escape))
        {
            RequestNavigation("landing");
        }
    }
    
    private void HandleTextInput()
    {
        // Handle text input for the session name field
        // This is a simplified implementation - in a real app you'd have proper text input handling
        if (_selectedComponentIndex == 0) // Input field is selected
        {
            // Handle backspace
            if (_inputHandler.IsKeyPressed(KeyboardKey.BackSpace) && _sessionName.Length > 0)
            {
                _sessionName = _sessionName[..^1];
            }
            
            // Handle character input (simplified)
            // In a real implementation, you'd handle all printable characters
            if (_inputHandler.IsKeyPressed(KeyboardKey.A))
            {
                _sessionName += "a";
            }
            // Add more character handling as needed
        }
    }
    
    private void UpdateComponentSelection()
    {
        var allComponents = _inputFields.Concat(_buttons.Cast<UIComponent>()).ToList();
        
        for (int i = 0; i < allComponents.Count; i++)
        {
            if (allComponents[i] is UIButton button)
            {
                button.SetSelected(i == _selectedComponentIndex);
            }
            else if (allComponents[i] is UITextInput input)
            {
                input.SetSelected(i == _selectedComponentIndex);
            }
        }
    }
    
    private void ActivateSelectedComponent()
    {
        var allComponents = _inputFields.Concat(_buttons.Cast<UIComponent>()).ToList();
        
        if (_selectedComponentIndex >= 0 && _selectedComponentIndex < allComponents.Count)
        {
            if (allComponents[_selectedComponentIndex] is UIButton button)
            {
                button.Activate();
            }
        }
    }
    
    private async void StartGame()
    {
        // Validate session name
        if (string.IsNullOrWhiteSpace(_sessionName))
        {
            _errorMessage = "Name cannot be empty";
            _hasError = true;
            return;
        }
        if (_sessionName.Length < 3)
        {
            _errorMessage = "Name must be at least 3 characters";
            _hasError = true;
            return;
        }
        _hasError = false;
        _errorMessage = "";
        try
        {
            // Get required services
            var apiClient = _serviceProvider.GetRequiredService<IHttpApiClient>();
            var signalR = _serviceProvider.GetRequiredService<ISignalRService>();
            var playerProfileProvider = _serviceProvider.GetRequiredService<IPlayerProfileProvider>();
            var playerName = playerProfileProvider.GetPlayerProfile()?.Name ?? "Player";
            // Create session using the extension method
            var sessionResponse = await apiClient.CreateNewSessionAsync(_sessionName, "SinglePlayer", CancellationToken.None);
            if (sessionResponse == null || sessionResponse.SessionId == Guid.Empty)
            {
                _errorMessage = "Failed to create session. Please check your connection.";
                _hasError = true;
                return;
            }
            // Store session info for the galaxy screen
            GalaxyScreen.CurrentSessionId = sessionResponse.SessionId;
            GalaxyScreen.CurrentWorld = sessionResponse.World;
            // Start SignalR and join session
            await signalR.StartAsync();
            await signalR.JoinSessionAsync(sessionResponse.SessionId);
            // Subscribe to world updates
            signalR.FullWorldReceived += world => GalaxyScreen.CurrentWorld = world;
            signalR.UpdatesReceived += updates =>
            {
                // TODO: Apply updates to GalaxyScreen.CurrentWorld
            };
            // Navigate to galaxy screen
            RequestNavigation("galaxy");
        }
        catch (Exception ex)
        {
            _errorMessage = $"Error: {ex.Message}";
            _hasError = true;
        }
    }
    
    private void DrawBackground(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        // Draw background panel
        var backgroundPanel = new RectangleF(200f, 100f, 1520f, 800f);
        primitiveBatch.DrawFilledRectangle(
            backgroundPanel, 
            Vector2.Zero, 
            0f, 
            0.5f, 
            new Color(26, 26, 51, 204));
        
        // Draw border
        primitiveBatch.DrawFilledRectangle(
            new RectangleF(backgroundPanel.X - 3, backgroundPanel.Y - 3, backgroundPanel.Width + 6, backgroundPanel.Height + 6), 
            Vector2.Zero, 
            0f, 
            0.5f, 
            StarWarsTheme.Border);
    }
    
    private void DrawTitle(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        // Draw title
        var titlePanel = new RectangleF(400f, 150f, 1120f, 100f);
        primitiveBatch.DrawFilledRectangle(
            titlePanel, 
            Vector2.Zero, 
            0f, 
            0.5f, 
            new Color(26, 26, 51, 153));
    }
    
    private void DrawInputField(ImmediateRenderer immediateRenderer, PrimitiveBatch primitiveBatch, SpriteBatch spriteBatch, CommandList commandList, Framebuffer framebuffer)
    {
        // Render input field components
        foreach (var inputField in _inputFields)
        {
            inputField.Render(immediateRenderer, primitiveBatch, spriteBatch, commandList, framebuffer);
        }
    }
    
    private void DrawButtons(ImmediateRenderer immediateRenderer, PrimitiveBatch primitiveBatch, SpriteBatch spriteBatch, CommandList commandList, Framebuffer framebuffer)
    {
        // Render button components
        foreach (var button in _buttons)
        {
            button.Render(immediateRenderer, primitiveBatch, spriteBatch, commandList, framebuffer);
        }
    }
    
    private void DrawErrorMessage(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        // Draw error message panel
        var errorPanel = new RectangleF(400f, 500f, 1120f, 60f);
        primitiveBatch.DrawFilledRectangle(
            errorPanel, 
            Vector2.Zero, 
            0f, 
            0.5f, 
            new Color(102, 26, 26, 153));
    }
    
    private void DrawNavigationInstructions(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        // Draw instructions panel at bottom
        var instructionsPanel = new RectangleF(400f, 700f, 1120f, 60f);
        primitiveBatch.DrawFilledRectangle(
            instructionsPanel, 
            Vector2.Zero, 
            0f, 
            0.5f, 
            new Color(26, 26, 51, 153));
    }
} 