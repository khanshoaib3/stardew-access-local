using System.Reflection;
using stardew_access.Translation;
using StardewValley;
using StardewValley.Menus;

namespace stardew_access.Utils;

public static class OptionsElementUtils
{
    public static bool NarrateOptionSlotsInMenuUsingReflection(IClickableMenu? menu, bool allowFallback = false)
    {
        if (menu is null) return false;

        var fields = menu.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

        var optionElementsLists = fields.Where(fi => fi.FieldType == typeof(List<OptionsElement>))
                                        .Select(f => f.GetValue(menu)).Cast<List<OptionsElement>>();

        if (optionElementsLists.Count() == 0)
        {
            return false;
        }

        var optionSlotsEnumerable = fields.Where(fi => fi.FieldType == typeof(List<ClickableComponent>) && fi.Name.Equals("optionSlots", StringComparison.OrdinalIgnoreCase));
        if (optionSlotsEnumerable.Count() == 0)
        {
#if DEBUG
            Log.Debug($"[OptionsElementUtils] ({menu.GetType().FullName}) A field with `List<OptionsElement>` found but not a field with name `optionSlots` and type `List<ClickableComponent>`.", once: true);
#endif
            return false;
        }

        var optionSlots = optionSlotsEnumerable.Select(f => f.GetValue(menu))
                                .Cast<List<ClickableComponent>>()
                                .First();

        var currentItemIndexEnumerable = fields.Where(fi => fi.FieldType == typeof(int) && fi.Name.Equals("currentItemIndex", StringComparison.OrdinalIgnoreCase));
        if (currentItemIndexEnumerable.Count() == 0)
        {
#if DEBUG
            Log.Debug($"[OptionsElementUtils] ({menu.GetType().FullName}) A field with `List<OptionsElement>` found and a field with name `optionSlots` and type `List<ClickableComponent>`, but not a field with name `currentItemIndex` and type `int`.", once: true);
#endif
            return false;
        }

        int currentItemIndex = currentItemIndexEnumerable.Select(f => f.GetValue(menu))
                                     .Cast<int>()
                                     .First();


        foreach (List<OptionsElement> optionElements in optionElementsLists)
        {
            if (optionSlots == null || optionSlots.Count == 0)
                continue;
            if (NarrateHoveredElementFromSlots(optionSlots, optionElements, currentItemIndex, allowFallback: allowFallback))
                return true;
        }
        return false;
    }

    public static bool NarrateHoveredElementFromSlots(List<ClickableComponent> optionSlots, List<OptionsElement> options, int currentItemIndex, bool allowFallback = false)
    {
        int x = Game1.getMouseX(true), y = Game1.getMouseY(true);
        for (int i = 0; i < optionSlots.Count; i++)
        {
            if (currentItemIndex + i >= options.Count || currentItemIndex < 0 || !optionSlots[i].bounds.Contains(x, y))
                continue;

            return NarrateElement(options[currentItemIndex + i], allowFallback: allowFallback);
        }

        return false;
    }

    public static bool NarrateHoveredElementFromList<T>(List<T> options, bool allowFallback = false) where T : OptionsElement
    {
        int x = Game1.getMouseX(true), y = Game1.getMouseY(true);
        for (int i = 0; i < options.Count; i++)
        {
            if (!options[i].bounds.Contains(x, y))
                continue;

            return NarrateElement(options[i], allowFallback: allowFallback);
        }

        return false;
    }

    // Returns true only if the element has something in "ScreenReaderText"
    public static bool NarrateElement(OptionsElement optionsElement, bool screenReaderInterrupt = true, bool allowFallback = false)
    {
        if (optionsElement.ScreenReaderIgnore) return false;

        var nameOfElement = GetNameOfElement(optionsElement, allowFallback);
        if (string.IsNullOrWhiteSpace(nameOfElement)) return false;
        
        MainClass.ScreenReader.SayWithMenuChecker(nameOfElement, interrupt: screenReaderInterrupt);
        return true;
    }

    public static string GetNameOfElement(OptionsElement optionsElement, bool allowFallback = false)
    {
        string translationKey;
        string label = allowFallback
            ? string.IsNullOrWhiteSpace(optionsElement.ScreenReaderText)
                ? optionsElement.label
                : optionsElement.ScreenReaderText
            : optionsElement.ScreenReaderText;
        object? tokens = new { label };

        switch (optionsElement)
        {
            case OptionsButton:
                translationKey = "options_element-button_info";
                break;
            case OptionsCheckbox checkbox:
                translationKey = "options_element-checkbox_info";
                tokens = new
                {
                    label,
                    is_checked = checkbox.isChecked ? 1 : 0
                };
                break;
            case OptionsDropDown dropdown:
                translationKey = "options_element-dropdown_info";
                tokens = new
                {
                    label,
                    selected_option = dropdown.dropDownDisplayOptions[dropdown.selectedOption]
                };
                break;
            case OptionsSlider slider:
                translationKey = "options_element-slider_info";
                tokens = new
                {
                    label,
                    slider_value = slider.value,
                    is_percentage = 0
                };
                break;
            case OptionsPlusMinus plusMinus:
                translationKey = "options_element-plus_minus_button_info";
                tokens = new
                {
                    label,
                    selected_option = plusMinus.displayOptions[plusMinus.selected]
                };
                break;
            case OptionsInputListener listener:
                string buttons = string.Join(", ", listener.buttonNames);
                translationKey = "options_element-input_listener_info";
                tokens = new
                {
                    label,
                    buttons_list = buttons
                };
                break;
            case OptionsTextEntry textEntry:
                translationKey = "options_element-text_box_info";
                tokens = new
                {
                    label,
                    value = string.IsNullOrEmpty(textEntry.textBox.Text) ? "null" : textEntry.textBox.Text,
                };
                break;
            default:
                return label;
        }

        return Translator.Instance.Translate(translationKey, tokens, TranslationCategory.Menu);
    }
}
