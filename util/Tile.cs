namespace util;

public class Tile(char repr, Point position, Domain? domain = null)
{
    public char Repr { get; } = repr;

    public Point Position { get; } = position;

    public Domain? Domain { get; } = domain;

    public ConsoleColor Color { get; private set; } = domain?.Color ?? ConsoleColor.White;

    public static Tile Empty(Point pos)
    {
        return new Tile('.', pos);
    }

    public static Tile Border(Point pos) {
        var tile = new Tile('#', pos)
        {
            Color = ConsoleColor.DarkGray
        };

        return tile;
    }

    public void Show()
    {
        var oldCmdColor = Console.ForegroundColor;

        Console.ForegroundColor = Color;
        Console.Write(Repr);

        Console.ForegroundColor = oldCmdColor;
    }
}