using System.Collections.Generic;
using System.Numerics;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

/// <summary>
/// Concrete implementation of IViewState for managing UI elements.
/// </summary>
public class ViewState : IViewState
{
    private readonly Dictionary<string, IUIElement> _elements = new();
    
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public string? FocusedElementId { get; set; }
    
    public IUIElement? GetElement(string elementId)
    {
        return _elements.TryGetValue(elementId, out var element) ? element : null;
    }
    
    public void AddElement(IUIElement element)
    {
        _elements[element.Id] = element;
    }
    
    public void RemoveElement(string elementId)
    {
        _elements.Remove(elementId);
    }
    
    public void ClearElements()
    {
        _elements.Clear();
        FocusedElementId = null;
    }
    
    /// <summary>
    /// Get all elements in this state.
    /// </summary>
    /// <returns>All UI elements.</returns>
    public IEnumerable<IUIElement> GetAllElements()
    {
        return _elements.Values;
    }
    
    /// <summary>
    /// Find an element at a specific position.
    /// </summary>
    /// <param name="position">Position to search.</param>
    /// <returns>The element at the position or null.</returns>
    public IUIElement? GetElementAt(Vector2 position)
    {
        foreach (var element in _elements.Values)
        {
            if (element.IsVisible && element.IsEnabled && element.Contains(position))
            {
                return element;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Set focus to an element.
    /// </summary>
    /// <param name="elementId">Element ID to focus.</param>
    public void SetFocus(string? elementId)
    {
        // Clear focus from current element
        if (FocusedElementId != null && _elements.TryGetValue(FocusedElementId, out var currentElement))
        {
            currentElement.HasFocus = false;
        }
        
        // Set focus to new element
        FocusedElementId = elementId;
        if (elementId != null && _elements.TryGetValue(elementId, out var newElement))
        {
            newElement.HasFocus = true;
        }
    }
} 