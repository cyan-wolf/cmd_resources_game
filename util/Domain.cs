

namespace util;

public class Domain(ConsoleColor color)
{
    public ConsoleColor Color => color;

    private readonly HashSet<Tile> _tiles = [];

    public void AddTile(Tile tile)
    {
        _tiles.Add(tile);
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
}