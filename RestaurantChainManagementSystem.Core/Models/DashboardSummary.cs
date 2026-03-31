namespace RestaurantChainManagementSystem.Core.Models;

public sealed record DashboardSummary(
    int Branches,
    int Employees,
    int Customers,
    int MenuItems,
    int PendingOrders,
    int ActiveDeliveries,
    int FeedbackCount);
