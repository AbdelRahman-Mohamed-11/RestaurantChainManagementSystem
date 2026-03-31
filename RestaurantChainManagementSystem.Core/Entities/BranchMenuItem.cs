namespace RestaurantChainManagementSystem.Core.Entities;

public sealed class BranchMenuItem
{
    public string ItemId { get; set; } = string.Empty;
    public decimal? OverridePrice { get; set; }
    public bool IsAvailable { get; set; }

    public BranchMenuItem()
    {
    }

    public BranchMenuItem(string itemId, decimal? overridePrice, bool isAvailable)
    {
        ItemId = string.IsNullOrWhiteSpace(itemId)
            ? throw new InvalidOperationException("ItemId is required.")
            : itemId.Trim();

        if (!Guid.TryParse(ItemId, out _))
        {
            throw new InvalidOperationException("ItemId must be a valid GUID.");
        }

        if (overridePrice is <= 0)
        {
            throw new InvalidOperationException("Override price must be greater than zero when supplied.");
        }

        OverridePrice = overridePrice;
        IsAvailable = isAvailable;
    }

    public decimal ResolvePrice(decimal basePrice) => OverridePrice ?? basePrice;

    public void SetAvailability(bool isAvailable) => IsAvailable = isAvailable;
}
