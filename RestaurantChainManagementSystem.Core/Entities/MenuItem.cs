using RestaurantChainManagementSystem.Core.Enums;
using RestaurantChainManagementSystem.Core.Extensions;

namespace RestaurantChainManagementSystem.Core.Entities;

public sealed class MenuItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public MenuCategory Category { get; set; }
    public List<AddOn> AddOns { get; set; } = [];
    public List<RecipeItem> Recipe { get; set; } = [];

    public MenuItem()
    {
    }

    private MenuItem(string id, string name, string description, decimal basePrice, MenuCategory category)
    {
        Id = id.GuidId(nameof(id));
        Name = name.Required(nameof(name));
        Description = description?.Trim() ?? string.Empty;
        BasePrice = basePrice.Positive(nameof(basePrice));
        Category = category;
    }

    public static MenuItem Create(
        string id,
        string name,
        string description,
        decimal basePrice,
        MenuCategory category) =>
        new(id, name, description, basePrice, category);
}
