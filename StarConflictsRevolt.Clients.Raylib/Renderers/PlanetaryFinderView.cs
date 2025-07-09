using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Raylib.Renderers;

public class PlanetaryFinderView : IView
{
    private readonly RenderContext _renderContext;
    private readonly GameCommandService _commandService;
    private string _searchText = "";
    private int _selectedPlanetIndex = 0;
    private int _scrollOffset = 0;
    private const int ItemsPerPage = 15;

    public PlanetaryFinderView(RenderContext renderContext, GameCommandService commandService)
    {
        _renderContext = renderContext;
        _commandService = commandService;
    }

    public GameView ViewType => GameView.PlanetaryFinder;

    public void Draw()
    {
        Graphics.ClearBackground(UIHelper.Colors.Background);
        
        // Draw title
        UIHelper.DrawText("Planetary Finder", 400, 20, UIHelper.FontSizes.Large, Color.White, true);
        
        // Draw search input
        UIHelper.DrawText("Search:", 50, 60, UIHelper.FontSizes.Medium, Color.White);
        _searchText = UIHelper.DrawTextInput(_searchText, 50, 90, 300, 30, "Enter planet name...");
        
        // Draw planets list
        DrawPlanetsList();
        
        // Draw action buttons
        DrawActionButtons();
        
        // Draw status bar
        UIHelper.DrawStatusBar(Window.GetScreenHeight() - 30, $"Planets: {GetFilteredPlanets().Count()} | Selected: {_selectedPlanetIndex + 1}");
        
        // Handle keyboard navigation
        HandleKeyboardInput();
    }
    
    private void DrawPlanetsList()
    {
        var planets = GetFilteredPlanets().ToList();
        var startY = 140;
        var itemHeight = 30;
        var panelWidth = Window.GetScreenWidth() - 100;
        var panelHeight = Window.GetScreenHeight() - 250;
        
        // Draw list panel
        UIHelper.DrawPanel(50, startY, panelWidth, panelHeight);
        
        // Draw header
        UIHelper.DrawText("Name", 70, startY + 10, UIHelper.FontSizes.Small, Color.Yellow);
        UIHelper.DrawText("System", 250, startY + 10, UIHelper.FontSizes.Small, Color.Yellow);
        UIHelper.DrawText("Radius", 400, startY + 10, UIHelper.FontSizes.Small, Color.Yellow);
        UIHelper.DrawText("Mass", 500, startY + 10, UIHelper.FontSizes.Small, Color.Yellow);
        UIHelper.DrawText("Distance", 600, startY + 10, UIHelper.FontSizes.Small, Color.Yellow);
        
        // Draw planet items
        var displayStart = _scrollOffset;
        var displayEnd = Math.Min(displayStart + ItemsPerPage, planets.Count);
        
        for (int i = displayStart; i < displayEnd; i++)
        {
            var planet = planets[i];
            var y = startY + 40 + (i - displayStart) * itemHeight;
            var isSelected = i == _selectedPlanetIndex;
            
            // Highlight selected item
            if (isSelected)
            {
                Graphics.DrawRectangle(70, y - 5, panelWidth - 40, itemHeight, UIHelper.Colors.Primary);
            }
            
            // Draw planet info
            UIHelper.DrawText(planet.Name, 70, y, UIHelper.FontSizes.Small, isSelected ? Color.Black : Color.White);
            UIHelper.DrawText(GetPlanetSystem(planet), 250, y, UIHelper.FontSizes.Small, isSelected ? Color.Black : Color.LightGray);
            UIHelper.DrawText($"{planet.Radius:F1}", 400, y, UIHelper.FontSizes.Small, isSelected ? Color.Black : Color.LightGray);
            UIHelper.DrawText($"{planet.Mass:F1}", 500, y, UIHelper.FontSizes.Small, isSelected ? Color.Black : Color.LightGray);
            UIHelper.DrawText($"{planet.DistanceFromSun:F1}", 600, y, UIHelper.FontSizes.Small, isSelected ? Color.Black : Color.LightGray);
        }
        
        // Draw scroll indicators
        if (_scrollOffset > 0)
        {
            UIHelper.DrawText("↑", Window.GetScreenWidth() - 50, startY + 10, UIHelper.FontSizes.Small, Color.Yellow);
        }
        if (displayEnd < planets.Count)
        {
            UIHelper.DrawText("↓", Window.GetScreenWidth() - 50, startY + panelHeight - 30, UIHelper.FontSizes.Small, Color.Yellow);
        }
    }
    
    private void DrawActionButtons()
    {
        var buttonY = Window.GetScreenHeight() - 80;
        var buttonWidth = 120;
        var buttonHeight = 35;
        var spacing = 20;
        var startX = 50;
        
        if (UIHelper.DrawButton("Select Planet", startX, buttonY, buttonWidth, buttonHeight))
        {
            SelectCurrentPlanet();
        }
        
        if (UIHelper.DrawButton("Build Structure", startX + buttonWidth + spacing, buttonY, buttonWidth, buttonHeight))
        {
            ShowBuildStructureDialog();
        }
        
        if (UIHelper.DrawButton("View Details", startX + (buttonWidth + spacing) * 2, buttonY, buttonWidth, buttonHeight))
        {
            ViewPlanetDetails();
        }
        
        if (UIHelper.DrawButton("Back to Menu", startX + (buttonWidth + spacing) * 3, buttonY, buttonWidth, buttonHeight, UIHelper.Colors.Secondary))
        {
            _renderContext.GameState.NavigateTo(GameView.Menu);
        }
    }
    
    private void HandleKeyboardInput()
    {
        var planets = GetFilteredPlanets().ToList();
        
        if (Input.IsKeyPressed(KeyboardKey.Up))
        {
            if (_selectedPlanetIndex > 0)
            {
                _selectedPlanetIndex--;
                if (_selectedPlanetIndex < _scrollOffset)
                {
                    _scrollOffset = Math.Max(0, _scrollOffset - 1);
                }
            }
        }
        
        if (Input.IsKeyPressed(KeyboardKey.Down))
        {
            if (_selectedPlanetIndex < planets.Count - 1)
            {
                _selectedPlanetIndex++;
                if (_selectedPlanetIndex >= _scrollOffset + ItemsPerPage)
                {
                    _scrollOffset = Math.Min(planets.Count - ItemsPerPage, _scrollOffset + 1);
                }
            }
        }
        
        if (Input.IsKeyPressed(KeyboardKey.Enter))
        {
            SelectCurrentPlanet();
        }
        
        if (Input.IsKeyPressed(KeyboardKey.Escape))
        {
            _renderContext.GameState.NavigateTo(GameView.Menu);
        }
    }
    
    private IEnumerable<PlanetDto> GetFilteredPlanets()
    {
        var world = _renderContext.World;
        if (world?.Galaxy?.StarSystems == null) return Enumerable.Empty<PlanetDto>();
        
        var allPlanets = world.Galaxy.StarSystems
            .SelectMany(s => s.Planets ?? Enumerable.Empty<PlanetDto>())
            .ToList();
        
        if (string.IsNullOrWhiteSpace(_searchText))
            return allPlanets;
        
        return allPlanets.Where(p => 
            p.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase));
    }
    
    private string GetPlanetSystem(PlanetDto planet)
    {
        var world = _renderContext.World;
        if (world?.Galaxy?.StarSystems == null) return "Unknown";
        
        var system = world.Galaxy.StarSystems.FirstOrDefault(s => 
            s.Planets?.Any(p => p.Id == planet.Id) == true);
        
        return system?.Name ?? "Unknown";
    }
    
    private void SelectCurrentPlanet()
    {
        var planets = GetFilteredPlanets().ToList();
        if (_selectedPlanetIndex >= 0 && _selectedPlanetIndex < planets.Count)
        {
            _renderContext.GameState.SelectedObject = planets[_selectedPlanetIndex];
            _renderContext.GameState.SetFeedback($"Selected planet: {planets[_selectedPlanetIndex].Name}", TimeSpan.FromSeconds(2));
        }
    }
    
    private void ShowBuildStructureDialog()
    {
        var planets = GetFilteredPlanets().ToList();
        if (_selectedPlanetIndex >= 0 && _selectedPlanetIndex < planets.Count)
        {
            var planet = planets[_selectedPlanetIndex];
            _renderContext.GameState.SetFeedback($"Build structure on {planet.Name} - not implemented yet", TimeSpan.FromSeconds(3));
        }
    }
    
    private void ViewPlanetDetails()
    {
        var planets = GetFilteredPlanets().ToList();
        if (_selectedPlanetIndex >= 0 && _selectedPlanetIndex < planets.Count)
        {
            var planet = planets[_selectedPlanetIndex];
            _renderContext.GameState.SetFeedback($"Viewing details for {planet.Name} - not implemented yet", TimeSpan.FromSeconds(3));
        }
    }
} 