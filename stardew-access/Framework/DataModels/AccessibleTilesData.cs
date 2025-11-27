using stardew_access.Tiles;
using stardew_access.Utils;

namespace stardew_access.Framework.DataModels;

public class AccessibleTilesData
{
    public List<AccessibleMapData> Maps = [];
}

public class AccessibleMapData
{
    public string Id = "";
    // TODO Maybe add translated display name??
    
    public List<AccessibleTileData> Tiles = [];
}

public class AccessibleTileData
{
    public string DisplayName = "";
    public int[] X = [];
    public int[] Y = [];
    public string Category = "";

    public AccessibleTile toAccessibleTile()
    {
        return new AccessibleTile(
            staticNameOrTranslationKey: DisplayName,
            staticCoordinates: AccessibleTile.generateCoordinatesCombination(X, Y),
            category: CATEGORY.FromString(Category)
        );
    }
}