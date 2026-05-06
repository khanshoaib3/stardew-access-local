using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using stardew_access.Translation;
using StardewValley.Menus;

namespace stardew_access.Patches;

internal class TextEntryMenuPatch : IPatch
{
    public void Apply(Harmony harmony)
    {
        harmony.Patch(
            original: AccessTools.Constructor(typeof(TextEntryMenu), [typeof(TextBox)]),
            postfix: new HarmonyMethod(typeof(TextEntryMenuPatch), nameof(ConstructorPatch))
        );
        
        harmony.Patch(
            original: AccessTools.Method(typeof(TextEntryMenu), "ShowKeyboard"),
            prefix: new HarmonyMethod(typeof(TextEntryMenuPatch), nameof(ShowKeyboardPatch))
        );

        harmony.Patch(
            original: AccessTools.Method(typeof(TextEntryMenu), nameof(TextEntryMenu.draw), [typeof(SpriteBatch)]),
            prefix: new HarmonyMethod(typeof(TextEntryMenuPatch), nameof(DrawPatch))
        );

        harmony.Patch(
            original: AccessTools.Method(typeof(TextEntryMenu), nameof(TextEntryMenu.Close)),
            prefix: new HarmonyMethod(typeof(TextEntryMenuPatch), nameof(ClosePatch))
        );
    }

    internal static void ConstructorPatch(TextEntryMenu __instance)
    {
        __instance.backspaceButton.ScreenReaderText = Translator.Instance.Translate("menu-text_entry-backspace", TranslationCategory.Menu);
        __instance.spaceButton.ScreenReaderText = Translator.Instance.Translate("menu-text_entry-space", TranslationCategory.Menu);
        __instance.okButton.ScreenReaderText = Translator.Instance.Translate("menu-text_entry-submit", TranslationCategory.Menu);
        __instance.upperCaseButton.ScreenReaderText = Translator.Instance.Translate("menu-text_entry-upper_case_toggle", TranslationCategory.Menu);
        __instance.symbolsButton.ScreenReaderText = Translator.Instance.Translate("menu-text_entry-symbols_toggle", TranslationCategory.Menu);
    }
    
    internal static void ShowKeyboardPatch(TextEntryMenu __instance, int index)
    {
        try
        {
            int index1 = 0;
            foreach (string str in __instance.letterMaps[index])
            {
                foreach (char ch in str)
                {
                    __instance.keys[index1].ScreenReaderText = ch.ToString() ?? "Unknown";
                    ++index1;
                }
            }
        }
        catch (Exception e)
        {
            Log.Error($"An error occured in ConstructorPatch() in TextEntryPatch:\n{e.Message}\n{e.StackTrace}");
        }
    }

    internal static void DrawPatch(TextEntryMenu __instance, TextBox ____target)
    {
        try
        {
            TextBoxPatch.DrawPatch(____target);
        }
        catch (Exception e)
        {
            Log.Error($"An error occured in DrawPatch() in TextEntryPatch:\n{e.Message}\n{e.StackTrace}");
        }
    }

    internal static void ClosePatch(TextBox ____target)
    {
        TextBoxPatch.activeTextBoxes = "";
        ____target.Selected = false;
    }
}