using RestaurantChainManagementSystem.Core.Enums;

namespace RestaurantChainManagementSystem.Core.Models;

public sealed class OrderPlacementRequest
{
    public required string CustomerId { get; init; }
    public required string BranchId { get; init; }
    public required string StaffId { get; init; }
    public required OrderType OrderType { get; init; }
    public string? DeliveryAddress { get; init; }
    public required List<OrderPlacementItem> Items { get; init; }
}

public sealed class OrderPlacementItem
{
    public required string MenuItemId { get; init; }
    public required int Quantity { get; init; }
    public string? SpecialNotes { get; init; }
    public List<string> AddOnIds { get; init; } = [];
}
