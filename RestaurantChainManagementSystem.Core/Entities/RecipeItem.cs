using RestaurantChainManagementSystem.Core.Enums;
using RestaurantChainManagementSystem.Core.Extensions;

namespace RestaurantChainManagementSystem.Core.Entities;

public sealed class RecipeItem
{
    public string IngredientId { get; set; } = string.Empty;
    public string IngredientName { get; set; } = string.Empty;
    public IngredientUnit Unit { get; set; }
    public decimal QuantityPerServing { get; set; }

    public RecipeItem()
    {
    }

    public RecipeItem(string ingredientId, string ingredientName, IngredientUnit unit, decimal quantityPerServing)
    {
        IngredientId = ingredientId.GuidId(nameof(ingredientId));
        IngredientName = ingredientName.Required(nameof(ingredientName));
        Unit = unit;
        QuantityPerServing = quantityPerServing.Positive(nameof(quantityPerServing));
    }
}
