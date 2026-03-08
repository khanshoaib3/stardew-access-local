using Microsoft.Xna.Framework;
using StardewValley;

namespace stardew_access.Features.Tracker;

using Utils;
using System.Collections.Generic;
using System.Linq;

class TrackedObjects
{
    /// <summary>
    /// {column:[{Name, SpecialObject}]}
    /// </summary>
    private readonly SortedList<string, Dictionary<string, SpecialObject>> Objects = [];

    public SortedList<string, Dictionary<string, SpecialObject>> GetObjects()
    {
        return Objects;
    }

    public void FindObjectsInArea(bool sortByProximity = true)
    {
        // TODO Maybe expose adding additional trackers to the API, but maybe this isn't needed as all the tiles will be added to stardew access anyways
        TTStardewAccess StardewAccessObjects = new();
        if (StardewAccessObjects.HasObjects())
        {
            AddObjects(StardewAccessObjects.GetObjects());
        }

        if (!sortByProximity)
        {
            Log.Debug("Sorting alphabetically");
            foreach (var cat in Objects)
            {
                var ordered = cat.Value.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                cat.Value.Clear();
                foreach (var item in ordered)
                {
                    cat.Value.Add(item.Key, item.Value);
                }
            }
        }
    }

    public void AddObjects(SortedList<string, Dictionary<string, SpecialObject>> objectsToAdd)
    {
        foreach (var kvp in objectsToAdd)
        {
            string category = kvp.Key;

            if (!Objects.ContainsKey(category))
            {
                Objects.Add(category, []);
            }

            foreach (var obj in kvp.Value)
            {
                if (!Objects.GetValueOrDefault(category!)!.ContainsKey(obj!.Key))
                    Objects.GetValueOrDefault(category)!.Add(obj!.Key, obj!.Value);
            }
        }
    }

    /// <summary>
    /// Copied from <see cref="TileTrackerBase.AddFocusableObject"/>
    /// </summary>
    /// <param name="category">The category of the tile, should i18n-ed</param>
    /// <param name="name">The name of the tile, should i18n-ed</param>
    /// <param name="tile">Tile's position</param>
    /// <param name="character">(Optional) The NPC at the tile</param>
    public void AddObject(string category, string name, Vector2 tile, NPC? character = null)
    {
        if (!Objects.ContainsKey(category))
        {
            Objects.Add(category, []);
        }

        SpecialObject sObject = new(name, tile);

        if (character != null)
        {
            sObject.character = character;
        }

        if (Objects[category].ContainsKey(name))
        {
            // This logic will go away as we will be implementing scrolling through multiple objects with same name
            sObject = TileTrackerBase.GetClosest(sObject, Objects[category][name]);
        }

        Objects[category][name] = sObject;
    }

    /// <summary>
    /// Removes the object with the given name in the given category.
    /// </summary>
    /// <param name="category">The category to look for the name</param>
    /// <param name="name">The name of the object/tile to remove</param>
    /// <returns>True if the object was successfully removed else false.</returns>
    public bool RemoveObject(string category, string name)
    {
        return Objects.TryGetValue(category, out var cat) && cat.Remove(name);
    }
}