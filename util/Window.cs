using System.Text;

namespace util;

public class Window
{
    public static Window Current => Game.Current.Window!;

    public Rect Dimensions { get; }

    public Tile[,] World { get; } 

    private readonly List<Domain> _domains = [];

    private readonly Random rnd = new();

    private bool _isFirstFrame = true;

    public Window(Rect dimensions, List<Point> domainStartingPositions, char[,]? customWorldLayout = null)
    {
        Dimensions = dimensions;
        World = new Tile[dimensions.X, dimensions.Y];
        
        if (customWorldLayout is not null)
        {
            InitCustomWorld(customWorldLayout);
        }
        else
        {
            // Fill in the world with empty and border tiles.
            InitDefaultWorld();
        }

        // Initialize the domains in the world.
        InitDomains(domainStartingPositions);
    }

    // Returns the active domains, ordered by tile count.
    // The domain with the most tiles is the first element, and 
    // the one with the least is the last element.
    public List<Domain> GetActiveDomainLeaderboard()
    {
        // Copies the domain list.
        var domainList = _domains
            .OrderBy(d => d.GetTileCount())
            .Reverse()
            .Where(d => d.GetTileCount() > 0)
            .ToList();

        return domainList;
    }

    // Returns the domains that no longer have tiles.
    public List<Domain> GetDefeatedDomains()
    {
        var domainList = _domains
            .Where(d => d.GetTileCount() == 0)
            .ToList();

        return domainList;
    }

    public void Update()
    {
        // Prevents the domains from spreading in the first frame.
        if (_isFirstFrame)
        {
            Draw();
            ShowScorePerDomain();
            CheckForAndDisplayWinner();
            _isFirstFrame = false;
            return;
        }

        UpdateDomainTiles();
        UpdateSpecialEvents();

        Draw();
        ShowScorePerDomain();
        CheckForAndDisplayWinner();
    }

    // Draws the tiles on screen based on the world state.
    private void Draw()
    {
        // String builder that stores the window content to be printed at the end of the draw method.
        var windowContent = new StringBuilder(Dimensions.X * Dimensions.Y * 2);

        for (int x = 0; x < Dimensions.X; x++)
        {
            for (int y = 0; y < Dimensions.Y; y++)
            {
                var tile = World[x, y];

                // Append the tile to the builder, using its string representation and color.
                ColorUtils.AppendToStrBuilder(
                    windowContent, 
                    tile.Repr, 
                    tile.Color,
                    shouldResetColor: false
                );
            }
            // Seperate each row by a new line.
            windowContent.Append('\n');
        }
        
        // Reset any color formating once all the content has been appended to the window.
        windowContent.Append(ColorUtils.PRINT_RESET_CODE);

        // Draw (print) the window content to the console.
        Console.Write(windowContent);
    }

    private void InitCustomWorld(char[,] customWorldLayout)
    {
        for (int x = 0; x < Dimensions.X; x++)
        {
            for (int y = 0; y < Dimensions.Y; y++)
            {
                char symbol = customWorldLayout[x, y];

                World[x, y] = symbol switch
                {
                    '#' => Tile.Border(new(x, y)),
                    _ => Tile.Empty(new(x, y)),
                };
            }
        }
    }

    // Initializes the tiles in the game world, if no custom map is provided.
    private void InitDefaultWorld()
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

            // Give the domain a starting tile (origin).
            var originTile = SpawnTileOnWorld(startPos, domain);

            domain.SetOriginTile(originTile);
        }
    }

    // Shows how many tiles each domain has.
    private void ShowScorePerDomain()
    {
        for (int i = 0; i < _domains.Count; i++)
        {
            Console.Write("Domain ");
            ColorUtils.ColorWrite($"#{i + 1} ", _domains[i].Color);

            var plurality = (_domains[i].GetTileCount() != 1) ? "s" : "";
            Console.Write($"has {_domains[i].GetTileCount()} tile{plurality}. Status: (");

            if (_domains[i].GetTileCount() == 0)
            {
                ColorUtils.ColorWrite("Defeated", ConsoleColor.Red);
            }
            else if (! _domains[i].OriginIsActive())
            {
                ColorUtils.ColorWrite("No Origin", ConsoleColor.DarkYellow);
            }
            else
            {
                ColorUtils.ColorWrite("Active", ConsoleColor.Green);
            }

            Console.Write(") ");
            Console.Write($"Population: {_domains[i].GetPopulation():n}");

            if (_domains[i].IsInCounterOffensive())
            {
                Console.Write(" (Counterattacking)");
            }

            Console.WriteLine();
        }
    }

    private void CheckForAndDisplayWinner()
    {
        // There cannot be a winner if there are no domains.
        if (_domains.Count == 0)
        {
            return;
        }

        int lastSurvivingDomainIdx = -1;
        int defeatedAmount = 0;

        for (int i = 0; i < _domains.Count; i++)
        {
            if (_domains[i].GetTileCount() == 0)
            {
                defeatedAmount++;
            }
            else 
            {
                lastSurvivingDomainIdx = i;
            }
        }

        if (defeatedAmount == _domains.Count - 1)
        {
            Domain winner = _domains[lastSurvivingDomainIdx];

            Console.Write("Domain ");
            ColorUtils.ColorWrite($"#{lastSurvivingDomainIdx + 1}", winner.Color);
            Console.WriteLine(" is the winner.");
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

        // If a domain without an origin happens to replace another domain's 
        // origin, then the domain (that is spreading) gets a new origin.
        if (!domain.OriginIsActive() && tileToBeReplaced.IsOrigin())
        {
            domain.SetOriginTile(tile);
        }

        // Return the newly created tile.
        return tile;
    }

    private void UpdateDomainTiles()
    {
        Queue<Action> spreadAttemptQueue = [];

        // Iterate over all the domains in the world.
        foreach (var domain in _domains)
        {
            // Iterate over all the tiles part of a particular domain.
            foreach (var tile in domain.GetTilesEnumerable())
            {
                // Update the tile.
                tile.UpdateTileStatus(rnd);

                // Process tile spread.
                double chanceOfChoosingToSpreadTile = tile.GetAttackPower();

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
                            // Queue this operation to do it after all the iterations.
                            // As to not to invalidate the iterator.
                            spreadAttemptQueue.Enqueue(() => {
                                // Prevent the spawn of a tile at this position if it 
                                // is part of the same domain.
                                if (tile.Domain!.Equals(World[nbrPos.X, nbrPos.Y].Domain))
                                {
                                    return;
                                }

                                var chanceToDefend = World[nbrPos.X, nbrPos.Y].GetDefense();

                                // Use the neighbor tile's defense to determine whether to spread.
                                if (rnd.NextDouble() > chanceToDefend)
                                {
                                    //ColorUtils.ColorWriteLine("trying to spread", domain.Color);
                                    SpawnTileOnWorld(nbrPos, domain);
                                }
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

    // Causes or stops special events such as alliances, etc.
    private void UpdateSpecialEvents()
    {
        if (rnd.NextDouble() < 0.12)
        {
            // Try to start a counter offensive on the worst performing domain.
            if (rnd.NextDouble() < 0.025)
            {
                List<Domain> leaderboard = GetActiveDomainLeaderboard();

                if (leaderboard.Count < 2) { return; }

                Domain strongestDomain = leaderboard[0];
                Domain weakestDomain = leaderboard[^1]; // last element of the leaderboard

                // Most of the time, the weakest domain is the one that will perform the counter offensive.
                if (rnd.NextDouble() < 0.9)
                {
                    weakestDomain.StartCounterOffensive();
                }
                // Rarely, the strongest domain is the one that will perform it.
                else
                {
                    strongestDomain.StartCounterOffensive();
                }
            }

            // Try to revive a defeated domain.
            else if (rnd.NextDouble() < 0.08)
            {
                List<Domain> defeatedDomains = GetDefeatedDomains();
                List<Domain> leaderboard = GetActiveDomainLeaderboard();

                if (defeatedDomains.Count == 0 || leaderboard.Count == 0) 
                {
                    return;
                }

                // Pick a random defeated domain.
                var domainToRevive = defeatedDomains[rnd.Next(defeatedDomains.Count)];

                var strongestDomain = leaderboard[0];
                // This is slow.
                List<Tile> strongestDomainTiles = strongestDomain.GetTilesEnumerable().ToList();

                Tile tileToBeReplaced = strongestDomainTiles[rnd.Next(strongestDomainTiles.Count)];

                // Revive the domain.
                var newTile = SpawnTileOnWorld(tileToBeReplaced.Position, domainToRevive);

                // Make the domain start out in counter offensive mode.
                domainToRevive.StartCounterOffensive();

                // The revived domain has a chance to have a new origin (or not).
                // We check if the tile to be replaced is an origin, since there is another mechanic that 
                // automatically makes the spreading tile an origin if it spreads over another domain's origin if 
                // its own domain lacks one.
                if (! tileToBeReplaced.IsOrigin() && rnd.NextDouble() < 0.5)
                {
                    domainToRevive.SetOriginTile(newTile);
                }
            }
        }
        // Try to clear any special effects.
        else
        {
            foreach (var domain in _domains)
            {
                // Try to end counter offensive.
                if (rnd.NextDouble() < 0.025)
                {
                    if (domain.IsInCounterOffensive())
                    {
                        domain.EndCounterOffensive();
                    }
                }
            }
        }
    }
}