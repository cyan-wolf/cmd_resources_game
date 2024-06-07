
using System.Diagnostics;
using System.Text;

namespace util;

public static class ColorUtils
{
    // Resets ANSI console format when printed.
    private const string PRINT_RESET_CODE = "\u001b[0m";

    // Writes the given content in the given color.
    public static void ColorWrite(string content, ConsoleColor color)
    {
        Console.Write(GetColorString(content, color));
    }

    // Writes a line of the given content in the given color.
    public static void ColorWriteLine(string content, ConsoleColor color)
    {
        ColorWrite($"{content}\n", color);
    }

    // Appends the given content in the given color to the string builder.
    // If `shouldResetColor` is true, this method automatically resets any applied format.
    public static void AppendToStrBuilder(StringBuilder str, object content, ConsoleColor color, bool shouldResetColor = true)
    {
        str.Append(GetANSIColorStartCode(color));
        str.Append(content);

        if (shouldResetColor)
        {
            str.Append(PRINT_RESET_CODE);
        }
    }

    // Returns a string, that when printed, shows the given content in the 
    // given color.
    public static string GetColorString(string content, ConsoleColor color)
    {
        var str = new StringBuilder(content.Length * 2);
        AppendToStrBuilder(str, content, color);

        return str.ToString();
    }

    // Gets the ANSI foreground color code for the given color.
    private static string ColorAsANSICode(ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.Red => "91",
            ConsoleColor.DarkRed => "31",
            ConsoleColor.Blue => "94",
            ConsoleColor.DarkBlue => "34",
            ConsoleColor.Green => "92",
            ConsoleColor.DarkGreen => "32",
            ConsoleColor.Cyan => "96",
            ConsoleColor.DarkCyan => "36",
            ConsoleColor.Yellow => "93",
            ConsoleColor.DarkYellow => "33",
            ConsoleColor.DarkGray => "90",
            ConsoleColor.Magenta => "95",
            ConsoleColor.DarkMagenta => "35",
            ConsoleColor.White => "97",
            ConsoleColor.Gray => "37",
            ConsoleColor.Black => "30",
            _ => throw new UnreachableException("impossible color"),
        };
    }

    // Creates a string that changes the console color format, when printed.
    private static string GetANSIColorStartCode(ConsoleColor color)
    {
        return $"\u001b[{ColorAsANSICode(color)}m";
    }
}