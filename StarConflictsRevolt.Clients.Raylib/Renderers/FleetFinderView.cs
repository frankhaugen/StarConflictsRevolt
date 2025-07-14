using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib.Services;

namespace StarConflictsRevolt.Clients.Raylib.Renderers;

public class FleetFinderView : IView
{
    private const int ItemsPerPage = 15;
    private readonly GameCommandService _commandService;
    private readonly RenderContext _renderContext;
    private int _scrollOffset;
    private string _searchText = "";
    private int _selectedFleetIndex;

    public FleetFinderView(RenderContext renderContext, GameCommandService commandService)
    {
        _renderContext = renderContext;
        _commandService = commandService;
    }

    public GameView ViewType => GameView.FleetFinder;

    public void Draw()
    {
        Graphics.ClearBackground(UIHelper.Colors.Background);

        // Draw title
        UIHelper.DrawText("Fleet Finder", 400, 20, UIHelper.FontSizes.Large, Color.White, true);

        // Draw search input
        UIHelper.DrawText("Search:", 50, 60, UIHelper.FontSizes.Medium, Color.White);
        _searchText = UIHelper.DrawTextInput(_searchText, 50, 90, 300, 30, "Enter fleet name...");

        // Draw fleets list
        DrawFleetsList();

        // Draw action buttons
        DrawActionButtons();

        // Draw status bar
        UIHelper.DrawStatusBar(Window.GetScreenHeight() - 30, $"Fleets: {GetFilteredFleets().Count()} | Selected: {_selectedFleetIndex + 1} | ESC/Backspace: Menu");

        // Handle keyboard navigation
        HandleKeyboardInput();
    }

    private void DrawFleetsList()
    {
        var fleets = GetFilteredFleets().ToList();
        var startY = 140;
        var itemHeight = 30;
        var panelWidth = Window.GetScreenWidth() - 100;
        var panelHeight = Window.GetScreenHeight() - 250;

        // Draw list panel
        UIHelper.DrawPanel(50, startY, panelWidth, panelHeight);

        // Draw header
        UIHelper.DrawText("Name", 70, startY + 10, UIHelper.FontSizes.Small, Color.Yellow);
        UIHelper.DrawText("Location", 250, startY + 10, UIHelper.FontSizes.Small, Color.Yellow);
        UIHelper.DrawText("Ships", 400, startY + 10, UIHelper.FontSizes.Small, Color.Yellow);
        UIHelper.DrawText("Status", 500, startY + 10, UIHelper.FontSizes.Small, Color.Yellow);

        // Draw fleet items
        var displayStart = _scrollOffset;
        var displayEnd = Math.Min(displayStart + ItemsPerPage, fleets.Count);

        for (var i = displayStart; i < displayEnd; i++)
        {
            var fleet = fleets[i];
            var y = startY + 40 + (i - displayStart) * itemHeight;
            var isSelected = i == _selectedFleetIndex;

            // Highlight selected item
            if (isSelected) Graphics.DrawRectangle(70, y - 5, panelWidth - 40, itemHeight, UIHelper.Colors.Primary);

            // Draw fleet info
            UIHelper.DrawText(fleet.Name ?? "Unknown Fleet", 70, y, UIHelper.FontSizes.Small, isSelected ? Color.Black : Color.White);
            UIHelper.DrawText(GetFleetLocation(fleet), 250, y, UIHelper.FontSizes.Small, isSelected ? Color.Black : Color.LightGray);
            UIHelper.DrawText(fleet.Ships?.Count().ToString() ?? "0", 400, y, UIHelper.FontSizes.Small, isSelected ? Color.Black : Color.LightGray);
            UIHelper.DrawText(GetFleetStatus(fleet), 500, y, UIHelper.FontSizes.Small, isSelected ? Color.Black : Color.LightGray);
        }

        // Draw scroll indicators
        if (_scrollOffset > 0) UIHelper.DrawText("↑", Window.GetScreenWidth() - 50, startY + 10, UIHelper.FontSizes.Small, Color.Yellow);
        if (displayEnd < fleets.Count) UIHelper.DrawText("↓", Window.GetScreenWidth() - 50, startY + panelHeight - 30, UIHelper.FontSizes.Small, Color.Yellow);
    }

    private void DrawActionButtons()
    {
        var buttonY = Window.GetScreenHeight() - 80;
        var buttonWidth = 120;
        var buttonHeight = 35;
        var spacing = 20;
        var startX = 50;

        if (UIHelper.DrawButton("Select Fleet", startX, buttonY, buttonWidth, buttonHeight)) SelectCurrentFleet();

        if (UIHelper.DrawButton("Move Fleet", startX + buttonWidth + spacing, buttonY, buttonWidth, buttonHeight)) ShowMoveFleetDialog();

        if (UIHelper.DrawButton("View Details", startX + (buttonWidth + spacing) * 2, buttonY, buttonWidth, buttonHeight)) ViewFleetDetails();

        if (UIHelper.DrawButton("Back to Menu", startX + (buttonWidth + spacing) * 3, buttonY, buttonWidth, buttonHeight, UIHelper.Colors.Secondary)) _renderContext.GameState.NavigateTo(GameView.Menu);
    }

    private void HandleKeyboardInput()
    {
        var fleets = GetFilteredFleets().ToList();

        if (Input.IsKeyPressed(KeyboardKey.Up))
            if (_selectedFleetIndex > 0)
            {
                _selectedFleetIndex--;
                if (_selectedFleetIndex < _scrollOffset) _scrollOffset = Math.Max(0, _scrollOffset - 1);
            }

        if (Input.IsKeyPressed(KeyboardKey.Down))
            if (_selectedFleetIndex < fleets.Count - 1)
            {
                _selectedFleetIndex++;
                if (_selectedFleetIndex >= _scrollOffset + ItemsPerPage) _scrollOffset = Math.Min(fleets.Count - ItemsPerPage, _scrollOffset + 1);
            }

        if (Input.IsKeyPressed(KeyboardKey.Enter)) SelectCurrentFleet();

        if (Input.IsKeyPressed(KeyboardKey.Escape)) _renderContext.GameState.NavigateTo(GameView.Menu);

        if (Input.IsKeyPressed(KeyboardKey.Backspace)) _renderContext.GameState.NavigateTo(GameView.Menu);
    }

    private IEnumerable<dynamic> GetFilteredFleets()
    {
        var world = _renderContext.World;
        if (world?.Galaxy?.StarSystems == null) return Enumerable.Empty<dynamic>();

        // For now, create placeholder fleet data since PlanetDto doesn't have fleets
        var allFleets = world.Galaxy.StarSystems
            .SelectMany(s => s.Planets ?? Enumerable.Empty<PlanetDto>())
            .SelectMany(p => CreatePlaceholderFleets(p))
            .ToList();

        if (string.IsNullOrWhiteSpace(_searchText))
            return allFleets;

        return allFleets.Where(f =>
            f.Name?.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private IEnumerable<dynamic> CreatePlaceholderFleets(PlanetDto planet)
    {
        // Create placeholder fleet data for demonstration
        return new[]
        {
            new { Id = Guid.NewGuid(), Name = $"Fleet Alpha at {planet.Name}", Location = planet.Name, Ships = 5, Status = "Idle" },
            new { Id = Guid.NewGuid(), Name = $"Fleet Beta at {planet.Name}", Location = planet.Name, Ships = 3, Status = "Patrolling" }
        };
    }

    private string GetFleetLocation(dynamic fleet)
    {
        return fleet.Location ?? "Unknown Location";
    }

    private string GetFleetStatus(dynamic fleet)
    {
        return fleet.Status ?? "Unknown";
    }

    private void SelectCurrentFleet()
    {
        var fleets = GetFilteredFleets().ToList();
        if (_selectedFleetIndex >= 0 && _selectedFleetIndex < fleets.Count)
        {
            _renderContext.GameState.SelectedObject = fleets[_selectedFleetIndex];
            _renderContext.GameState.SetFeedback($"Selected fleet: {fleets[_selectedFleetIndex].Name}", TimeSpan.FromSeconds(2));
        }
    }

    private void ShowMoveFleetDialog()
    {
        var fleets = GetFilteredFleets().ToList();
        if (_selectedFleetIndex >= 0 && _selectedFleetIndex < fleets.Count) _renderContext.GameState.SetFeedback("Move fleet dialog not implemented yet", TimeSpan.FromSeconds(3));
    }

    private void ViewFleetDetails()
    {
        var fleets = GetFilteredFleets().ToList();
        if (_selectedFleetIndex >= 0 && _selectedFleetIndex < fleets.Count) _renderContext.GameState.SetFeedback("Fleet details not implemented yet", TimeSpan.FromSeconds(3));
    }
}