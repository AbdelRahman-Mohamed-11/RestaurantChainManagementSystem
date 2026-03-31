using RestaurantChainManagementSystem.Core.Extensions;

namespace RestaurantChainManagementSystem.Core.Entities;

public sealed class OrderItemAddOn
{
    public string AddOnId { get; set; } = string.Empty;
    public string AddOnName { get; set; } = string.Empty;
    public decimal AddOnPrice { get; set; }

    public OrderItemAddOn()
    {
    }

    private OrderItemAddOn(string addOnId, string addOnName, decimal addOnPrice)
    {
        AddOnId = addOnId.GuidId(nameof(addOnId));
        AddOnName = addOnName.Required(nameof(addOnName));
        AddOnPrice = addOnPrice.NotNegative(nameof(addOnPrice));
    }

    public static OrderItemAddOn Create(string addOnId, string addOnName, decimal addOnPrice) =>
        new(addOnId, addOnName, addOnPrice);
}
