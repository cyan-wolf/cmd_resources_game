namespace util;

public class Tile(char repr, ConsoleColor color = ConsoleColor.White)
{
    public char Repr { get; } = repr;

    public ConsoleColor Color { get; } = color;

    public static Tile Empty()
    {
        return new Tile('.');
    }

    public static Tile Border() {
        return new Tile('#', ConsoleColor.DarkGray);
    }

    public void Show()
    {
        var oldCmdColor = Console.ForegroundColor;

        Console.ForegroundColor = Color;
        Console.Write(Repr);

        Console.ForegroundColor = oldCmdColor;
    }
}