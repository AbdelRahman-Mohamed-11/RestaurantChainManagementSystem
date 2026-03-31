using RestaurantChainManagementSystem.Core.Extensions;

namespace RestaurantChainManagementSystem.Core.Entities;

public sealed class Feedback
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public DateTime SubmittedAtUtc { get; set; }
    public int Rating { get; set; }
    public string Comments { get; set; } = string.Empty;

    public Feedback()
    {
    }

    private Feedback(string id, string customerId, string orderId, int rating, string comments)
    {
        if (rating is < 1 or > 5)
        {
            throw new InvalidOperationException("Rating must be between 1 and 5.");
        }

        Id = id.GuidId(nameof(id));
        CustomerId = customerId.GuidId(nameof(customerId));
        OrderId = orderId.GuidId(nameof(orderId));
        Rating = rating;
        Comments = comments?.Trim() ?? string.Empty;
        SubmittedAtUtc = DateTime.UtcNow;
    }

    public static Feedback Create(string id, string customerId, string orderId, int rating, string comments) =>
        new(id, customerId, orderId, rating, comments);
}
