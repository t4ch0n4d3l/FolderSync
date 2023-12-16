public class C : IDisposable
{
    private readonly ConsoleColor _foreground, _background;

    private C(ConsoleColor foreground, ConsoleColor background)
    {
        _foreground = foreground;
        _background = background;
    }

    public static IDisposable Foreground(ConsoleColor foreground) => Color(foreground, Console.BackgroundColor);

    public static IDisposable Background(ConsoleColor background) => Color(Console.ForegroundColor, background);

    public static IDisposable Color(ConsoleColor foreground, ConsoleColor background)
    {
        var currentForeground = Console.ForegroundColor;
        var currentBackground = Console.BackgroundColor;
        Console.ForegroundColor = foreground;
        Console.BackgroundColor = background;
        return new C(currentForeground, currentBackground);
    }

    public void Dispose()
    {
        Console.ForegroundColor = _foreground;
        Console.BackgroundColor = _background;
    }
}