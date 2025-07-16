using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Transformations;
using StarConflictsRevolt.Clients.Bliss.Core.UI;
using StarConflictsRevolt.Clients.Bliss.Core.UI.Components;
using StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;
using System.Numerics;
using Veldrid;
using Color = Bliss.CSharp.Colors.Color;
using RectangleF = Bliss.CSharp.Transformations.RectangleF;

namespace StarConflictsRevolt.Clients.Bliss.Views;

/// <summary>
/// Placeholder screen for screens that haven't been implemented yet.
/// </summary>
public class PlaceholderScreen : BaseScreen
{
    private readonly IInputHandler _inputHandler;
    private readonly UIScalingService _scalingService;
    private readonly SimpleTextRenderer _textRenderer;
    private readonly string _screenName;
    private readonly SimpleButton _backButton;
    
    public PlaceholderScreen(
        IInputHandler inputHandler,
        UIScalingService scalingService,
        SimpleTextRenderer textRenderer,
        string screenId,
        string screenName)
        : base(screenId, screenName)
    {
        _inputHandler = inputHandler;
        _scalingService = scalingService;
        _textRenderer = textRenderer;
        _screenName = screenName;
        
        // Create back button
        var buttonWidth = 200f;
        var buttonHeight = 60f;
        var baseBounds = new RectangleF(
            _scalingService.CenterHorizontally(buttonWidth),
            600f,
            buttonWidth,
            buttonHeight
        );
        var scaledBounds = _scalingService.ScaleRectangle(baseBounds);
        _backButton = new SimpleButton(_inputHandler, "Back to Menu", scaledBounds, 
            () => RequestNavigation("landing"), _textRenderer);
    }
    
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        _backButton.Update(deltaTime);
    }
    
    public override void HandleInput()
    {
        base.HandleInput();
        
        // Handle Escape key
        if (_inputHandler.IsKeyPressed(KeyboardKey.Escape))
        {
            RequestNavigation("landing");
        }
    }
    
    public override void Render(ImmediateRenderer immediateRenderer,
                               PrimitiveBatch primitiveBatch,
                               SpriteBatch spriteBatch,
                               CommandList commandList,
                               Framebuffer framebuffer)
    {
        // Draw background
        var currentRes = _scalingService.CurrentResolution;
        primitiveBatch.DrawFilledRectangle(
            new RectangleF(0, 0, currentRes.X, currentRes.Y),
            Vector2.Zero, 0f, 0.1f, Color.Black);
        
        // Draw title panel
        var baseTitlePanel = new RectangleF(_scalingService.CenterHorizontally(800f), 200f, 800f, 150f);
        var scaledTitlePanel = _scalingService.ScaleRectangle(baseTitlePanel);
        primitiveBatch.DrawFilledRectangle(
            scaledTitlePanel, 
            Vector2.Zero, 
            0f, 
            0.2f,
            new Color(26, 26, 51, 204));
        
        // Draw title border
        var baseBorderRect = new RectangleF(baseTitlePanel.X - 3, baseTitlePanel.Y - 3, baseTitlePanel.Width + 6, baseTitlePanel.Height + 6);
        var scaledBorderRect = _scalingService.ScaleRectangle(baseBorderRect);
        primitiveBatch.DrawFilledRectangle(
            scaledBorderRect, 
            Vector2.Zero, 
            0f, 
            0.25f,
            new Color(102, 102, 153, 255));
        
        // Draw message panel
        var baseMessagePanel = new RectangleF(_scalingService.CenterHorizontally(600f), 400f, 600f, 100f);
        var scaledMessagePanel = _scalingService.ScaleRectangle(baseMessagePanel);
        primitiveBatch.DrawFilledRectangle(
            scaledMessagePanel, 
            Vector2.Zero, 
            0f, 
            0.2f,
            new Color(26, 26, 51, 204));
        
        // Draw back button
        _backButton.Render(immediateRenderer, primitiveBatch, spriteBatch, commandList, framebuffer);
        
        // Draw text
        DrawTitleText(spriteBatch, commandList, framebuffer);
        DrawMessageText(spriteBatch, commandList, framebuffer);
        DrawButtonText(spriteBatch, commandList, framebuffer);
    }
    
    private void DrawTitleText(SpriteBatch spriteBatch, CommandList commandList, Framebuffer framebuffer)
    {
        var baseTitlePanel = new RectangleF(_scalingService.CenterHorizontally(800f), 200f, 800f, 150f);
        var scaledTitlePanel = _scalingService.ScaleRectangle(baseTitlePanel);
        
        _textRenderer.DrawTextCentered(
            _screenName, scaledTitlePanel, spriteBatch, "Default", 32f, Color.White);
    }
    
    private void DrawMessageText(SpriteBatch spriteBatch, CommandList commandList, Framebuffer framebuffer)
    {
        var baseMessagePanel = new RectangleF(_scalingService.CenterHorizontally(600f), 400f, 600f, 100f);
        var scaledMessagePanel = _scalingService.ScaleRectangle(baseMessagePanel);
        
        _textRenderer.DrawTextCentered(
            "This screen is not yet implemented.", scaledMessagePanel, spriteBatch, "Default", 18f, Color.White);
    }
    
    private void DrawButtonText(SpriteBatch spriteBatch, CommandList commandList, Framebuffer framebuffer)
    {
        _textRenderer.DrawTextCentered(
            _backButton.Text, _backButton.Bounds, spriteBatch, "Default", 18f, Color.White);
    }
} 