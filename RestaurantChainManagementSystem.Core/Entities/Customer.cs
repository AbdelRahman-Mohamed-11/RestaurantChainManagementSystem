using RestaurantChainManagementSystem.Core.Extensions;

namespace RestaurantChainManagementSystem.Core.Entities;

public sealed class Customer
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int LoyaltyPoints { get; set; }

    public Customer()
    {
    }

    private Customer(string id, string fullName, string phoneNumber, string email)
    {
        Id = id.GuidId(nameof(id));
        FullName = fullName.Required(nameof(fullName));
        PhoneNumber = phoneNumber.ValidPhone(nameof(phoneNumber));
        Email = email.ValidEmail(nameof(email));
        LoyaltyPoints = 0;
    }

    public static Customer Create(string id, string fullName, string phoneNumber, string email) =>
        new(id, fullName, phoneNumber, email);

    public void AddLoyaltyPoints(decimal totalAmount)
    {
        LoyaltyPoints += (int)Math.Floor(totalAmount);
    }
}
