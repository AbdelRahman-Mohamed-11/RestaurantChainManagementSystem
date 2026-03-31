using RestaurantChainManagementSystem.Core.Extensions;

namespace RestaurantChainManagementSystem.Core.Entities;

public sealed class AddOn
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal ExtraPrice { get; set; }

    public AddOn()
    {
    }

    public AddOn(string id, string name, decimal extraPrice)
    {
        Id = id.GuidId(nameof(id));
        Name = name.Required(nameof(name));
        ExtraPrice = extraPrice.NotNegative(nameof(extraPrice));
    }
}
