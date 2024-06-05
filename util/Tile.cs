using System.Threading.Channels;

namespace util;

public class Tile(char repr, Point position, Domain? domain = null)
{
    private enum TileType
    {
        // The empty tiles represented by '.'.
        EMPTY = 0,
        // The border tiles represented by '#'.
        BORDER = 1,
        // The regular tiles represented by '@' at the start and then by '*'.
        NORMAL = 2,
        // The origin tiles represented by '$'.
        ORIGIN = 3,
        // The housing tiles represented by '^'.
        HOUSING = 4,
    }

    public char Repr { get; set; } = repr;

    public Point Position { get; } = position;

    public Domain? Domain { get; } = domain;

    public ConsoleColor Color { get; private set; } = domain?.Color ?? ConsoleColor.White;

    private TileType Type { get; set; } = TileType.NORMAL;

    public static Tile Empty(Point pos)
    {
        return new Tile('.', pos) 
        {
            Type = TileType.EMPTY,
        };
    }

    public static Tile Border(Point pos) {
        var tile = new Tile('#', pos)
        {
            Color = ConsoleColor.DarkGray,
            Type = TileType.BORDER,
        };

        return tile;
    }

    // Called by this tile's domain.
    public void MakeOrigin()
    {
        Repr = '$';
        Type = TileType.ORIGIN;
    }

    // Determines how likely this tile is to spread to neighboring tiles.
    public double GetAttackPower()
    {
        return Domain?.GetAttackPower() ?? 0.0;
    }

    // Determines how resistant this tile is to being replaced.
    public double GetDefense()
    {
        return Domain?.GetDefense() ?? 0.0;
    }

    public void Show()
    {
        ColorUtils.ColorWrite(Repr.ToString(), Color);
    }

    // Called when this tile is replaced by an enemy tile.
    public void Destroy()
    {
        if (Type is TileType.HOUSING)
        {
            Domain!.HousingTiles.Remove(this);
        }
    }

    // Called every frame.
    public void UpdateTileStatus(Random rnd)
    {
        if (Type is TileType.EMPTY or TileType.BORDER or TileType.ORIGIN)
        {
            return;
        }
        if (Type is TileType.NORMAL)
        {
            // Change the representation to show that the tile has been around for at least a turn.
            Repr = '*';
        }

        const double CHANCE_TO_CHANGING_HOUSING_STATUS = 0.05;

        if (rnd.NextDouble() < CHANCE_TO_CHANGING_HOUSING_STATUS)
        {
            if (Domain!.ShouldIncreaseHousingTiles())
            {
                Type = TileType.HOUSING;
                Repr = '^';
                Domain!.HousingTiles.Add(this);
            }
            else 
            {
                Type = TileType.NORMAL;
                Repr = '*';
                Domain!.HousingTiles.Remove(this);
            }
        }
    }
}