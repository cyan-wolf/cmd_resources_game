

namespace util;

public class Domain(ConsoleColor color)
{
    public ConsoleColor Color => color;

    public Tile Origin { get; private set; }

    // All of the tiles that belong to this domain.
    private readonly HashSet<Tile> _tiles = [];

    // Used for keeping track of what tiles are currently housing.
    public HashSet<Tile> HousingTiles { get; } = [];

    private double _attackPower = 0.4;

    private double _defense = 0.7;

    public void AddTile(Tile tile)
    {
        _tiles.Add(tile);
    }

    public void SetOriginTile(Tile tile)
    {
        Origin = tile;
        tile.MakeOrigin();
    }

    public void RemoveTile(Tile tile)
    {
        tile.Destroy();
        _tiles.Remove(tile);
    }

    public int GetTileCount()
    {
        return _tiles.Count;
    }

    public IEnumerable<Tile> GetTilesEnumerable()
    {
        return _tiles;
    }

    public double GetAttackPower()
    {
        if (OriginIsActive())
        {
            return _attackPower;
        }
        else
        {
            return 1.0;
        }
    }

    public double GetDefense()
    {
        if (OriginIsActive())
        {
            return _defense;
        }
        else
        {
            return 0.0;
        }
    }

    // Used to approximately show how many housing tiles
    // this domain has.
    public double GetPopulation()
    {
        var rnd = new Random();

        return ((0.1 * GetTileCount()) + HousingTiles.Count * (2 + 0.5*rnd.NextDouble())) * (1 + 0.1*rnd.NextDouble());
    }

    public bool OriginIsActive()
    {
        return _tiles.Contains(Origin);
    }

    public bool ShouldIncreaseHousingTiles()
    {
        return HousingTiles.Count < GetPreferredHousingTileAmount();
    }

    // Returns the number of housing tiles that the domain would want to have.
    private int GetPreferredHousingTileAmount()
    {
        return GetTileCount() / 20;
    }
}