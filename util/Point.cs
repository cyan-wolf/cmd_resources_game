namespace util;

public struct Point(int x, int y)
{
    public int X { get; set; } = x;

    public int Y { get; set; } = y;

    public readonly (int, int) ToPair()
    {
        return (X, Y);
    }
}