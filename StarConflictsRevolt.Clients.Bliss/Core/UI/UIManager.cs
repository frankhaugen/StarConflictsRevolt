using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Transformations;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Veldrid;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI;

/// <summary>
/// Manages all UI components including navigation bars and modals.
/// </summary>
public class UIManager
{
    private readonly NavigationBar _navigationBar;
    private readonly ModalDialog _modalDialog;
    private readonly float _screenWidth;
    private readonly float _screenHeight;
    private bool _isNavigationBarVisible = true;
    
    public event Action<string>? ViewRequested;
    
    /// <summary>
    /// Gets the navigation bar.
    /// </summary>
    public NavigationBar NavigationBar => _navigationBar;
    
    /// <summary>
    /// Gets the modal dialog.
    /// </summary>
    public ModalDialog ModalDialog => _modalDialog;
    
    /// <summary>
    /// Gets or sets whether the navigation bar is visible.
    /// </summary>
    public bool IsNavigationBarVisible
    {
        get => _isNavigationBarVisible;
        set
        {
            _isNavigationBarVisible = value;
            _navigationBar.IsVisible = value;
        }
    }
    
    public UIManager(float screenWidth, float screenHeight)
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        
        // Initialize navigation bar
        _navigationBar = new NavigationBar();
        _navigationBar.SetBounds(UILayout.TopRect(_screenWidth, 60f, _screenWidth, 10f));
        _navigationBar.ViewRequested += (viewName) => ViewRequested?.Invoke(viewName);
        
        // Initialize modal dialog
        _modalDialog = new ModalDialog();
        _modalDialog.SetBounds(UILayout.CenteredRect(500f, 350f, _screenWidth, _screenHeight));
        
        // Set up event handlers
        _modalDialog.OnClose += OnModalClosed;
    }
    
    /// <summary>
    /// Updates all UI components.
    /// </summary>
    public void Update(float deltaTime)
    {
        _navigationBar.Update(deltaTime);
        _modalDialog.Update(deltaTime);
    }
    
    /// <summary>
    /// Renders all UI components.
    /// </summary>
    public void Render(ImmediateRenderer immediateRenderer, PrimitiveBatch primitiveBatch, 
                      SpriteBatch spriteBatch, CommandList commandList, Framebuffer framebuffer)
    {
        // Render navigation bar first (background)
        if (_isNavigationBarVisible)
        {
            _navigationBar.Render(immediateRenderer, primitiveBatch, spriteBatch, commandList, framebuffer);
        }
        
        // Render modal dialog last (foreground)
        _modalDialog.Render(immediateRenderer, primitiveBatch, spriteBatch, commandList, framebuffer);
    }
    
    /// <summary>
    /// Handles input for all UI components.
    /// </summary>
    public void HandleInput()
    {
        // Handle modal input first (if modal is open, it gets priority)
        if (_modalDialog.IsVisible)
        {
            _modalDialog.HandleInput();
            return; // Don't process other input when modal is open
        }
        
        // Handle navigation bar input
        if (_isNavigationBarVisible)
        {
            HandleNavigationBarInput();
        }
    }
    
    /// <summary>
    /// Shows a message dialog.
    /// </summary>
    public void ShowMessage(string title, string message, string buttonText = "OK")
    {
        _modalDialog.ShowMessage(title, message, buttonText);
    }
    
    /// <summary>
    /// Shows a confirmation dialog.
    /// </summary>
    public void ShowConfirmation(string title, string message, Action onConfirm, Action? onCancel = null)
    {
        _modalDialog.ShowConfirmation(title, message, onConfirm, onCancel);
    }
    
    /// <summary>
    /// Shows a custom dialog.
    /// </summary>
    public void ShowCustomDialog(string title, string message, params ModalButton[] buttons)
    {
        _modalDialog.ShowCustom(title, message, buttons);
    }
    
    /// <summary>
    /// Selects a navigation item by view name.
    /// </summary>
    public void SelectNavigationItem(string viewName)
    {
        _navigationBar.SelectItem(viewName);
    }
    
    /// <summary>
    /// Adds a navigation item.
    /// </summary>
    public void AddNavigationItem(NavigationItem item)
    {
        _navigationBar.AddItem(item);
    }
    
    /// <summary>
    /// Removes a navigation item.
    /// </summary>
    public void RemoveNavigationItem(string viewName)
    {
        _navigationBar.RemoveItem(viewName);
    }
    
    /// <summary>
    /// Toggles the navigation bar visibility.
    /// </summary>
    public void ToggleNavigationBar()
    {
        IsNavigationBarVisible = !IsNavigationBarVisible;
    }
    
    private void HandleNavigationBarInput()
    {
        // Handle navigation bar keyboard shortcuts
        if (Input.IsKeyPressed(KeyboardKey.Tab))
        {
            _navigationBar.SelectNext();
        }
        else if (Input.IsKeyPressed(KeyboardKey.Tab) && Input.IsKeyDown(KeyboardKey.ShiftLeft))
        {
            _navigationBar.SelectPrevious();
        }
        else if (Input.IsKeyPressed(KeyboardKey.Enter))
        {
            _navigationBar.ActivateSelected();
        }
        else if (Input.IsKeyPressed(KeyboardKey.F1))
        {
            _navigationBar.SelectItem("Main Menu");
            _navigationBar.ActivateSelected();
        }
        else if (Input.IsKeyPressed(KeyboardKey.F2))
        {
            _navigationBar.SelectItem("Galaxy Overview");
            _navigationBar.ActivateSelected();
        }
        else if (Input.IsKeyPressed(KeyboardKey.F3))
        {
            _navigationBar.SelectItem("Tactical Battle");
            _navigationBar.ActivateSelected();
        }
        else if (Input.IsKeyPressed(KeyboardKey.F12))
        {
            ToggleNavigationBar();
        }
    }
    
    private void OnModalClosed()
    {
        // Modal was closed, can resume normal input handling
    }
} 