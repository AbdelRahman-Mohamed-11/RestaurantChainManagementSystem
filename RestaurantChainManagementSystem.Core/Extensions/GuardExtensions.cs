namespace RestaurantChainManagementSystem.Core.Extensions;

public static class GuardExtensions
{
    public static string Required(this string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return value.Trim();
    }

    public static string GuidId(this string? value, string fieldName)
    {
        var normalized = value.Required(fieldName);

        if (!Guid.TryParse(normalized, out _))
        {
            throw new InvalidOperationException($"{fieldName} must be a valid GUID.");
        }

        return normalized;
    }

    public static decimal Positive(this decimal value, string fieldName)
    {
        if (value <= 0)
        {
            throw new InvalidOperationException($"{fieldName} must be greater than zero.");
        }

        return value;
    }

    public static int Positive(this int value, string fieldName)
    {
        if (value <= 0)
        {
            throw new InvalidOperationException($"{fieldName} must be greater than zero.");
        }

        return value;
    }

    public static int NotNegative(this int value, string fieldName)
    {
        if (value < 0)
        {
            throw new InvalidOperationException($"{fieldName} cannot be negative.");
        }

        return value;
    }

    public static decimal NotNegative(this decimal value, string fieldName)
    {
        if (value < 0)
        {
            throw new InvalidOperationException($"{fieldName} cannot be negative.");
        }

        return value;
    }

    public static DateOnly NotFuture(this DateOnly value, string fieldName)
    {
        if (value > DateOnly.FromDateTime(DateTime.Today))
        {
            throw new InvalidOperationException($"{fieldName} cannot be in the future.");
        }

        return value;
    }

    public static string ValidEmail(this string? value, string fieldName)
    {
        var email = value.Required(fieldName);

        var atIndex = email.IndexOf('@');
        var dotIndex = email.LastIndexOf('.');

        if (atIndex <= 0 || dotIndex <= atIndex + 1 || dotIndex == email.Length - 1)
        {
            throw new InvalidOperationException($"{fieldName} must be a valid email address.");
        }

        return email;
    }

    public static string ValidPhone(this string? value, string fieldName)
    {
        var phone = value.Required(fieldName);
        var digitCount = 0;

        for (var index = 0; index < phone.Length; index++)
        {
            if (char.IsDigit(phone[index]))
            {
                digitCount++;
            }
        }

        if (digitCount < 8)
        {
            throw new InvalidOperationException($"{fieldName} must contain at least 8 digits.");
        }

        return phone;
    }
}
