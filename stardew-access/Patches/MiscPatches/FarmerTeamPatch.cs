using HarmonyLib;
using StardewValley;

namespace stardew_access.Patches;

public class FarmerTeamPatch : IPatch
{
    public void Apply(Harmony harmony)
    {
        harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(FarmerTeam), "OnCalicoStatueEffectAdded"),
            postfix: new HarmonyMethod(typeof(FarmerTeamPatch), nameof(OnCalicoStatueEffectAddedPatch))
        );
    }

    private static void OnCalicoStatueEffectAddedPatch(int key)
    {
        try
        {
            string text = Game1.content.LoadString("Strings\\1_6_Strings:DF_Mine_CalicoStatue_Description_" + key);
            MainClass.ScreenReader.SayWithChecker(text, false);
        }
        catch (Exception e)
        {
            Log.Error($"Error in patch:FarmerTeamPatch->OnCalicoStatueEffectAddedPatch \n{e.Message}\n{e.StackTrace}");
        }
    }
}