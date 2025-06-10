using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Locations;

namespace stardew_access.Patches
{
    internal class DrawAboveAlwaysFrontLayerPatch : IPatch
    {
        public void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.DeclaredMethod(typeof(NPC), "drawAboveAlwaysFrontLayer"),
                postfix: new HarmonyMethod(typeof(DrawAboveAlwaysFrontLayerPatch), nameof(NpcDrawAboveAlwaysFrontLayerPatch))
            );

            harmony.Patch(
                original: AccessTools.DeclaredMethod(typeof(Junimo), "drawAboveAlwaysFrontLayer"),
                postfix: new HarmonyMethod(typeof(DrawAboveAlwaysFrontLayerPatch), nameof(NpcDrawAboveAlwaysFrontLayerPatch))
            );

            harmony.Patch(
                original: AccessTools.DeclaredMethod(typeof(DesertFestival), "drawAboveAlwaysFrontLayer"),
                postfix: new HarmonyMethod(typeof(DrawAboveAlwaysFrontLayerPatch), nameof(DesertFestivalDrawAboveAlwaysFrontLayerPatch))
            );
        }

        private static void NpcDrawAboveAlwaysFrontLayerPatch(object __instance, string ___textAboveHead, int ___textAboveHeadTimer)
        {
            if (!MainClass.Config.AutoReadCharacterBubbles) return;
            try
            {
                if (__instance is not NPC && __instance is not Junimo) return;

                if (___textAboveHeadTimer > 2900 && ___textAboveHead != null)
                {
                    string displayName = (__instance is Junimo junimo)
                        ? junimo.displayName
                        : ((NPC)__instance).displayName;
                    MainClass.ScreenReader.SayWithChecker($"{displayName} says {___textAboveHead}", true);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error in patch:NpcDrawAboveAlwaysFrontLayerPatch \n{e.Message}\n{e.StackTrace}");
            }
        }

        private static void DesertFestivalDrawAboveAlwaysFrontLayerPatch(DesertFestival __instance, float ____raceTextTimer, string ____raceText)
        {
            if (!MainClass.Config.AutoReadCharacterBubbles) return;
            try
            {
                if ((double)____raceTextTimer <= 0.0 || ____raceText == null)
                    return;
                MainClass.ScreenReader.SayWithChecker(____raceText, false);
            }
            catch (Exception e)
            {
                Log.Error($"Error in patch:DesertFestivalDrawAboveAlwaysFrontLayerPatch \n{e.Message}\n{e.StackTrace}");
            }
        }
    }
}