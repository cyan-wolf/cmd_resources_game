using System.Diagnostics;

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
        // The tiles near the origin represented by '%'.
        FORTIFICATION = 5,
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
        ChangeType(TileType.ORIGIN);
    }

    // Determines how likely this tile is to spread to neighboring tiles.
    public double GetAttackPower()
    {
        if (Type is TileType.FORTIFICATION)
        {
            return Math.Max(0.8, Domain!.GetAttackPower());
        }
        else 
        {
            return Domain?.GetAttackPower() ?? 0.0;
        }
    }

    // Determines how resistant this tile is to being replaced.
    public double GetDefense()
    {
        if (Type is TileType.FORTIFICATION)
        {
            return Math.Max(0.9, Domain?.GetDefense() ?? 0.0);
        }
        else if (Type is TileType.BORDER)
        {
            return 1.0;
        }
        else 
        {
            return Domain?.GetDefense() ?? 0.0;
        }
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
        if (Type is TileType.NORMAL && Repr != '*')
        {
            // Change the representation to show that the tile has been around for at least a turn.
            Repr = '*';
            return;
        }

        if (Type is TileType.NORMAL or TileType.HOUSING)
        {
            var statusChanged = TryToFortify(rnd);
            if (statusChanged) { return; }

            statusChanged = TryToChangeHousingStatus(rnd);
            if (statusChanged) { return; }
        }
    }

    // This method returns `null` if either:
    // 1. This tile has no domain.
    // 2. This tile's domain has lost its origin.
    public double? GetDistanceToDomainOrigin()
    {
        if (Domain is null) 
        {
            return null;
        }
        else if (Domain.Origin is null)
        {
            return null;
        }

        var originPos = Domain.Origin.Position;
        
        // Taxicab distance.
        return Math.Abs(Position.X - originPos.X) + Math.Abs(Position.Y - originPos.Y);
    }

    public bool IsOrigin()
    {
        return Type is TileType.ORIGIN;
    }

    // This method is only called on `NORMAL` or `HOUSING` tiles.
    private bool TryToChangeHousingStatus(Random rnd)
    {
        const double CHANCE_TO_CHANGING_HOUSING_STATUS = 0.05;

        if (rnd.NextDouble() < CHANCE_TO_CHANGING_HOUSING_STATUS)
        {
            if (Domain!.ShouldIncreaseHousingTiles())
            {
                ChangeType(TileType.HOUSING);
            }
            else if (Domain!.ShouldDecreaseHousingTiles())
            {
                ChangeType(TileType.NORMAL);
            }

            return true;
        }
        return false;
    }

    // This method is only called on `NORMAL` tiles.
    // Returns `true` if this tile managed to update.
    private bool TryToFortify(Random rnd)
    {
        // Utility funciton used to calculate the tile's "promotion chance",
        // based on its distance to the domain origin and the size (tile count) of the domain.
        static double promotionChanceCalc(double d, double c)
        {
            return Math.Exp(-1.0 * (d / 2 + 100 / c));
        }

        const double CHANCE_TO_FORTIFY = 0.025;

        if (rnd.NextDouble() < CHANCE_TO_FORTIFY)
        {
            var dist = GetDistanceToDomainOrigin() ?? throw new UnreachableException();
            var count = Domain!.GetTileCount();

            var promotionChance = promotionChanceCalc(dist, count);

            if (rnd.NextDouble() < promotionChance)
            {
                ChangeType(TileType.FORTIFICATION);
            }

            return true;
        }

        return false;
    }

    private void ChangeType(TileType newType)
    {
        if (Type is TileType.HOUSING && newType is not TileType.HOUSING)
        {
            Domain!.HousingTiles.Remove(this);
        }

        switch (newType)
        {
        case TileType.EMPTY:
            Repr = '.';
            break;

        case TileType.BORDER:
            Repr = '#';
            break;

        case TileType.NORMAL:
            Repr = '*';
            break;

        case TileType.ORIGIN:
            Repr = '$';
            break;

        case TileType.HOUSING:
            Repr = '^';
            Domain!.HousingTiles.Add(this);
            break;

        case TileType.FORTIFICATION:
            Repr = '%';
            break;
        }

        Type = newType;
    }
}