using RestaurantChainManagementSystem.Core.Extensions;

namespace RestaurantChainManagementSystem.Core.Entities;

public sealed class OrderItem
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string SpecialNotes { get; set; } = string.Empty;
    public List<OrderItemAddOn> SelectedAddOns { get; set; } = [];

    public OrderItem()
    {
    }

    private OrderItem(
        string itemId,
        string itemName,
        int quantity,
        decimal unitPrice,
        string specialNotes,
        List<OrderItemAddOn> selectedAddOns)
    {
        ItemId = itemId.GuidId(nameof(itemId));
        ItemName = itemName.Required(nameof(itemName));
        Quantity = quantity.Positive(nameof(quantity));
        UnitPrice = unitPrice.Positive(nameof(unitPrice));
        SpecialNotes = specialNotes?.Trim() ?? string.Empty;
        SelectedAddOns = selectedAddOns;
    }

    public static OrderItem Create(
        string itemId,
        string itemName,
        int quantity,
        decimal unitPrice,
        string specialNotes,
        List<OrderItemAddOn>? selectedAddOns = null)
    {
        return new OrderItem(itemId, itemName, quantity, unitPrice, specialNotes, selectedAddOns ?? []);
    }

    public decimal Total
    {
        get
        {
            decimal addOnTotal = 0;

            for (var index = 0; index < SelectedAddOns.Count; index++)
            {
                addOnTotal += SelectedAddOns[index].AddOnPrice;
            }

            return (UnitPrice + addOnTotal) * Quantity;
        }
    }
}
