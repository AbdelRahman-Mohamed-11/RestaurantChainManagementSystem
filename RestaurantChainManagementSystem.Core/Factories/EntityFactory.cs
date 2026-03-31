using RestaurantChainManagementSystem.Core.Entities;
using RestaurantChainManagementSystem.Core.Enums;

namespace RestaurantChainManagementSystem.Core.Factories;

public static class EntityFactory
{
    public static Branch CreateBranch(
        string code,
        string name,
        string address,
        string contactNumber,
        string openingHours,
        string managerEmployeeId) =>
        Branch.Create(code, name, address, contactNumber, openingHours, managerEmployeeId);

    public static Employee CreateEmployee(
        string code,
        string fullName,
        EmployeePosition position,
        decimal salary,
        DateOnly hireDate,
        string phoneNumber,
        string email,
        string branchId) =>
        Employee.Create(code, fullName, position, salary, hireDate, phoneNumber, email, branchId);

    public static Customer CreateCustomer(string code, string fullName, string phoneNumber, string email) =>
        Customer.Create(code, fullName, phoneNumber, email);

    public static MenuItem CreateMenuItem(
        string code,
        string name,
        string description,
        decimal basePrice,
        MenuCategory category) =>
        MenuItem.Create(code, name, description, basePrice, category);

    public static Order CreateOrder(
        string code,
        string customerId,
        string branchId,
        string staffId,
        OrderType orderType) =>
        Order.Create(code, customerId, branchId, staffId, orderType);

    public static Feedback CreateFeedback(
        string code,
        string customerId,
        string orderId,
        int rating,
        string comments) =>
        Feedback.Create(code, customerId, orderId, rating, comments);
}
