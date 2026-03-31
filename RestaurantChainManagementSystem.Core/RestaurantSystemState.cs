using RestaurantChainManagementSystem.Core.Entities;

namespace RestaurantChainManagementSystem.Core;

public sealed class RestaurantSystemState
{
    public List<Branch> Branches { get; set; } = [];
    public List<Employee> Employees { get; set; } = [];
    public List<Customer> Customers { get; set; } = [];
    public List<MenuItem> MenuItems { get; set; } = [];
    public List<Ingredient> Ingredients { get; set; } = [];
    public List<ShiftSchedule> ShiftSchedules { get; set; } = [];
    public List<Order> Orders { get; set; } = [];
    public List<Feedback> Feedbacks { get; set; } = [];
}
