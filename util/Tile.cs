namespace util;

public class Tile(char repr, Point position, ConsoleColor color = ConsoleColor.White)
{
    public char Repr { get; } = repr;

    public Point Position { get; } = position;

    public ConsoleColor Color { get; } = color;

    public static Tile Empty(Point pos)
    {
        return new Tile('.', pos);
    }

    public static Tile Border(Point pos) {
        return new Tile('#', pos, ConsoleColor.DarkGray);
    }

    public void Show()
    {
        var oldCmdColor = Console.ForegroundColor;

        Console.ForegroundColor = Color;
        Console.Write(Repr);

        Console.ForegroundColor = oldCmdColor;
    }
}