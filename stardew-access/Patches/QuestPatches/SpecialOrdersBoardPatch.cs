using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using stardew_access.Translation;
using StardewValley;
using StardewValley.Menus;
using StardewValley.SpecialOrders;

namespace stardew_access.Patches;

internal class SpecialOrdersBoardPatch : IPatch
{
    public void Apply(Harmony harmony)
    {
        harmony.Patch(
            original: AccessTools.Method(typeof(SpecialOrdersBoard), nameof(SpecialOrdersBoard.draw), [typeof(SpriteBatch)]),
            postfix: new HarmonyMethod(typeof(SpecialOrdersBoardPatch), nameof(DrawPatch))
        );
    }

    private static void DrawPatch(SpecialOrdersBoard __instance)
    {
        try
        {
            int x = Game1.getMouseX(true), y = Game1.getMouseY(true); // Mouse x and y position

            if (__instance.acceptLeftQuestButton.visible && __instance.acceptLeftQuestButton.containsPoint(x, y))
            {
                MainClass.ScreenReader.TranslateAndSayWithMenuChecker("menu-special_orders_board-accept_button", true, new
                {
                    is_left_quest = 1,
                    quest_details = GetQuestDetails(__instance.leftOrder, Game1.player.team.completedSpecialOrders.Contains(__instance.leftOrder.questKey.Value))
                });
                return;
            }

            if (__instance.acceptRightQuestButton.visible && __instance.acceptRightQuestButton.containsPoint(x, y))
            {
                MainClass.ScreenReader.TranslateAndSayWithMenuChecker("menu-special_orders_board-accept_button", true, new
                {
                    is_left_quest = 0,
                    quest_details = GetQuestDetails(__instance.rightOrder, Game1.player.team.completedSpecialOrders.Contains(__instance.rightOrder.questKey.Value))
                });
                return;
            }

            SpecialOrder? inProgress = IsInProgress(__instance.leftOrder, __instance)
                ? __instance.leftOrder
                : IsInProgress(__instance.rightOrder, __instance)
                    ? __instance.rightOrder
                    : null;
            

            if (inProgress is null)
            {
                MainClass.ScreenReader.TranslateAndSayWithMenuChecker("menu-special_orders_board-no_active_quest", true);
                return;
            }

            if (Game1.player.team.completedSpecialOrders.Contains(inProgress.questKey.Value))
            {
                if (inProgress.questState.Value == SpecialOrderStatus.Complete)
                {
                    MainClass.ScreenReader.TranslateAndSayWithMenuChecker("menu-special_orders_board-quest_completed", true, new
                    {
                        name = inProgress.GetName()
                    });
                }
                else
                {
                    MainClass.ScreenReader.TranslateAndSayWithMenuChecker("menu-special_orders_board-quest_in_progress", true, new
                    {
                        quest_details = GetQuestDetails(inProgress, true)
                    });
                }

                return;
            }

            MainClass.ScreenReader.TranslateAndSayWithMenuChecker("menu-special_orders_board-quest_in_progress", true, new
            {
                quest_details = GetQuestDetails(inProgress, false)
            });
        }
        catch (Exception e)
        {
            Log.Error($"An error occurred in special orders board patch:\n{e.Message}\n{e.StackTrace}");
        }
    }

    private static string GetQuestDetails(SpecialOrder order, bool previouslyCompleted) => Translator.Instance.Translate(
        "menu-special_orders_board-quest_details", new
        {
            name = order.GetName(),
            previously_completed = previouslyCompleted ? 1 : 0,
            description = order.GetDescription(),
            objectives_list = string.Join(", ", order.GetObjectiveDescriptions()),
            is_timed = order.IsTimedQuest() ? 1 : 0,
            days = order.GetDaysLeft(),
            has_money_reward = order.HasMoneyReward() ? 1 : 0,
            money = order.GetMoneyReward()
        },
        TranslationCategory.Menu);

    private static bool IsInProgress(SpecialOrder order, SpecialOrdersBoard __instance)
    {
        // Ref: SpecialOrdersBoard::draw()
        bool flag1 = false;
        bool flag2 = false;
        foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
        {
            if (specialOrder.questState.Value == SpecialOrderStatus.InProgress)
            {
                foreach (SpecialOrder availableSpecialOrder in Game1.player.team.availableSpecialOrders)
                {
                    if (!(availableSpecialOrder.orderType.Value != __instance.GetOrderType()) &&
                        specialOrder.questKey.Value == availableSpecialOrder.questKey.Value)
                    {
                        if (order.questKey != specialOrder.questKey)
                            flag1 = true;
                        flag2 = true;
                        break;
                    }
                }

                if (flag2)
                    break;
            }
        }

        if (!flag2 && Game1.player.team.acceptedSpecialOrderTypes.Contains(__instance.GetOrderType()))
            flag1 = true;

        return !flag1;
    }
}
