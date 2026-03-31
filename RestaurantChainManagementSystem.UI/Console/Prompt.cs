namespace RestaurantChainManagementSystem.UI.ConsoleUi;

public static class Prompt
{
    public static string Required(string label)
    {
        while (true)
        {
            Theme.Label($"{label}: ");
            var value = Console.ReadLine()?.Trim();

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            Theme.Warning("Value is required.");
        }
    }

    public static int Int(string label, int minValue = int.MinValue, int maxValue = int.MaxValue)
    {
        while (true)
        {
            Theme.Label($"{label}: ");
            var input = Console.ReadLine();

            if (int.TryParse(input, out var value) && value >= minValue && value <= maxValue)
            {
                return value;
            }

            Theme.Warning($"Enter a number between {minValue} and {maxValue}.");
        }
    }

    public static decimal Decimal(string label, decimal minValue = decimal.MinValue)
    {
        while (true)
        {
            Theme.Label($"{label}: ");
            var input = Console.ReadLine();

            if (decimal.TryParse(input, out var value) && value >= minValue)
            {
                return value;
            }

            Theme.Warning($"Enter a valid number greater than or equal to {minValue}.");
        }
    }

    public static bool Confirm(string label)
    {
        Theme.Label($"{label} (y/n): ");
        var input = Console.ReadLine()?.Trim().ToLowerInvariant();
        return input is "y" or "yes";
    }
}
