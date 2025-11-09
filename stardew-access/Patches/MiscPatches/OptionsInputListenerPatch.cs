using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley.Menus;

namespace stardew_access.Patches;

public class OptionsInputListenerPatch : IPatch
{
    private static DateTime? _listeningStartTime;
    
    public void Apply(Harmony harmony)
    {
        harmony.Patch(
            original: AccessTools.Method(typeof(OptionsInputListener), "receiveLeftClick"),
            prefix: new HarmonyMethod(GetType(), nameof(ReceiveLeftClickPatch))
        );
        
        harmony.Patch(
            original: AccessTools.Method(typeof(OptionsInputListener), "receiveKeyPress"),
            prefix: new HarmonyMethod(GetType(), nameof(ReceiveKeyPressPatch))
        );
    }

    public static void ReceiveLeftClickPatch(OptionsInputListener __instance, bool ___listening, Rectangle ___setbuttonBounds, int x, int y)
    {
        if (__instance.greyedOut || ___listening || !___setbuttonBounds.Contains(x, y)) return;

        if (__instance.whichOption != -1)
        {
            _listeningStartTime = DateTime.Now;
        }
    }

    public static bool ReceiveKeyPressPatch(Keys key)
    {
        if (_listeningStartTime != null && DateTime.Now - _listeningStartTime <= TimeSpan.FromMilliseconds(10))
        {
            _listeningStartTime = null;
            return false;
        }

        return true;
    }
}