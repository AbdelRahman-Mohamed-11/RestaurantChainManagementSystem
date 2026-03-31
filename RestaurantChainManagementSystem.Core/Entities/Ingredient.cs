using RestaurantChainManagementSystem.Core.Enums;
using RestaurantChainManagementSystem.Core.Extensions;

namespace RestaurantChainManagementSystem.Core.Entities;

public sealed class Ingredient
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public IngredientUnit Unit { get; set; }

    public Ingredient()
    {
    }

    public Ingredient(string id, string name, IngredientUnit unit)
    {
        Id = id.GuidId(nameof(id));
        Name = name.Required(nameof(name));
        Unit = unit;
    }
}
