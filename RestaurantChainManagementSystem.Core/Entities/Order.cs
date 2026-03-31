using RestaurantChainManagementSystem.Core.Enums;
using RestaurantChainManagementSystem.Core.Extensions;

namespace RestaurantChainManagementSystem.Core.Entities;

public sealed class Order
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string StaffId { get; set; } = string.Empty;
    public OrderType Type { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? CancellationReason { get; set; }
    public List<OrderItem> Items { get; set; } = [];
    public DeliveryRecord? Delivery { get; set; }
    public Order()
    {
    }

    private Order(string id, string customerId, string branchId, string staffId, OrderType type)
    {
        Id = id.GuidId(nameof(id));
        CustomerId = customerId.GuidId(nameof(customerId));
        BranchId = branchId.GuidId(nameof(branchId));
        StaffId = staffId.GuidId(nameof(staffId));
        Type = type;
        CreatedAtUtc = DateTime.UtcNow;
        Status = OrderStatus.Pending;
    }

    public static Order Create(string id, string customerId, string branchId, string staffId, OrderType type)
    {
        return new Order(id, customerId, branchId, staffId, type);
    }

    public decimal TotalAmount
    {
        get
        {
            decimal total = 0;

            for (var index = 0; index < Items.Count; index++)
            {
                total += Items[index].Total;
            }

            return total;
        }
    }

    public void AddItem(OrderItem item)
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Items can only be added while the order is pending.");
        }

        Items.Add(item ?? throw new ArgumentNullException(nameof(item)));
    }

    public void AttachDelivery(DeliveryRecord delivery)
    {
        if (Type != OrderType.Delivery)
        {
            throw new InvalidOperationException("Only delivery orders can have a delivery record.");
        }

        Delivery = delivery;
    }

    public void EnsureHasItems()
    {
        if (Items.Count == 0)
        {
            throw new InvalidOperationException("An order must contain at least one item.");
        }
    }

    public void MarkPreparing()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Only pending orders can move to preparing.");
        }

        Status = OrderStatus.Preparing;
    }

    public void MarkServed()
    {
        if (Status != OrderStatus.Preparing)
        {
            throw new InvalidOperationException("Only preparing orders can move to served.");
        }

        Status = OrderStatus.Served;
    }

    public void MarkCompletedWithoutPayment()
    {
        if (Status != OrderStatus.Served)
        {
            throw new InvalidOperationException("Only served orders can be completed.");
        }

        Status = OrderStatus.Completed;
    }

    public void Complete(PaymentMethod paymentMethod)
    {
        if (Status != OrderStatus.Served)
        {
            throw new InvalidOperationException("Payment can only be processed for served orders.");
        }

        PaymentMethod = paymentMethod;
        Status = OrderStatus.Completed;
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Completed)
        {
            throw new InvalidOperationException("Completed orders cannot be cancelled.");
        }

        CancellationReason = reason.Required(nameof(reason));
        Status = OrderStatus.Cancelled;
    }
}
