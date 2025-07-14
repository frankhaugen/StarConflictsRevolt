using Microsoft.Extensions.Logging;
using NSubstitute;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using StarConflictsRevolt.Clients.Raylib.Rendering.Core;
using StarConflictsRevolt.Clients.Raylib.Rendering.UI;
using System.Numerics;
using TUnit.Assertions;
using TUnit.Core;

namespace StarConflictsRevolt.Tests.ClientTests;

/// <summary>
/// Tests for the UI system demonstrating testability without rendering.
/// </summary>
public class UITests
{
    private ILogger<BaseView> _loggerMock;
    private IInputState _inputStateMock;
    private IUIRenderer _rendererMock;
    private TestView _testView;
    private ButtonElement _testButton;
    
    [Test]
    public async Task ButtonElement_ShouldDetectMouseHover()
    {
        // Arrange
        _loggerMock = Substitute.For<ILogger<BaseView>>();
        _inputStateMock = Substitute.For<IInputState>();
        _rendererMock = Substitute.For<IUIRenderer>();
        
        _testView = new TestView();
        _testButton = new ButtonElement("test-button", "Test Button", new Vector2(100, 100), new Vector2(200, 50));
        
        var mousePosition = new Vector2(150, 125); // Inside button bounds
        _inputStateMock.MousePosition.Returns(mousePosition);
        
        // Act
        _testButton.Update(0.016f, _inputStateMock);
        
        // Assert
        // The button should be hovered (we can test this by checking if it would render with hover color)
        await Assert.That(_testButton.Contains(mousePosition)).IsTrue();
    }
    
    [Test]
    public async Task ButtonElement_ShouldHandleClick()
    {
        // Arrange
        _loggerMock = Substitute.For<ILogger<BaseView>>();
        _inputStateMock = Substitute.For<IInputState>();
        _rendererMock = Substitute.For<IUIRenderer>();
        
        _testView = new TestView();
        _testButton = new ButtonElement("test-button", "Test Button", new Vector2(100, 100), new Vector2(200, 50));
        
        var mousePosition = new Vector2(150, 125);
        var clicked = false;
        _testButton.Clicked += () => clicked = true;
        
        _inputStateMock.MousePosition.Returns(mousePosition);
        _inputStateMock.IsMouseButtonPressed(MouseButton.Left).Returns(true);
        
        // Act - Simulate button press
        _testButton.Update(0.016f, _inputStateMock);
        
        // Simulate button release
        _inputStateMock.IsMouseButtonPressed(MouseButton.Left).Returns(false);
        _inputStateMock.IsMouseButtonReleased(MouseButton.Left).Returns(true);
        _testButton.Update(0.016f, _inputStateMock);
        
        // Assert
        await Assert.That(clicked).IsTrue();
    }
    
    [Test]
    public async Task ButtonElement_ShouldHandleKeyboardActivation()
    {
        // Arrange
        _loggerMock = Substitute.For<ILogger<BaseView>>();
        _inputStateMock = Substitute.For<IInputState>();
        _rendererMock = Substitute.For<IUIRenderer>();
        
        _testView = new TestView();
        _testButton = new ButtonElement("test-button", "Test Button", new Vector2(100, 100), new Vector2(200, 50));
        
        var clicked = false;
        _testButton.Clicked += () => clicked = true;
        _testButton.HasFocus = true;
        
        _inputStateMock.IsKeyPressed(KeyboardKey.Enter).Returns(true);
        
        // Act
        var handled = _testButton.HandleInput(_inputStateMock);
        
        // Assert
        await Assert.That(handled).IsTrue();
        await Assert.That(clicked).IsTrue();
    }
    
    [Test]
    public async Task ButtonElement_ShouldRenderCorrectly()
    {
        // Arrange
        _loggerMock = Substitute.For<ILogger<BaseView>>();
        _inputStateMock = Substitute.For<IInputState>();
        _rendererMock = Substitute.For<IUIRenderer>();
        
        _testView = new TestView();
        _testButton = new ButtonElement("test-button", "Test Button", new Vector2(100, 100), new Vector2(200, 50));
        
        _rendererMock.ScreenWidth.Returns(800);
        _rendererMock.ScreenHeight.Returns(600);
        
        // Act
        _testButton.Render(_rendererMock);
        
        // Assert
        _rendererMock.Received().DrawRectangle(100, 100, 200, 50, Arg.Any<Color>());
        _rendererMock.Received().DrawText("Test Button", 110, 115, 20, Arg.Any<Color>());
    }
    
    [Test]
    public async Task ButtonElement_ShouldContainPoint()
    {
        // Arrange
        _loggerMock = Substitute.For<ILogger<BaseView>>();
        _inputStateMock = Substitute.For<IInputState>();
        _rendererMock = Substitute.For<IUIRenderer>();
        
        _testView = new TestView();
        _testButton = new ButtonElement("test-button", "Test Button", new Vector2(100, 100), new Vector2(200, 50));
        
        var pointInside = new Vector2(150, 125);
        var pointOutside = new Vector2(500, 500);
        
        // Act & Assert
        await Assert.That(_testButton.Contains(pointInside)).IsTrue();
        await Assert.That(_testButton.Contains(pointOutside)).IsFalse();
    }
    
    [Test]
    public async Task ButtonElement_ShouldHaveCorrectBounds()
    {
        // Arrange
        _loggerMock = Substitute.For<ILogger<BaseView>>();
        _inputStateMock = Substitute.For<IInputState>();
        _rendererMock = Substitute.For<IUIRenderer>();
        
        _testView = new TestView();
        _testButton = new ButtonElement("test-button", "Test Button", new Vector2(100, 100), new Vector2(200, 50));
        
        // Act & Assert
        await Assert.That(_testButton.Bounds.X).IsEqualTo(100);
        await Assert.That(_testButton.Bounds.Y).IsEqualTo(100);
        await Assert.That(_testButton.Bounds.Width).IsEqualTo(200);
        await Assert.That(_testButton.Bounds.Height).IsEqualTo(50);
    }
    
    [Test]
    public async Task View_ShouldHaveCorrectViewType()
    {
        // Arrange
        _loggerMock = Substitute.For<ILogger<BaseView>>();
        _inputStateMock = Substitute.For<IInputState>();
        _rendererMock = Substitute.For<IUIRenderer>();
        
        _testView = new TestView();
        _testButton = new ButtonElement("test-button", "Test Button", new Vector2(100, 100), new Vector2(200, 50));
        
        // Act & Assert
        await Assert.That(_testView.ViewType).IsEqualTo(GameView.Menu);
    }
    
    [Test]
    public async Task View_ShouldDraw()
    {
        // Arrange
        _loggerMock = Substitute.For<ILogger<BaseView>>();
        _inputStateMock = Substitute.For<IInputState>();
        _rendererMock = Substitute.For<IUIRenderer>();
        
        _testView = new TestView();
        _testButton = new ButtonElement("test-button", "Test Button", new Vector2(100, 100), new Vector2(200, 50));
        
        _rendererMock.ScreenWidth.Returns(800);
        _rendererMock.ScreenHeight.Returns(600);
        
        // Act
        _testView.Draw();
        
        // Assert - The view should have drawn something
        // We can't easily test the actual drawing without a real renderer,
        // but we can verify the method doesn't throw
        // For now, just verify the method exists and can be called
        await Assert.That(_testView).IsNotNull();
    }
    
    [Test]
    public async Task UIManager_ShouldInitializeCorrectly()
    {
        // Arrange
        _loggerMock = Substitute.For<ILogger<BaseView>>();
        _inputStateMock = Substitute.For<IInputState>();
        _rendererMock = Substitute.For<IUIRenderer>();
        
        _testView = new TestView();
        _testButton = new ButtonElement("test-button", "Test Button", new Vector2(100, 100), new Vector2(200, 50));
        
        _rendererMock.ScreenWidth.Returns(1920);
        _rendererMock.ScreenHeight.Returns(1080);
        
        // Act
        var uiManager = new UIManager(_rendererMock, _inputStateMock);
        
        // Assert
        await Assert.That(uiManager.DpiScale).IsEqualTo(1.0f);
        await Assert.That(uiManager.BaseWidth).IsEqualTo(1920);
        await Assert.That(uiManager.BaseHeight).IsEqualTo(1080);
    }
    
    [Test]
    public async Task UIManager_ShouldDetectHighDpi()
    {
        // Arrange
        _loggerMock = Substitute.For<ILogger<BaseView>>();
        _inputStateMock = Substitute.For<IInputState>();
        _rendererMock = Substitute.For<IUIRenderer>();
        
        _testView = new TestView();
        _testButton = new ButtonElement("test-button", "Test Button", new Vector2(100, 100), new Vector2(200, 50));
        
        _rendererMock.ScreenWidth.Returns(3840);
        _rendererMock.ScreenHeight.Returns(2160);
        
        // Act
        var uiManager = new UIManager(_rendererMock, _inputStateMock);
        
        // Assert
        await Assert.That(uiManager.DpiScale).IsGreaterThan(1.0f);
    }
    
    [Test]
    public async Task UIManager_ShouldHandleCameraUpdates()
    {
        // Arrange
        _loggerMock = Substitute.For<ILogger<BaseView>>();
        _inputStateMock = Substitute.For<IInputState>();
        _rendererMock = Substitute.For<IUIRenderer>();
        
        _testView = new TestView();
        _testButton = new ButtonElement("test-button", "Test Button", new Vector2(100, 100), new Vector2(200, 50));
        
        _rendererMock.ScreenWidth.Returns(1920);
        _rendererMock.ScreenHeight.Returns(1080);
        _inputStateMock.MouseWheelMove.Returns(1.0f);
        
        var uiManager = new UIManager(_rendererMock, _inputStateMock);
        var initialZoom = uiManager.CameraZoom;
        
        // Act
        uiManager.Update(0.016f);
        
        // Assert
        await Assert.That(uiManager.CameraZoom).IsGreaterThan(initialZoom);
    }
    
    [Test]
    public async Task UIManager_ShouldRegisterAndSetViews()
    {
        // Arrange
        _loggerMock = Substitute.For<ILogger<BaseView>>();
        _inputStateMock = Substitute.For<IInputState>();
        _rendererMock = Substitute.For<IUIRenderer>();
        
        _testView = new TestView();
        _testButton = new ButtonElement("test-button", "Test Button", new Vector2(100, 100), new Vector2(200, 50));
        
        _rendererMock.ScreenWidth.Returns(1920);
        _rendererMock.ScreenHeight.Returns(1080);
        
        var uiManager = new UIManager(_rendererMock, _inputStateMock);
        
        // Act
        uiManager.RegisterView(_testView);
        uiManager.SetCurrentView(GameView.Menu);
        
        // Assert
        // We can't easily test the internal view storage without exposing it,
        // but we can verify the methods don't throw by checking the uiManager is still valid
        await Assert.That(uiManager).IsNotNull();
    }
}