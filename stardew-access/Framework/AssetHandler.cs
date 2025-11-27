using stardew_access.Framework.DataModels;
using StardewModdingAPI.Events;
using StardewValley;

namespace stardew_access.Framework;

public static class AssetHandler
{
    private const string TrackedMachinesDataAssetName = "shoaib.stardewaccess/TrackedMachinesData";
    private const string AccessibleTilesDataAssetName = "shoaib.stardewaccess/AccessibleTilesData";

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
            Log.Trace($"Loaded asset {TrackedMachinesDataAssetName} with {modsData.Count} entries.");
            return _trackedMachines;
        }
    }

    private static Dictionary<string, AccessibleMapData>? _accessibleTilesData;

    public static Dictionary<string, AccessibleMapData> AccessibleTilesData
    {
        get
        {
            if (_accessibleTilesData != null) return _accessibleTilesData;

            var modsData = Game1.content.Load<Dictionary<string, AccessibleTilesData>>(AccessibleTilesDataAssetName);
            _accessibleTilesData = new Dictionary<string, AccessibleMapData>();
            foreach (var accessibleTilesData in modsData)
            {
                foreach (var accessibleTileMapData in accessibleTilesData.Value.Maps)
                {
                    _accessibleTilesData.Add(accessibleTileMapData.Id, accessibleTileMapData);
                }
            }
            
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
                Log.Trace($"Asset {TrackedMachinesDataAssetName} invalidated, reloading.");
                _trackedMachines = null;
            }
            if (name.IsEquivalentTo(AccessibleTilesDataAssetName))
            {
                Log.Trace($"Asset {AccessibleTilesDataAssetName} invalidated, reloading.");
                _accessibleTilesData = null;
            }
        }
    }
}