using RestaurantChainManagementSystem.Core.Entities;
using RestaurantChainManagementSystem.Core.Enums;
using RestaurantChainManagementSystem.Core.Extensions;
using RestaurantChainManagementSystem.Core.Factories;
using RestaurantChainManagementSystem.Core.Interfaces;
using RestaurantChainManagementSystem.Core.Models;

namespace RestaurantChainManagementSystem.Core.Services;

public sealed class RestaurantApplicationService
{
    private readonly IRestaurantDataStore _dataStore;

    public RestaurantApplicationService(IRestaurantDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public async Task<DashboardSummary> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        var state = await _dataStore.LoadAsync(cancellationToken);
        var pendingOrders = 0;
        var activeDeliveries = 0;

        for (var index = 0; index < state.Orders.Count; index++)
        {
            var order = state.Orders[index];

            if (order.Status == OrderStatus.Pending)
            {
                pendingOrders++;
            }

            if (order.Delivery is not null && order.Delivery.Status == DeliveryStatus.OnTheWay)
            {
                activeDeliveries++;
            }
        }

        return new DashboardSummary(
            state.Branches.Count,
            state.Employees.Count,
            state.Customers.Count,
            state.MenuItems.Count,
            pendingOrders,
            activeDeliveries,
            state.Feedbacks.Count);
    }

    public Task<RestaurantSystemState> GetStateAsync(CancellationToken cancellationToken = default)
    {
        return _dataStore.LoadAsync(cancellationToken);
    }

    public async Task<Customer> RegisterCustomerAsync(
        string fullName,
        string phoneNumber,
        string email,
        CancellationToken cancellationToken = default)
    {
        var state = await _dataStore.LoadAsync(cancellationToken);

        EnsureUniqueCustomer(state, phoneNumber, email);

        var customer = EntityFactory.CreateCustomer(
            NewId(),
            fullName,
            phoneNumber,
            email);

        state.Customers.Add(customer);
        RestaurantStateValidator.Validate(state);
        await _dataStore.SaveAsync(state, cancellationToken);
        return customer;
    }

    public async Task<Order> PlaceOrderAsync(
        OrderPlacementRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var state = await _dataStore.LoadAsync(cancellationToken);
        FindCustomerById(state, request.CustomerId);
        var branch = FindBranchById(state, request.BranchId);
        var staff = FindEmployeeById(state, request.StaffId);

        if (staff.Position is not (EmployeePosition.Waiter or EmployeePosition.Cashier))
        {
            throw new InvalidOperationException("Only waiters and cashiers can place orders.");
        }

        if (!staff.PrimaryBranchId.Equals(branch.Id, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The selected staff member does not belong to the selected branch.");
        }

        var order = EntityFactory.CreateOrder(
            NewId(),
            request.CustomerId,
            request.BranchId,
            request.StaffId,
            request.OrderType);

        foreach (var itemRequest in request.Items)
        {
            var menuItem = FindMenuItemById(state, itemRequest.MenuItemId);
            var branchMenu = FindBranchMenuItemByItemId(branch, itemRequest.MenuItemId);

            if (!branchMenu.IsAvailable)
            {
                throw new InvalidOperationException($"Menu item '{menuItem.Name}' is not available at this branch.");
            }

            var selectedAddOns = new List<OrderItemAddOn>();

            for (var addOnIndex = 0; addOnIndex < itemRequest.AddOnIds.Count; addOnIndex++)
            {
                var addOn = FindAddOnById(menuItem, itemRequest.AddOnIds[addOnIndex]);
                selectedAddOns.Add(OrderItemAddOn.Create(addOn.Id, addOn.Name, addOn.ExtraPrice));
            }

            var orderItem = OrderItem.Create(
                menuItem.Id,
                menuItem.Name,
                itemRequest.Quantity,
                branchMenu.ResolvePrice(menuItem.BasePrice),
                itemRequest.SpecialNotes ?? string.Empty,
                selectedAddOns);

            order.AddItem(orderItem);
        }

        order.EnsureHasItems();

        if (request.OrderType == OrderType.Delivery)
        {
            var address = request.DeliveryAddress.Required(nameof(request.DeliveryAddress));
            order.AttachDelivery(DeliveryRecord.Create(
                NewId(),
                address));
        }

        state.Orders.Add(order);
        RestaurantStateValidator.Validate(state);
        await _dataStore.SaveAsync(state, cancellationToken);
        return order;
    }

    public async Task<Order> MoveOrderToPreparingAsync(
        string orderId,
        string actorEmployeeId,
        string? managerOverrideEmployeeId,
        CancellationToken cancellationToken = default)
    {
        var state = await _dataStore.LoadAsync(cancellationToken);
        var order = FindOrderById(state, orderId);
        var branch = FindBranchById(state, order.BranchId);
        var actor = FindEmployeeById(state, actorEmployeeId);

        if (actor.Position != EmployeePosition.Chef)
        {
            throw new InvalidOperationException("Only chefs can move orders to preparing.");
        }

        if (!actor.PrimaryBranchId.Equals(branch.Id, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The chef must belong to the same branch as the order.");
        }

        var requirements = BuildInventoryRequirements(state, order);
        var shortages = new List<string>();

        foreach (var pair in requirements)
        {
            var stock = FindInventoryItemByIngredientId(branch, pair.Key);

            if (stock.CurrentQuantity < pair.Value)
            {
                shortages.Add(stock.IngredientName);
            }
        }

        if (shortages.Count > 0)
        {
            if (string.IsNullOrWhiteSpace(managerOverrideEmployeeId))
            {
                throw new InvalidOperationException(
                    $"Insufficient inventory for: {string.Join(", ", shortages)}. Manager override is required.");
            }

            var manager = FindEmployeeById(state, managerOverrideEmployeeId);

            if (manager.Position != EmployeePosition.BranchManager ||
                !manager.PrimaryBranchId.Equals(branch.Id, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Manager override must be approved by the branch manager of the same branch.");
            }
        }

        foreach (var requirement in requirements)
        {
            var stock = FindInventoryItemByIngredientId(branch, requirement.Key);
            stock.Deduct(requirement.Value);
        }

        order.MarkPreparing();
        RestaurantStateValidator.Validate(state);
        await _dataStore.SaveAsync(state, cancellationToken);
        return order;
    }

    public async Task<Order> MarkOrderServedAsync(
        string orderId,
        string actorEmployeeId,
        CancellationToken cancellationToken = default)
    {
        var state = await _dataStore.LoadAsync(cancellationToken);
        var order = FindOrderById(state, orderId);
        var actor = FindEmployeeById(state, actorEmployeeId);

        if (actor.Position != EmployeePosition.Chef)
        {
            throw new InvalidOperationException("Only chefs can mark orders as served.");
        }

        if (!actor.PrimaryBranchId.Equals(order.BranchId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The chef must belong to the same branch as the order.");
        }

        order.MarkServed();
        RestaurantStateValidator.Validate(state);
        await _dataStore.SaveAsync(state, cancellationToken);
        return order;
    }

    public async Task<Order> ProcessPaymentAsync(
        string orderId,
        PaymentMethod paymentMethod,
        string actorEmployeeId,
        CancellationToken cancellationToken = default)
    {
        var state = await _dataStore.LoadAsync(cancellationToken);
        var order = FindOrderById(state, orderId);
        var customer = FindCustomerById(state, order.CustomerId);
        var actor = FindEmployeeById(state, actorEmployeeId);

        if (actor.Position != EmployeePosition.Cashier)
        {
            throw new InvalidOperationException("Only cashiers can process payments.");
        }

        if (!actor.PrimaryBranchId.Equals(order.BranchId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The cashier must belong to the same branch as the order.");
        }

        order.Complete(paymentMethod);
        customer.AddLoyaltyPoints(order.TotalAmount);

        RestaurantStateValidator.Validate(state);
        await _dataStore.SaveAsync(state, cancellationToken);
        return order;
    }

    public async Task<Order> CancelOrderAsync(
        string orderId,
        string reason,
        string actorEmployeeId,
        CancellationToken cancellationToken = default)
    {
        var state = await _dataStore.LoadAsync(cancellationToken);
        var order = FindOrderById(state, orderId);
        var actor = FindEmployeeById(state, actorEmployeeId);

        if (!actor.PrimaryBranchId.Equals(order.BranchId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The employee must belong to the same branch as the order.");
        }

        if (actor.Position == EmployeePosition.Waiter || actor.Position == EmployeePosition.Cashier)
        {
            if (order.Status != OrderStatus.Pending)
            {
                throw new InvalidOperationException("Waiters and cashiers can cancel pending orders only.");
            }
        }
        else if (actor.Position == EmployeePosition.BranchManager)
        {
            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException("Completed or cancelled orders cannot be cancelled again.");
            }

            if (order.Delivery is not null && order.Delivery.Status == DeliveryStatus.OnTheWay)
            {
                throw new InvalidOperationException("Orders already out for delivery cannot be cancelled from the menu.");
            }
        }
        else
        {
            throw new InvalidOperationException("Only waiter, cashier, or branch manager can cancel orders.");
        }

        if (order.Delivery is not null &&
            !string.IsNullOrWhiteSpace(order.Delivery.DeliveryStaffId))
        {
            var deliveryEmployee = FindEmployeeById(state, order.Delivery.DeliveryStaffId);

            if (deliveryEmployee.DeliveryProfile is not null)
            {
                deliveryEmployee.DeliveryProfile.MarkAvailable();
            }
        }

        order.Cancel(reason);
        RestaurantStateValidator.Validate(state);
        await _dataStore.SaveAsync(state, cancellationToken);
        return order;
    }

    public async Task<DeliveryRecord> AssignDeliveryAsync(
        string orderId,
        string staffId,
        string actorEmployeeId,
        CancellationToken cancellationToken = default)
    {
        var state = await _dataStore.LoadAsync(cancellationToken);
        var order = FindOrderById(state, orderId);
        var actor = FindEmployeeById(state, actorEmployeeId);

        if (order.Delivery is null)
        {
            throw new InvalidOperationException("This order does not have a delivery record.");
        }

        if (order.Status != OrderStatus.Served)
        {
            throw new InvalidOperationException("Delivery staff can only be assigned after the order is served and ready for dispatch.");
        }

        if (actor.Position is not (EmployeePosition.Waiter or EmployeePosition.Cashier or EmployeePosition.BranchManager))
        {
            throw new InvalidOperationException("Only waiter, cashier, or branch manager can assign delivery staff.");
        }

        var employee = FindEmployeeById(state, staffId);

        if (employee.DeliveryProfile is null)
        {
            throw new InvalidOperationException("The selected employee is not a delivery staff member.");
        }

        if (!employee.DeliveryProfile.IsAvailable)
        {
            throw new InvalidOperationException("The selected delivery staff member is not available.");
        }

        if (!employee.PrimaryBranchId.Equals(order.BranchId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The delivery staff member must belong to the same branch as the order.");
        }

        employee.DeliveryProfile.MarkAssigned();
        order.Delivery.AssignStaff(employee.Id);

        RestaurantStateValidator.Validate(state);
        await _dataStore.SaveAsync(state, cancellationToken);
        return order.Delivery;
    }

    public async Task<DeliveryRecord> UpdateDeliveryStatusAsync(
        string orderId,
        DeliveryStatus status,
        string? reason,
        string actorEmployeeId,
        CancellationToken cancellationToken = default)
    {
        var state = await _dataStore.LoadAsync(cancellationToken);
        var order = FindOrderById(state, orderId);

        if (order.Delivery is null)
        {
            throw new InvalidOperationException("This order does not have a delivery record.");
        }

        if (string.IsNullOrWhiteSpace(order.Delivery.DeliveryStaffId) ||
            !order.Delivery.DeliveryStaffId.Equals(actorEmployeeId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only the assigned delivery staff member can update this delivery.");
        }

        if (!string.IsNullOrWhiteSpace(order.Delivery.DeliveryStaffId))
        {
            var employee = FindEmployeeById(state, order.Delivery.DeliveryStaffId!);
            employee.DeliveryProfile?.MarkAvailable();
        }

        if (status == DeliveryStatus.Delivered)
        {
            if (order.Status != OrderStatus.Served)
            {
                throw new InvalidOperationException("Only served delivery orders can be marked as delivered.");
            }

            order.Delivery.MarkDelivered();
            order.MarkCompletedWithoutPayment();
        }
        else if (status == DeliveryStatus.Failed)
        {
            order.Delivery.MarkFailed(reason ?? string.Empty);
            order.Cancel("Delivery failed.");
        }
        else
        {
            throw new InvalidOperationException("Only Delivered and Failed statuses can be set manually.");
        }

        RestaurantStateValidator.Validate(state);
        await _dataStore.SaveAsync(state, cancellationToken);
        return order.Delivery;
    }

    public async Task<Feedback> SubmitFeedbackAsync(
        string customerId,
        string orderId,
        int rating,
        string comments,
        CancellationToken cancellationToken = default)
    {
        var state = await _dataStore.LoadAsync(cancellationToken);
        var order = FindOrderById(state, orderId);

        if (!order.CustomerId.Equals(customerId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The order does not belong to the selected customer.");
        }

        if (order.Status != OrderStatus.Completed)
        {
            throw new InvalidOperationException("Feedback can only be submitted for completed orders.");
        }

        for (var index = 0; index < state.Feedbacks.Count; index++)
        {
            var feedbackItem = state.Feedbacks[index];

            if (feedbackItem.OrderId.Equals(orderId, StringComparison.OrdinalIgnoreCase) &&
                feedbackItem.CustomerId.Equals(customerId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Feedback already exists for this order.");
            }
        }

        var feedback = EntityFactory.CreateFeedback(
            NewId(),
            customerId,
            orderId,
            rating,
            comments);

        state.Feedbacks.Add(feedback);
        RestaurantStateValidator.Validate(state);
        await _dataStore.SaveAsync(state, cancellationToken);
        return feedback;
    }

    public async Task ResetAsync(RestaurantSystemState state, CancellationToken cancellationToken = default)
    {
        RestaurantStateValidator.Validate(state);
        await _dataStore.SaveAsync(state, cancellationToken);
    }

    private static void EnsureUniqueCustomer(RestaurantSystemState state, string phoneNumber, string email)
    {
        for (var index = 0; index < state.Customers.Count; index++)
        {
            if (state.Customers[index].PhoneNumber.Equals(phoneNumber, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Customer phone number must be unique.");
            }
        }

        for (var index = 0; index < state.Customers.Count; index++)
        {
            if (state.Customers[index].Email.Equals(email, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Customer email must be unique.");
            }
        }
    }

    private static Dictionary<string, decimal> BuildInventoryRequirements(RestaurantSystemState state, Order order)
    {
        var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        foreach (var orderItem in order.Items)
        {
            var menuItem = FindMenuItemById(state, orderItem.ItemId);

            foreach (var recipeItem in menuItem.Recipe)
            {
                var requiredQuantity = recipeItem.QuantityPerServing * orderItem.Quantity;

                if (result.TryGetValue(recipeItem.IngredientId, out var existing))
                {
                    result[recipeItem.IngredientId] = existing + requiredQuantity;
                }
                else
                {
                    result[recipeItem.IngredientId] = requiredQuantity;
                }
            }
        }

        return result;
    }

    private static string NewId()
    {
        return Guid.NewGuid().ToString();
    }

    private static Branch FindBranchById(RestaurantSystemState state, string branchId)
    {
        for (var index = 0; index < state.Branches.Count; index++)
        {
            if (state.Branches[index].Id.Equals(branchId, StringComparison.OrdinalIgnoreCase))
            {
                return state.Branches[index];
            }
        }

        throw new InvalidOperationException($"Branch '{branchId}' was not found.");
    }

    private static Employee FindEmployeeById(RestaurantSystemState state, string employeeId)
    {
        for (var index = 0; index < state.Employees.Count; index++)
        {
            if (state.Employees[index].Id.Equals(employeeId, StringComparison.OrdinalIgnoreCase))
            {
                return state.Employees[index];
            }
        }

        throw new InvalidOperationException($"Employee '{employeeId}' was not found.");
    }

    private static Customer FindCustomerById(RestaurantSystemState state, string customerId)
    {
        for (var index = 0; index < state.Customers.Count; index++)
        {
            if (state.Customers[index].Id.Equals(customerId, StringComparison.OrdinalIgnoreCase))
            {
                return state.Customers[index];
            }
        }

        throw new InvalidOperationException($"Customer '{customerId}' was not found.");
    }

    private static MenuItem FindMenuItemById(RestaurantSystemState state, string menuItemId)
    {
        for (var index = 0; index < state.MenuItems.Count; index++)
        {
            if (state.MenuItems[index].Id.Equals(menuItemId, StringComparison.OrdinalIgnoreCase))
            {
                return state.MenuItems[index];
            }
        }

        throw new InvalidOperationException($"Menu item '{menuItemId}' was not found.");
    }

    private static BranchMenuItem FindBranchMenuItemByItemId(Branch branch, string menuItemId)
    {
        for (var index = 0; index < branch.MenuItems.Count; index++)
        {
            if (branch.MenuItems[index].ItemId.Equals(menuItemId, StringComparison.OrdinalIgnoreCase))
            {
                return branch.MenuItems[index];
            }
        }

        throw new InvalidOperationException($"Branch menu item '{menuItemId}' was not found.");
    }

    private static AddOn FindAddOnById(MenuItem menuItem, string addOnId)
    {
        for (var index = 0; index < menuItem.AddOns.Count; index++)
        {
            if (menuItem.AddOns[index].Id.Equals(addOnId, StringComparison.OrdinalIgnoreCase))
            {
                return menuItem.AddOns[index];
            }
        }

        throw new InvalidOperationException($"Add-on '{addOnId}' was not found.");
    }

    private static InventoryItem FindInventoryItemByIngredientId(Branch branch, string ingredientId)
    {
        for (var index = 0; index < branch.InventoryItems.Count; index++)
        {
            if (branch.InventoryItems[index].IngredientId.Equals(ingredientId, StringComparison.OrdinalIgnoreCase))
            {
                return branch.InventoryItems[index];
            }
        }

        throw new InvalidOperationException($"Inventory item '{ingredientId}' was not found.");
    }

    private static Order FindOrderById(RestaurantSystemState state, string orderId)
    {
        for (var index = 0; index < state.Orders.Count; index++)
        {
            if (state.Orders[index].Id.Equals(orderId, StringComparison.OrdinalIgnoreCase))
            {
                return state.Orders[index];
            }
        }

        throw new InvalidOperationException($"Order '{orderId}' was not found.");
    }
}
