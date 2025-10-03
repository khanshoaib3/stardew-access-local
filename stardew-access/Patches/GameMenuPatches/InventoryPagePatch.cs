using System.Text;
using HarmonyLib;
using stardew_access.Translation;
using stardew_access.Utils;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.Constants;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace stardew_access.Patches;

internal class InventoryPagePatch : IPatch
{
    public void Apply(Harmony harmony)
    {
        harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(InventoryPage), "draw"),
            postfix: new HarmonyMethod(typeof(InventoryPagePatch), nameof(InventoryPagePatch.DrawPatch))
        );
    }

    private static void DrawPatch(InventoryPage __instance)
    {
        try
        {
            int x = Game1.getMouseX(true), y = Game1.getMouseY(true); // Mouse x and y position

            HandleKeyBinds();

            if (NarrateHoveredButton(__instance, x, y))
            {
                return;
            }

            if (NarrateHoveredEquipmentSlot(__instance, x, y))
            {
                return;
            }

            if (InventoryUtils.NarrateHoveredSlot(__instance.inventory, giveExtraDetails: true, isHoveredItemBundleItem: GameMenu.bundleItemHovered))
            {
                return;
            }
        }
        catch (Exception e)
        {
            Log.Error($"An error occurred in inventory page patch:\n{e.Message}\n{e.StackTrace}");
        }
    }

    private static void HandleKeyBinds()
    {
        if (MainClass.Config.MoneyKey.JustPressed())
        {
            SpeakMoneyWithExtras();
            return;
        }

        if (MainClass.Config.HealthNStaminaKey.JustPressed())
        {
            SpeakHealthNStaminaWithBuffs();
            return;
        }
    }

    private static bool NarrateHoveredButton(InventoryPage __instance, int x, int y)
    {
        string? translationKey = null;
        bool isDropItemButton = false;

        if (__instance.inventory.dropItemInvisibleButton != null &&
            __instance.inventory.dropItemInvisibleButton.containsPoint(x, y))
        {
            translationKey = "common-ui-drop_item_button";
            isDropItemButton = true;
        }
        else if (__instance.organizeButton != null && __instance.organizeButton.containsPoint(x, y))
        {
            translationKey = "common-ui-organize_inventory_button";
        }
        else if (__instance.trashCan != null && __instance.trashCan.containsPoint(x, y))
        {
            translationKey = "common-ui-trashcan_button";
        }
        else if (__instance.junimoNoteIcon != null && __instance.junimoNoteIcon.containsPoint(x, y))
        {
            translationKey = "common-ui-community_center_button";
        }
        else
        {
            return false;
        }

        if (!MainClass.ScreenReader.TranslateAndSayWithMenuChecker(translationKey, true)) return true;
        if (isDropItemButton) Game1.playSound("drop_item");

        return true;
    }

    private static bool NarrateHoveredEquipmentSlot(InventoryPage __instance, int mouseX, int mouseY)
    {
        for (int i = 0; i < __instance.equipmentIcons.Count; i++)
        {
            if (!__instance.equipmentIcons[i].containsPoint(mouseX, mouseY))
                continue;

            MainClass.ScreenReader.SayWithMenuChecker(
                GetNameAndDescriptionOfItem(__instance.equipmentIcons[i].name.ToLower().Replace(" ", "_")), true);

            return true;
        }

        return false;
    }

    private static string GetNameAndDescriptionOfItem(string slotName)
    {
        Item? item = slotName switch
        {
            "hat" => Game1.player.hat.Value,
            "left_ring" => Game1.player.leftRing.Value,
            "right_ring" => Game1.player.rightRing.Value,
            "boots" => Game1.player.boots.Value,
            "shirt" => Game1.player.shirtItem.Value,
            "pants" => Game1.player.pantsItem.Value,
            _ => null
        };

        string description = (item != null)
            ? (item is Boots boots)
                ? boots.description +
                  (boots.defenseBonus.Value > 0 ? "\n" + Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", boots.defenseBonus.Value) : "") +
                  (boots.immunityBonus.Value > 0 ? "\n" + Game1.content.LoadString("Strings\\UI:ItemHover_ImmunityBonus", boots.immunityBonus.Value) : "")
                : item.getDescription()
            : "";

        return Translator.Instance.Translate("common-ui-equipment_slots", new
        {
            slot_name = slotName,
            is_empty = (item == null) ? 1 : 0,
            item_name = (item == null) ? "" : item.DisplayName,
            item_description = (item == null) ? "" : description
        }, TranslationCategory.Menu);
    }

    internal static void SpeakMoneyWithExtras()
    {
        string farmName = Game1.content.LoadString("Strings\\UI:Inventory_FarmName", Game1.player.farmName.Value);
        string currentFunds = Game1.content.LoadString(
            "Strings\\UI:Inventory_CurrentFunds" + (Game1.player.useSeparateWallets ? "_Separate" : ""),
            Utility.getNumberWithCommas(Game1.player.Money)
        );
        string totalEarnings = Game1.content.LoadString(
            "Strings\\UI:Inventory_TotalEarnings" + (Game1.player.useSeparateWallets ? "_Separate" : ""),
            Utility.getNumberWithCommas((int)Game1.player.totalMoneyEarned)
        );
        string dateInfo = Utility.getDateString();
        int festivalScore = Game1.CurrentEvent?.playerControlSequenceID != null
                            && Game1.CurrentEvent.playerControlSequenceID is "eggHunt" or "fair" or "iceFishing"
            ? Game1.player.festivalScore
            : -1;
        string festivalType = Game1.CurrentEvent?.playerControlSequenceID != null
            ? Game1.CurrentEvent.playerControlSequenceID switch
            {
                "eggHunt" => "EggHunt",
                "fair" => "StardewFair",
                "iceFishing" => "FestivalOfIce",
                _ => Game1.CurrentEvent.playerControlSequenceID
            }
            : "null";
        int walnut = Game1.player.currentLocation is IslandLocation ? Game1.netWorldState.Value.GoldenWalnuts : -1;
        int qiGems = Game1.player.currentLocation?.Name == "QiNutRoom" ? Game1.player.QiGems : -1;
        int qiCoins = Game1.player.currentLocation is Club ? Game1.player.clubCoins : -1;
        bool isDesertFestival = Utility.GetDayOfPassiveFestival("DesertFestival") > 0
                                && ((Game1.player.currentLocation is MineShaft && Game1.mine.getMineArea() == 121)
                                    || Game1.player.currentLocation is DesertFestival);
        int calicoEggCount = isDesertFestival ? Game1.player.Items.CountId("CalicoEgg") : -1;
        int calicoEggRating = isDesertFestival ? Game1.player.team.highestCalicoEggRatingToday.Value + 1 : -1;
        int squidFestCount = (Game1.player.currentLocation is Beach && Game1.IsWinter && Game1.dayOfMonth >= 12 &&
                              Game1.dayOfMonth <= 13)
            ? (int)Game1.stats.Get(StatKeys.SquidFestScore(Game1.dayOfMonth, Game1.year))
            : -1;

        MainClass.ScreenReader.TranslateAndSay("menu-inventory_page-money_info_key", true, new
        {
            farm_name = farmName,
            current_funds = currentFunds,
            total_earnings = totalEarnings,
            date_info = dateInfo,
            festival_type = festivalType,
            festival_score = festivalScore,
            calico_egg_count = calicoEggCount,
            calico_egg_rating = calicoEggRating,
            squid_fest_count = squidFestCount,
            golden_walnut_count = walnut,
            qi_gem_count = qiGems,
            qi_club_coins = qiCoins
        }, TranslationCategory.Menu);
    }

    private static void SpeakHealthNStaminaWithBuffs()
    {
        int health = CurrentPlayer.CurrentHealth;
        int stamina = CurrentPlayer.CurrentStamina;
        string buffs = Game1.player.buffs.AppliedBuffs.Values.Reverse().Join(e => Translator.Instance.Translate(
            "menu-inventory_page-buff_info", new
            {
                name = string.IsNullOrWhiteSpace(e.displayName) ? "null" : e.displayName,
                effects = getDescription(e) ?? "null",
                time_left = e.millisecondsDuration <= 0
                    ? "null"
                    : $"{(e.millisecondsDuration / 60000).ToString()}:{(e.millisecondsDuration % 60000 / 10000).ToString()}{(e.millisecondsDuration % 60000 % 10000 / 1000).ToString()}"
            }, TranslationCategory.Menu)
        );
        buffs = buffs.Trim();

        MainClass.ScreenReader.TranslateAndSay("menu-inventory_page-health_n_buff_info_key", true,
            new { health, stamina, buffs = string.IsNullOrWhiteSpace(buffs) ? "null" : buffs }, TranslationCategory.Menu);
    }

    // Method copied from BuffsDisplay
    private static string? getDescription(Buff buff)
    {
        StringBuilder stringBuilder = new StringBuilder();
        
        string description1 = buff.description;
        if ((description1 != null ? (description1.Length > 1 ? 1 : 0) : 0) != 0)
            stringBuilder.Append(buff.description + " ");
        foreach (BuffAttributeDisplay displayAttribute in BuffsDisplay.displayAttributes)
        {
            string? description2 = getDescription(buff, displayAttribute);
            if (description2 != null) stringBuilder.Append(description2 + " ");
        }

        string? sourceLine = getSourceLine(buff);
        if (sourceLine != null) stringBuilder.Append(sourceLine + " ");
        if (string.IsNullOrWhiteSpace(stringBuilder.ToString())) return null;
        return stringBuilder.ToString().TrimEnd();
    }

    // Method copied from BuffsDisplay
    private static string? getDescription(Buff buff, BuffAttributeDisplay attribute)
    {
        float num = attribute.Value(buff);
        if (num == 0.0) return null;
        return attribute.Description(num);
    }

    // Method copied from BuffsDisplay
    private static string? getSourceLine(Buff buff)
    {
        string str = buff.displaySource ?? buff.source;
        return string.IsNullOrWhiteSpace(str)
            ? null
            : Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.508") + str;
    }
}
