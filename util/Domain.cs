

namespace util;

public class Domain(ConsoleColor color)
{
    public ConsoleColor Color => color;

    public Tile Origin { get; private set; }

    private readonly HashSet<Tile> _tiles = [];

    private double _attackPower = 0.4;

    private double _defense = 0.7;

    public void AddTile(Tile tile)
    {
        _tiles.Add(tile);
    }

    public void SetOriginTile(Tile tile)
    {
        Origin = tile;
        tile.Repr = '$';
    }

    public void RemoveTile(Tile tile)
    {
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

    public bool OriginIsActive()
    {
        return _tiles.Contains(Origin);
    }
}