namespace RestaurantChainManagementSystem.UI.ConsoleUi;

public static class Theme
{
    public static void Header(string text)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(new string('═', 72));
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(text);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(new string('═', 72));
        Console.ResetColor();
    }

    public static void Success(string text)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    public static void Warning(string text)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    public static void Error(string text)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    public static void Label(string text)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write(text);
        Console.ResetColor();
    }
}
