namespace util;

public class Window
{
    public Window(Rect dimensions)
    {
        Dimensions = dimensions;
        World = new Tile[dimensions.X, dimensions.Y];

        InitWorld();
    }

    public Rect Dimensions { get; }

    public Tile[,] World { get; } 

    public void Update()
    {
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
                    World[x, y] = Tile.Border();
                }
                else
                {
                    World[x, y] = Tile.Empty();
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
}