using RestaurantChainManagementSystem.Core.Enums;
using RestaurantChainManagementSystem.Core.Extensions;

namespace RestaurantChainManagementSystem.Core.Entities;

public sealed class DeliveryRecord
{
    public string Id { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? DeliveryStaffId { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }
    public DeliveryStatus Status { get; set; }
    public string? FailureReason { get; set; }

    public DeliveryRecord()
    {
    }

    private DeliveryRecord(string id, string deliveryAddress)
    {
        Id = id.GuidId(nameof(id));
        DeliveryAddress = deliveryAddress.Required(nameof(deliveryAddress));
        Status = DeliveryStatus.AwaitingAssignment;
    }

    public static DeliveryRecord Create(string id, string deliveryAddress) => new(id, deliveryAddress);

    public void AssignStaff(string staffId)
    {
        DeliveryStaffId = staffId.GuidId(nameof(staffId));
        Status = DeliveryStatus.OnTheWay;
    }

    public void MarkDelivered()
    {
        Status = DeliveryStatus.Delivered;
        DeliveredAtUtc = DateTime.UtcNow;
        FailureReason = null;
    }

    public void MarkFailed(string reason)
    {
        FailureReason = reason.Required(nameof(reason));
        Status = DeliveryStatus.Failed;
    }
}
