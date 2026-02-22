using stardew_access.Patches;
using StardewModdingAPI;
using StardewValley;

namespace stardew_access.Utils;

internal static class MouseUtils
{
    private static bool wasRecentlyClicked = false;

    internal static async void SimulateMouseClicksWithDelay(Action<int, int>? leftClickHandler, Action<int, int>? rightClickHandler)
    {
        if (!wasRecentlyClicked && SimulateMouseClicks(leftClickHandler, rightClickHandler))
        {
            wasRecentlyClicked = true;
            await Task.Delay(300);
            wasRecentlyClicked = false;
        }
    }

    internal static bool SimulateMouseClicks(Action<int, int>? leftClickHandler, Action<int, int>? rightClickHandler)
    {
        int mouseX = Game1.getMouseX(true);
        int mouseY = Game1.getMouseY(true);

        if (leftClickHandler != null)
        {
#if DEBUG
            Log.Debug($"Simulating left mouse click at {mouseX}x {mouseY}y in menu {IClickableMenuPatch.ActiveMenuOrSubMenu}");
#endif
            if (MainClass.Config.LeftClickMainKey.JustPressed())
            {
                MainClass.ModHelper!.Input.Press(SButton.MouseLeft);
                return true;
            }
            if (MainClass.Config.LeftClickAlternateKey.JustPressed())
            {
                leftClickHandler(mouseX, mouseY);
                return true;
            }
        }
        
        if (rightClickHandler != null)
        {
#if DEBUG
            Log.Debug($"Simulating right mouse click at {mouseX}x {mouseY}y");
#endif
            if (MainClass.Config.RightClickMainKey.JustPressed())
            {
                MainClass.ModHelper!.Input.Press(SButton.MouseRight);
                return true;
            }

            if (MainClass.Config.RightClickAlternateKey.JustPressed())
            {
                rightClickHandler(mouseX, mouseY);
                return true;
            }
        }

        return false;
    }
}
