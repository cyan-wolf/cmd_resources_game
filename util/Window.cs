namespace util;

public class Window
{
    private List<Domain> _domains = [
        new Domain(ConsoleColor.Red),
        new Domain(ConsoleColor.Blue),
        new Domain(ConsoleColor.Green),
        new Domain(ConsoleColor.Magenta),
    ];

    private Random rnd = new();

    public Window(Rect dimensions)
    {
        Dimensions = dimensions;
        World = new Tile[dimensions.X, dimensions.Y];

        InitWorld();

        SpawnTileOnWorld(new(3, 3), _domains[0]);
        SpawnTileOnWorld(new(7, 7), _domains[1]);
        SpawnTileOnWorld(new(15, 15), _domains[2]);
        SpawnTileOnWorld(new(15, 3), _domains[3]);
    }

    public Rect Dimensions { get; }

    public Tile[,] World { get; } 

    public void Update()
    {
        TryToSpreadDomains();
        Draw();
    }

    // Draws the tiles on screen based on the world state.
    private void Draw()
    {
        for (int x = 0; x < Dimensions.X; x++)
        {
            for (int y = 0; y < Dimensions.Y; y++)
            {
                var tile = World[x, y];
                tile.Show();
            }
            Console.WriteLine();
        }
    }

    // Initializes the tiles in the game world.
    private void InitWorld()
    {
        for (int x = 0; x < Dimensions.X; x++)
        {
            for (int y = 0; y < Dimensions.Y; y++)
            {
                if (IsOnBorder(new (x, y)))
                {
                    World[x, y] = Tile.Border(new(x, y));
                }
                else
                {
                    World[x, y] = Tile.Empty(new(x, y));
                }
            }
        }
    }

    // Determines whether the given position is on the border.
    private bool IsOnBorder(Point pos)
    {
        return pos.X == 0 
            || pos.Y == 0 
            || pos.X == Dimensions.X - 1
            || pos.Y == Dimensions.Y - 1;
    }

    private bool IsInWorld(Point pos)
    {
        return pos.X >= 0
            || pos.Y >= 0
            || pos.X < Dimensions.X
            || pos.Y < Dimensions.Y;
    }

    private Tile SpawnTileOnWorld(Point pos, Domain domain)
    {
        var tile = new Tile('@', pos, domain.Color);

        // TODO: When a tile from a domain is replaced it needs to be removed 
        // from the domain (i.e. call `domain.RemoveTile(tileToBeReplaced)`).

        // Add the tile to the world.
        World[pos.X, pos.Y] = tile;

        // Add the tile to the domain.
        domain.AddTile(tile);

        // Return the newly created tile.
        return tile;
    }

    private void TryToSpreadDomains()
    {
        Queue<Action> spreadAttemptQueue = [];
        double chanceOfChoosingToSpreadTile = 0.5;

        // Iterate over all the domains in the world.
        foreach (var domain in _domains)
        {
            // Iterate over all the tiles part of a particular domain.
            foreach (var tile in domain.GetTilesEnumerable())
            {
                // Randomly decide whether to choose this tile.
                if (rnd.NextDouble() < chanceOfChoosingToSpreadTile)
                {
                    var tilePos = tile.Position;

                    Point[] neighborPositions = [
                        new(tilePos.X + 1, tilePos.Y),
                        new(tilePos.X - 1, tilePos.Y),
                        new(tilePos.X, tilePos.Y + 1),
                        new(tilePos.X, tilePos.Y - 1),
                    ];

                    // Shuffle the neighbor positions so that the domain 
                    // doesn't try to spread in the same direction each time.
                    Random.Shared.Shuffle(neighborPositions);

                    // Iterate over the neighbors.
                    foreach (var nbrPos in neighborPositions)
                    {
                        // Check whether the tile can spread to this position.
                        if (!IsOnBorder(nbrPos) && IsInWorld(nbrPos))
                        {
                            // Queue this operation to do it after all the iterations.
                            // As to not invalidate the iterator.
                            spreadAttemptQueue.Enqueue(() => SpawnTileOnWorld(nbrPos, domain));
                            // Break out of the neighbor `foreach` loop since we 
                            // only want to spread out once per tile.
                            break;
                        }
                    }
                }
            }
        }

        // Perform all the spread attempts after the iterations.
        while (spreadAttemptQueue.Count > 0)
        {
            var spreadAttempt = spreadAttemptQueue.Dequeue();
            spreadAttempt();
        }
    }
}