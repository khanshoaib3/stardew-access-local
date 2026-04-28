using stardew_access.Framework.DataModels;
using StardewModdingAPI.Events;
using StardewValley;

namespace stardew_access.Framework;

public static class AssetHandler
{
    private const string TrackedMachinesDataAssetName = "shoaib.stardewaccess/TrackedMachinesData";
    private const string AccessibleTilesDataAssetName = "shoaib.stardewaccess/AccessibleTilesData";
    
    public static event EventHandler? OnAccessibleTilesDataAssetInvalidated;

    private static HashSet<string>? _trackedMachines;

    public static HashSet<string> TrackedMachines
    {
        get
        {
            if (_trackedMachines != null) return _trackedMachines;

            var modsData = Game1.content.Load<Dictionary<string, TrackedMachinesData>>(TrackedMachinesDataAssetName);
            _trackedMachines = new HashSet<string>(
                modsData
                    .Values
                    .SelectMany(m => m.QualifiedObjectIds)
            );
            Log.Info($"Loaded asset {TrackedMachinesDataAssetName} with {modsData.Count} mod entries.");
            return _trackedMachines;
        }
    }

    // <map_id, <mod_id, map_data>>
    private static Dictionary<string, Dictionary<string, AccessibleMapData>>? _accessibleTilesData;

    public static Dictionary<string, Dictionary<string, AccessibleMapData>> AccessibleTilesData
    {
        get
        {
            if (_accessibleTilesData != null) return _accessibleTilesData;

            var modsData = Game1.content.Load<Dictionary<string, AccessibleTilesData>>(AccessibleTilesDataAssetName);
            _accessibleTilesData = new Dictionary<string, Dictionary<string, AccessibleMapData>>();
            foreach (var accessibleTilesData in modsData)
            {
                foreach (var accessibleTileMapData in accessibleTilesData.Value.Maps)
                {
                    if (_accessibleTilesData.ContainsKey(accessibleTileMapData.Id))
                    {
                        _accessibleTilesData[accessibleTileMapData.Id].Add(accessibleTilesData.Key, accessibleTileMapData);
                    }
                    else
                    {
                        _accessibleTilesData.Add(accessibleTileMapData.Id, new Dictionary<string, AccessibleMapData>()
                        {
                            { accessibleTilesData.Key, accessibleTileMapData }
                        });
                    }
                }
            }
            
            Log.Info($"Loaded {AccessibleTilesDataAssetName} with {modsData.Count} mod entries. Loaded {_accessibleTilesData.Count} maps.");
            return _accessibleTilesData;
        }
    }

    public static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(TrackedMachinesDataAssetName))
        {
            e.LoadFrom(() => new Dictionary<string, TrackedMachinesData>(), AssetLoadPriority.Exclusive);
            _accessibleTilesData = null;
        }
        if (e.NameWithoutLocale.IsEquivalentTo(AccessibleTilesDataAssetName))
        {
            e.LoadFrom(() => new Dictionary<string, AccessibleTilesData>(), AssetLoadPriority.Exclusive);
            _accessibleTilesData = null;
        }
    }

    public static void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        foreach (var name in e.NamesWithoutLocale)
        {
            if (name.IsEquivalentTo(TrackedMachinesDataAssetName))
            {
                Log.Debug($"Asset {TrackedMachinesDataAssetName} invalidated, reloading.");
                _trackedMachines = null;
            }
            if (name.IsEquivalentTo(AccessibleTilesDataAssetName))
            {
                Log.Debug($"Asset {AccessibleTilesDataAssetName} invalidated, reloading.");
                _accessibleTilesData = null;
                OnAccessibleTilesDataAssetInvalidated?.Invoke(sender, e);
            }
        }
    }
}