namespace util;

public static class ColorUtils
{
    public static void ColorWrite(string content, ConsoleColor fgColor)
    {
        var originalFgColor = Console.ForegroundColor;
        Console.ForegroundColor = fgColor;
        Console.Write(content);
        Console.ForegroundColor = originalFgColor;
    }

    public static void ColorWriteLine(string content, ConsoleColor fgColor)
    {
        ColorWrite($"{content}\n", fgColor);
    }
}