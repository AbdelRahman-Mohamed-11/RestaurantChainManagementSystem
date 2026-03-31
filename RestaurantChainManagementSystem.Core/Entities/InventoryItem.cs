using RestaurantChainManagementSystem.Core.Enums;
using RestaurantChainManagementSystem.Core.Extensions;

namespace RestaurantChainManagementSystem.Core.Entities;

public sealed class InventoryItem
{
    public string IngredientId { get; set; } = string.Empty;
    public string IngredientName { get; set; } = string.Empty;
    public IngredientUnit Unit { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal LowStockThreshold { get; set; }

    public InventoryItem()
    {
    }

    public InventoryItem(
        string ingredientId,
        string ingredientName,
        IngredientUnit unit,
        decimal currentQuantity,
        decimal lowStockThreshold)
    {
        IngredientId = ingredientId.GuidId(nameof(ingredientId));
        IngredientName = ingredientName.Required(nameof(ingredientName));
        Unit = unit;
        CurrentQuantity = currentQuantity.NotNegative(nameof(currentQuantity));
        LowStockThreshold = lowStockThreshold.NotNegative(nameof(lowStockThreshold));
    }

    public bool WillBeLowAfter(decimal amount) => CurrentQuantity - amount <= LowStockThreshold;

    public void Deduct(decimal amount)
    {
        amount.NotNegative(nameof(amount));

        if (CurrentQuantity - amount < 0)
        {
            throw new InvalidOperationException($"Ingredient '{IngredientName}' does not have enough stock.");
        }

        CurrentQuantity -= amount;
    }
}
