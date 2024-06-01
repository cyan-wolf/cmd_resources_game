namespace util;

public class Window
{
    private readonly List<Domain> _domains = [];

    private readonly Random rnd = new();

    private bool _isFirstFrame = true;

    public Window(Rect dimensions, List<Point> domainStartingPositions)
    {
        Dimensions = dimensions;
        World = new Tile[dimensions.X, dimensions.Y];

        // Fill in the world with empty and border tiles.
        InitWorld();

        // Initialize the domains in the world.
        InitDomains(domainStartingPositions);
    }

    public Rect Dimensions { get; }

    public Tile[,] World { get; } 

    public void Update()
    {
        // Prevents the domains from spreading in the first frame.
        if (_isFirstFrame)
        {
            Draw();
            ShowScorePerDomain();
            _isFirstFrame = false;
            return;
        }

        TryToSpreadDomains();
        Draw();
        ShowScorePerDomain();
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

    private void InitDomains(List<Point> domainStartingPositions)
    {
        // 13 colors
        Queue<ConsoleColor> domainColors = new ([
            // Main colors.
            ConsoleColor.Red,
            ConsoleColor.Blue,
            ConsoleColor.Green,
            ConsoleColor.Magenta,
            ConsoleColor.Yellow,
            ConsoleColor.Cyan,
            ConsoleColor.DarkYellow,
            ConsoleColor.Gray,

            // Colors that are harder to tell apart.
            ConsoleColor.DarkRed,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkCyan,
            ConsoleColor.DarkGreen,
            ConsoleColor.DarkBlue,
        ]);
        
        // Initialize all the domains specified by the user.
        foreach (var startPos in domainStartingPositions)
        {
            // Create a new domain with a preselected color.
            var domain = new Domain(domainColors.Dequeue());

            // Register the domain.
            _domains.Add(domain);

            // Give the domain a starting tile.
            SpawnTileOnWorld(startPos, domain);
        }
    }

    // Shows how many tiles each domain has.
    private void ShowScorePerDomain()
    {
        var originalFgColor = Console.ForegroundColor;

        for (int i = 0; i < _domains.Count; i++)
        {
            Console.Write("Domain ");
            ColorUtils.ColorWrite($"#{i + 1} ", _domains[i].Color);

            var plurality = (_domains[i].GetTileCount() != 1) ? "s" : "";
            Console.Write($"has {_domains[i].GetTileCount()} tile{plurality}. Status: (");
            
            if (_domains[i].GetTileCount() > 0)
            {
                ColorUtils.ColorWrite("Active", ConsoleColor.Green);
            }
            else
            {
                ColorUtils.ColorWrite("Defeated", ConsoleColor.Red);
            }

            Console.WriteLine(")");
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
        var tile = new Tile('@', pos, domain);

        // When a tile from a domain is replaced it needs to be removed 
        // from the domain's internal listing.
        var tileToBeReplaced = World[pos.X, pos.Y];
        tileToBeReplaced.Domain?.RemoveTile(tileToBeReplaced);

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
                        new(tilePos.X, tilePos.Y + 1),
                        new(tilePos.X - 1, tilePos.Y),
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
                            // Prevent the spawn of a tile at this position if it 
                            // is part of the same domain.
                            if (tile.Domain!.Equals(World[nbrPos.X, nbrPos.X].Domain))
                            {
                                continue;
                            }
                            // Queue this operation to do it after all the iterations.
                            // As to not to invalidate the iterator.
                            spreadAttemptQueue.Enqueue(() => {
                                SpawnTileOnWorld(nbrPos, domain);
                            });
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