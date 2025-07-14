using System.Numerics;
using Raylib_CSharp;
using Raylib_CSharp.Camera.Cam2D;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.UI;

public static class RayLibHelper
{
    public static Vector2 GetScreenToWorld2D(Vector2 getMousePosition, Camera2D camera)
    {
        var screenToWorld = RayMath.Vector2Subtract(getMousePosition, camera.Offset);
        screenToWorld = RayMath.Vector2Scale(screenToWorld, 1.0f / camera.Zoom);
        screenToWorld = RayMath.Vector2Add(screenToWorld, camera.Target);
        return screenToWorld;
    }
}