using RectangleF = Bliss.CSharp.Transformations.RectangleF;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI;

/// <summary>
/// Utility class for UI layout calculations and positioning.
/// </summary>
public static class UILayout
{
    /// <summary>
    /// Centers a rectangle within the screen bounds.
    /// </summary>
    public static RectangleF Center(RectangleF rect, float screenWidth, float screenHeight)
    {
        var x = (screenWidth - rect.Width) / 2f;
        var y = (screenHeight - rect.Height) / 2f;
        return new RectangleF(x, y, rect.Width, rect.Height);
    }
    
    /// <summary>
    /// Centers a rectangle horizontally within the screen bounds.
    /// </summary>
    public static RectangleF CenterHorizontal(RectangleF rect, float screenWidth)
    {
        var x = (screenWidth - rect.Width) / 2f;
        return new RectangleF(x, rect.Y, rect.Width, rect.Height);
    }
    
    /// <summary>
    /// Centers a rectangle vertically within the screen bounds.
    /// </summary>
    public static RectangleF CenterVertical(RectangleF rect, float screenHeight)
    {
        var y = (screenHeight - rect.Height) / 2f;
        return new RectangleF(rect.X, y, rect.Width, rect.Height);
    }
    
    /// <summary>
    /// Creates a rectangle with specified dimensions centered on screen.
    /// </summary>
    public static RectangleF CenteredRect(float width, float height, float screenWidth, float screenHeight)
    {
        var x = (screenWidth - width) / 2f;
        var y = (screenHeight - height) / 2f;
        return new RectangleF(x, y, width, height);
    }
    
    /// <summary>
    /// Creates a rectangle positioned at the top of the screen with specified dimensions.
    /// </summary>
    public static RectangleF TopRect(float width, float height, float screenWidth, float yOffset = 0f)
    {
        var x = (screenWidth - width) / 2f;
        return new RectangleF(x, yOffset, width, height);
    }
    
    /// <summary>
    /// Creates a rectangle positioned at the bottom of the screen with specified dimensions.
    /// </summary>
    public static RectangleF BottomRect(float width, float height, float screenWidth, float screenHeight, float yOffset = 0f)
    {
        var x = (screenWidth - width) / 2f;
        var y = screenHeight - height - yOffset;
        return new RectangleF(x, y, width, height);
    }
    
    /// <summary>
    /// Creates a rectangle positioned at the left side of the screen with specified dimensions.
    /// </summary>
    public static RectangleF LeftRect(float width, float height, float screenHeight, float xOffset = 0f)
    {
        var y = (screenHeight - height) / 2f;
        return new RectangleF(xOffset, y, width, height);
    }
    
    /// <summary>
    /// Creates a rectangle positioned at the right side of the screen with specified dimensions.
    /// </summary>
    public static RectangleF RightRect(float width, float height, float screenWidth, float screenHeight, float xOffset = 0f)
    {
        var x = screenWidth - width - xOffset;
        var y = (screenHeight - height) / 2f;
        return new RectangleF(x, y, width, height);
    }
    
    /// <summary>
    /// Calculates the position to center text within a rectangle.
    /// </summary>
    public static Vector2 CenterTextPosition(RectangleF container, float textWidth, float textHeight)
    {
        var x = container.X + (container.Width - textWidth) / 2f;
        var y = container.Y + (container.Height - textHeight) / 2f;
        return new Vector2(x, y);
    }
    
    /// <summary>
    /// Creates a grid layout with specified number of columns and rows.
    /// </summary>
    public static RectangleF[,] CreateGrid(float containerX, float containerY, float containerWidth, float containerHeight, 
                                         int columns, int rows, float spacing = 10f)
    {
        var cellWidth = (containerWidth - (columns - 1) * spacing) / columns;
        var cellHeight = (containerHeight - (rows - 1) * spacing) / rows;
        
        var grid = new RectangleF[columns, rows];
        
        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                var x = containerX + col * (cellWidth + spacing);
                var y = containerY + row * (cellHeight + spacing);
                grid[col, row] = new RectangleF(x, y, cellWidth, cellHeight);
            }
        }
        
        return grid;
    }
} 