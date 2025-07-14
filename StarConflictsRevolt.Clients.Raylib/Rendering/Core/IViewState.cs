using System.Numerics;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

/// <summary>
/// Represents the state of a view that can be tested independently of rendering.
/// </summary>
public interface IViewState
{
    /// <summary>
    /// Whether the view is visible.
    /// </summary>
    bool IsVisible { get; set; }
    
    /// <summary>
    /// Whether the view is enabled for interaction.
    /// </summary>
    bool IsEnabled { get; set; }
    
    /// <summary>
    /// Current focus element ID.
    /// </summary>
    string? FocusedElementId { get; set; }
    
    /// <summary>
    /// Get a UI element by its ID.
    /// </summary>
    /// <param name="elementId">Element identifier.</param>
    /// <returns>The UI element or null if not found.</returns>
    IUIElement? GetElement(string elementId);
    
    /// <summary>
    /// Add a UI element to the state.
    /// </summary>
    /// <param name="element">Element to add.</param>
    void AddElement(IUIElement element);
    
    /// <summary>
    /// Remove a UI element from the state.
    /// </summary>
    /// <param name="elementId">ID of element to remove.</param>
    void RemoveElement(string elementId);
    
    /// <summary>
    /// Clear all elements from the state.
    /// </summary>
    void ClearElements();
    
    /// <summary>
    /// Get all elements in this state.
    /// </summary>
    /// <returns>All UI elements.</returns>
    IEnumerable<IUIElement> GetAllElements();
    
    /// <summary>
    /// Find an element at a specific position.
    /// </summary>
    /// <param name="position">Position to search.</param>
    /// <returns>The element at the position or null.</returns>
    IUIElement? GetElementAt(Vector2 position);
    
    /// <summary>
    /// Set focus to an element.
    /// </summary>
    /// <param name="elementId">Element ID to focus.</param>
    void SetFocus(string? elementId);
} 