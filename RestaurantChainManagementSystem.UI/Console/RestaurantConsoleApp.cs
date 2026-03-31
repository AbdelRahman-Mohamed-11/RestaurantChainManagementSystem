using RestaurantChainManagementSystem.Core;
using RestaurantChainManagementSystem.Core.Entities;
using RestaurantChainManagementSystem.Core.Enums;
using RestaurantChainManagementSystem.Core.Models;
using RestaurantChainManagementSystem.Core.Services;
using RestaurantChainManagementSystem.Infrastructure.Seed;

namespace RestaurantChainManagementSystem.UI.ConsoleUi;

public sealed class RestaurantConsoleApp
{
    private readonly RestaurantApplicationService _service;

    public RestaurantConsoleApp(RestaurantApplicationService service)
    {
        _service = service;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            SafeClear();
            var employee = await LoginAsync();

            if (employee is null)
            {
                return;
            }

            await RunRoleMenuAsync(employee);
        }
    }

    private static void SafeClear()
    {
        try
        {
            if (!Console.IsOutputRedirected)
            {
                Console.Clear();
            }
        }
        catch (IOException)
        {
        }
    }

    private async Task ShowDashboardAsync(Employee employee)
    {
        var summary = await _service.GetDashboardSummaryAsync();

        Theme.Header("Restaurant Chain Management System");
        Console.WriteLine($"Logged in as    : {employee.FullName} ({employee.Position})");
        Console.WriteLine($"Branches        : {summary.Branches}");
        Console.WriteLine($"Employees       : {summary.Employees}");
        Console.WriteLine($"Customers       : {summary.Customers}");
        Console.WriteLine($"Menu Items      : {summary.MenuItems}");
        Console.WriteLine($"Pending Orders  : {summary.PendingOrders}");
        Console.WriteLine($"Active Delivery : {summary.ActiveDeliveries}");
        Console.WriteLine($"Feedback Count  : {summary.FeedbackCount}");
        Console.WriteLine();
    }

    private async Task<Employee?> LoginAsync()
    {
        Theme.Header("Login");
        var state = await _service.GetStateAsync();

        foreach (var listedEmployee in state.Employees)
        {
            Console.WriteLine($"{listedEmployee.Id} | {listedEmployee.FullName} | {listedEmployee.Position}");
        }

        Console.WriteLine();
        Console.WriteLine("0. Exit");
        Console.WriteLine();

        var employeeId = Prompt.Required("Enter employee ID or 0");

        if (employeeId == "0")
        {
            return null;
        }

        var employee = FindEmployeeById(state.Employees, employeeId);

        if (employee is null)
        {
            Theme.Warning("Employee ID not found.");
            Theme.Warning(Environment.NewLine + "Press any key to continue...");
            WaitForContinue();
        }

        return employee;
    }

    private async Task RunRoleMenuAsync(Employee employee)
    {
        while (true)
        {
            SafeClear();
            await ShowDashboardAsync(employee);
            ShowRoleMenu(employee.Position);

            var choice = Prompt.Int("Choose an option", 0, GetMaxOption(employee.Position));
            SafeClear();

            try
            {
                if (choice == 0)
                {
                    return;
                }

                await ExecuteRoleActionAsync(employee, choice);
            }
            catch (Exception exception)
            {
                Theme.Error(exception.Message);
            }

            Theme.Warning(Environment.NewLine + "Press any key to continue...");
            WaitForContinue();
        }
    }

    private static void WaitForContinue()
    {
        try
        {
            if (Console.IsInputRedirected)
            {
                Console.ReadLine();
                return;
            }

            Console.ReadKey(true);
        }
        catch (InvalidOperationException)
        {
            Console.ReadLine();
        }
    }

    private static void ShowRoleMenu(EmployeePosition role)
    {
        switch (role)
        {
            case EmployeePosition.BranchManager:
                Console.WriteLine("1. View branches");
                Console.WriteLine("2. View employees and shifts");
                Console.WriteLine("3. View customers");
                Console.WriteLine("4. Register customer");
                Console.WriteLine("5. View menu");
                Console.WriteLine("6. Cancel order");
                Console.WriteLine("7. Assign delivery staff");
                Console.WriteLine("8. Inventory and feedback");
                Console.WriteLine("9. Reset seed data");
                break;
            case EmployeePosition.Waiter:
                Console.WriteLine("1. View customers");
                Console.WriteLine("2. Register customer");
                Console.WriteLine("3. View menu");
                Console.WriteLine("4. Place order");
                Console.WriteLine("5. Cancel pending order");
                Console.WriteLine("6. Assign delivery staff");
                Console.WriteLine("7. Inventory and feedback");
                break;
            case EmployeePosition.Cashier:
                Console.WriteLine("1. View customers");
                Console.WriteLine("2. Register customer");
                Console.WriteLine("3. View menu");
                Console.WriteLine("4. Place order");
                Console.WriteLine("5. Process payment");
                Console.WriteLine("6. Cancel pending order");
                Console.WriteLine("7. Assign delivery staff");
                break;
            case EmployeePosition.Chef:
                Console.WriteLine("1. View menu");
                Console.WriteLine("2. Update kitchen status");
                Console.WriteLine("3. View inventory");
                break;
            case EmployeePosition.DeliveryStaff:
                Console.WriteLine("1. View my delivery jobs");
                Console.WriteLine("2. Mark delivery as delivered");
                Console.WriteLine("3. Mark delivery as failed");
                break;
        }

        Console.WriteLine("0. Logout");
        Console.WriteLine();
    }

    private static int GetMaxOption(EmployeePosition role)
    {
        switch (role)
        {
            case EmployeePosition.BranchManager:
                return 9;
            case EmployeePosition.Waiter:
                return 7;
            case EmployeePosition.Cashier:
                return 7;
            case EmployeePosition.Chef:
                return 3;
            case EmployeePosition.DeliveryStaff:
                return 3;
            default:
                return 0;
        }
    }

    private async Task ExecuteRoleActionAsync(Employee employee, int choice)
    {
        switch (employee.Position)
        {
            case EmployeePosition.BranchManager:
                await ExecuteManagerActionAsync(employee, choice);
                break;
            case EmployeePosition.Waiter:
                await ExecuteWaiterActionAsync(employee, choice);
                break;
            case EmployeePosition.Cashier:
                await ExecuteCashierActionAsync(employee, choice);
                break;
            case EmployeePosition.Chef:
                await ExecuteChefActionAsync(employee, choice);
                break;
            case EmployeePosition.DeliveryStaff:
                await ExecuteDeliveryActionAsync(employee, choice);
                break;
        }
    }

    private async Task ExecuteManagerActionAsync(Employee employee, int choice)
    {
        switch (choice)
        {
            case 1:
                await ShowBranchesAsync();
                break;
            case 2:
                await ShowEmployeesAndShiftsAsync();
                break;
            case 3:
                await ShowCustomersAsync();
                break;
            case 4:
                await RegisterCustomerAsync();
                break;
            case 5:
                await ShowMenuAsync();
                break;
            case 6:
                await CancelOrderAsync(employee);
                break;
            case 7:
                await AssignDeliveryStaffAsync(employee);
                break;
            case 8:
                await ShowInventoryAndFeedbackAsync();
                break;
            case 9:
                await ResetSeedAsync();
                break;
        }
    }

    private async Task ExecuteWaiterActionAsync(Employee employee, int choice)
    {
        switch (choice)
        {
            case 1:
                await ShowCustomersAsync();
                break;
            case 2:
                await RegisterCustomerAsync();
                break;
            case 3:
                await ShowMenuAsync();
                break;
            case 4:
                await PlaceOrderAsync(employee);
                break;
            case 5:
                await CancelOrderAsync(employee);
                break;
            case 6:
                await AssignDeliveryStaffAsync(employee);
                break;
            case 7:
                await ShowInventoryAndFeedbackAsync();
                break;
        }
    }

    private async Task ExecuteCashierActionAsync(Employee employee, int choice)
    {
        switch (choice)
        {
            case 1:
                await ShowCustomersAsync();
                break;
            case 2:
                await RegisterCustomerAsync();
                break;
            case 3:
                await ShowMenuAsync();
                break;
            case 4:
                await PlaceOrderAsync(employee);
                break;
            case 5:
                await ProcessPaymentAsync(employee);
                break;
            case 6:
                await CancelOrderAsync(employee);
                break;
            case 7:
                await AssignDeliveryStaffAsync(employee);
                break;
        }
    }

    private async Task ExecuteChefActionAsync(Employee employee, int choice)
    {
        switch (choice)
        {
            case 1:
                await ShowMenuAsync();
                break;
            case 2:
                await UpdateKitchenStatusAsync(employee);
                break;
            case 3:
                await ShowInventoryOnlyAsync();
                break;
        }
    }

    private async Task ExecuteDeliveryActionAsync(Employee employee, int choice)
    {
        switch (choice)
        {
            case 1:
                await ShowMyDeliveriesAsync(employee);
                break;
            case 2:
                await UpdateAssignedDeliveryAsync(employee, DeliveryStatus.Delivered);
                break;
            case 3:
                await UpdateAssignedDeliveryAsync(employee, DeliveryStatus.Failed);
                break;
        }
    }

    private async Task ShowBranchesAsync()
    {
        Theme.Header("Branches");
        var state = await _service.GetStateAsync();

        foreach (var branch in state.Branches)
        {
            Console.WriteLine($"{branch.Id} | {branch.Name}");
            Console.WriteLine($"Address: {branch.Address}");
            Console.WriteLine($"Phone  : {branch.ContactNumber}");
            Console.WriteLine($"Hours  : {branch.OpeningHours}");
            Console.WriteLine($"Manager: {FindEmployeeById(state.Employees, branch.ManagerEmployeeId)!.FullName}");
            Console.WriteLine(new string('-', 72));
        }
    }

    private async Task ShowEmployeesAndShiftsAsync()
    {
        Theme.Header("Employees and Shifts");
        var state = await _service.GetStateAsync();

        foreach (var employee in state.Employees)
        {
            Console.WriteLine($"{employee.Id} | {employee.FullName} | {employee.Position} | Branch {employee.PrimaryBranchId}");

            if (employee.DeliveryProfile is not null)
            {
                Console.WriteLine($"  Delivery: {employee.DeliveryProfile.VehicleType} - {employee.DeliveryProfile.AssignedArea} - Available: {employee.DeliveryProfile.IsAvailable}");
            }

            for (var shiftIndex = 0; shiftIndex < state.ShiftSchedules.Count; shiftIndex++)
            {
                var shift = state.ShiftSchedules[shiftIndex];

                if (shift.EmployeeId.Equals(employee.Id, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"  Shift: {shift.Date:yyyy-MM-dd} {shift.StartTime:HH\\:mm}-{shift.EndTime:HH\\:mm}");
                }
            }

            Console.WriteLine(new string('-', 72));
        }
    }

    private async Task ShowCustomersAsync()
    {
        Theme.Header("Customers");
        var state = await _service.GetStateAsync();

        foreach (var customer in state.Customers)
        {
            Console.WriteLine($"{customer.Id} | {customer.FullName} | {customer.PhoneNumber} | Points: {customer.LoyaltyPoints}");
        }
    }

    private async Task RegisterCustomerAsync()
    {
        Theme.Header("Register Customer");
        var customer = await _service.RegisterCustomerAsync(
            Prompt.Required("Full name"),
            Prompt.Required("Phone number"),
            Prompt.Required("Email"));

        Theme.Success($"Customer created successfully: {customer.Id}");
    }

    private async Task ShowMenuAsync()
    {
        Theme.Header("Menu");
        var state = await _service.GetStateAsync();
        var branch = state.Branches[0];

        foreach (var item in state.MenuItems)
        {
            var branchMenu = FindBranchMenuItem(branch, item.Id);
            Console.WriteLine($"{item.Id} | {item.Name} | {item.Category} | Price: {branchMenu.ResolvePrice(item.BasePrice):0.00}");
            Console.WriteLine($"  {item.Description}");

            if (item.AddOns.Count > 0)
            {
                Console.Write("  Add-ons: ");

                for (var addOnIndex = 0; addOnIndex < item.AddOns.Count; addOnIndex++)
                {
                    var addOn = item.AddOns[addOnIndex];

                    if (addOnIndex > 0)
                    {
                        Console.Write(", ");
                    }

                    Console.Write($"{addOn.Id} {addOn.Name} (+{addOn.ExtraPrice:0.00})");
                }

                Console.WriteLine();
            }

            Console.WriteLine(new string('-', 72));
        }
    }

    private async Task PlaceOrderAsync(Employee actor)
    {
        Theme.Header("Place Order");
        var state = await _service.GetStateAsync();

        ShowCustomersBrief(state.Customers);
        var customerId = Prompt.Required("Customer ID");

        Console.WriteLine("1. Dine-In");
        Console.WriteLine("2. Takeaway");
        Console.WriteLine("3. Delivery");
        var orderType = (OrderType)Prompt.Int("Order type", 1, 3);

        var items = new List<OrderPlacementItem>();

        do
        {
            ShowMenuCompact(state);
            var menuItemId = Prompt.Required("Menu item ID");
            var menuItem = FindMenuItemById(state.MenuItems, menuItemId);
            var addOnIds = new List<string>();

            if (menuItem.AddOns.Count > 0 && Prompt.Confirm("Add add-ons for this item?"))
            {
                Console.Write("Available add-ons: ");

                for (var addOnIndex = 0; addOnIndex < menuItem.AddOns.Count; addOnIndex++)
                {
                    var addOn = menuItem.AddOns[addOnIndex];

                    if (addOnIndex > 0)
                    {
                        Console.Write(", ");
                    }

                    Console.Write($"{addOn.Id}:{addOn.Name}");
                }

                Console.WriteLine();
                var rawIds = Prompt.Required("Enter add-on IDs separated by comma");
                var addOnParts = rawIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                for (var index = 0; index < addOnParts.Length; index++)
                {
                    addOnIds.Add(addOnParts[index]);
                }
            }

            var notes = Prompt.Required("Special notes (write '-' if none)");

            items.Add(new OrderPlacementItem
            {
                MenuItemId = menuItemId,
                Quantity = Prompt.Int("Quantity", 1, 100),
                SpecialNotes = notes == "-" ? string.Empty : notes,
                AddOnIds = addOnIds
            });
        }
        while (Prompt.Confirm("Add another item?"));

        string? address = null;

        if (orderType == OrderType.Delivery)
        {
            address = Prompt.Required("Delivery address");
        }

        var order = await _service.PlaceOrderAsync(new OrderPlacementRequest
        {
            CustomerId = customerId,
            BranchId = state.Branches[0].Id,
            StaffId = actor.Id,
            OrderType = orderType,
            DeliveryAddress = address,
            Items = items
        });

        Theme.Success($"Order created successfully: {order.Id} | Total = {order.TotalAmount:0.00}");
    }

    private async Task UpdateKitchenStatusAsync(Employee actor)
    {
        Theme.Header("Kitchen Status");
        var state = await _service.GetStateAsync();
        ShowOrdersBrief(GetOrdersForBranch(state.Orders, actor.PrimaryBranchId));

        var orderId = Prompt.Required("Order ID");
        Console.WriteLine("1. Move to Preparing");
        Console.WriteLine("2. Mark as Served");
        var action = Prompt.Int("Action", 1, 2);

        if (action == 1)
        {
            string? managerId = null;

            if (Prompt.Confirm("Provide branch manager override if inventory is short?"))
            {
                var branchState = await _service.GetStateAsync();
                ShowEmployeesBrief(GetManagersForBranch(branchState.Employees, actor.PrimaryBranchId));
                managerId = Prompt.Required("Manager employee ID");
            }

            var order = await _service.MoveOrderToPreparingAsync(orderId, actor.Id, managerId);
            Theme.Success($"Order {order.Id} is now {order.Status}.");
        }
        else
        {
            var order = await _service.MarkOrderServedAsync(orderId, actor.Id);
            Theme.Success($"Order {order.Id} is now {order.Status}.");
        }
    }

    private async Task ProcessPaymentAsync(Employee actor)
    {
        Theme.Header("Process Payment");
        var state = await _service.GetStateAsync();
        ShowOrdersBrief(GetServedOrdersForBranch(state.Orders, actor.PrimaryBranchId));

        var orderId = Prompt.Required("Order ID");
        Console.WriteLine("1. Cash");
        Console.WriteLine("2. Card");
        Console.WriteLine("3. Wallet");
        var method = (PaymentMethod)Prompt.Int("Payment method", 1, 3);

        var order = await _service.ProcessPaymentAsync(orderId, method, actor.Id);
        Theme.Success($"Payment completed for {order.Id}. Total = {order.TotalAmount:0.00}");
    }

    private async Task AssignDeliveryStaffAsync(Employee actor)
    {
        Theme.Header("Assign Delivery Staff");
        var state = await _service.GetStateAsync();
        var deliveryOrders = GetAssignableDeliveryOrders(state.Orders, actor.PrimaryBranchId);

        if (deliveryOrders.Count == 0)
        {
            Theme.Warning("No served delivery orders are waiting for assignment.");
            return;
        }

        ShowOrdersBrief(deliveryOrders);
        var orderId = Prompt.Required("Order ID");
        ShowEmployeesBrief(GetAvailableDeliveryStaff(state.Employees, actor.PrimaryBranchId));
        var staffId = Prompt.Required("Delivery staff ID");
        var delivery = await _service.AssignDeliveryAsync(orderId, staffId, actor.Id);
        Theme.Success($"Delivery {delivery.Id} assigned.");
    }

    private async Task CancelOrderAsync(Employee actor)
    {
        Theme.Header("Cancel Order");
        var state = await _service.GetStateAsync();
        ShowOrdersBrief(GetCancelableOrders(state.Orders, actor));

        var orderId = Prompt.Required("Order ID");
        var reason = Prompt.Required("Cancellation reason");
        var order = await _service.CancelOrderAsync(orderId, reason, actor.Id);
        Theme.Warning($"Order {order.Id} is now {order.Status}.");
    }

    private async Task ShowInventoryAndFeedbackAsync()
    {
        Theme.Header("Inventory and Feedback");
        var state = await _service.GetStateAsync();
        var branch = state.Branches[0];

        Console.WriteLine("Inventory");
        Console.WriteLine(new string('-', 72));
        foreach (var inventoryItem in branch.InventoryItems)
        {
            Console.WriteLine($"{inventoryItem.IngredientName,-20} {inventoryItem.CurrentQuantity,8:0.##} {inventoryItem.Unit,-10} Low at: {inventoryItem.LowStockThreshold:0.##}");
        }

        Console.WriteLine();
        Console.WriteLine("1. Submit feedback");
        Console.WriteLine("2. View feedback list");
        var action = Prompt.Int("Action", 1, 2);

        if (action == 1)
        {
            ShowCustomersBrief(state.Customers);
            var customerId = Prompt.Required("Customer ID");
            ShowOrdersBrief(GetCompletedOrders(state.Orders));
            var orderId = Prompt.Required("Order ID");
            var rating = Prompt.Int("Rating", 1, 5);
            var comments = Prompt.Required("Comments (write '-' if none)");
            var feedback = await _service.SubmitFeedbackAsync(customerId, orderId, rating, comments == "-" ? string.Empty : comments);
            Theme.Success($"Feedback saved successfully: {feedback.Id}");
        }
        else
        {
            Console.WriteLine();
            foreach (var feedback in state.Feedbacks)
            {
                Console.WriteLine($"{feedback.Id} | Order {feedback.OrderId} | Rating {feedback.Rating}/5 | {feedback.Comments}");
            }
        }
    }

    private async Task ShowInventoryOnlyAsync()
    {
        Theme.Header("Inventory");
        var state = await _service.GetStateAsync();
        var branch = state.Branches[0];

        foreach (var inventoryItem in branch.InventoryItems)
        {
            Console.WriteLine($"{inventoryItem.IngredientName,-20} {inventoryItem.CurrentQuantity,8:0.##} {inventoryItem.Unit,-10} Low at: {inventoryItem.LowStockThreshold:0.##}");
        }
    }

    private async Task ShowMyDeliveriesAsync(Employee actor)
    {
        Theme.Header("My Delivery Jobs");
        var state = await _service.GetStateAsync();
        var myOrders = GetActiveDeliveriesForDriver(state.Orders, actor.Id);

        if (myOrders.Count == 0)
        {
            Theme.Warning("You have no assigned deliveries.");
            return;
        }

        ShowOrdersBrief(myOrders);
    }

    private async Task UpdateAssignedDeliveryAsync(Employee actor, DeliveryStatus status)
    {
        Theme.Header(status == DeliveryStatus.Delivered ? "Mark Delivery As Delivered" : "Mark Delivery As Failed");
        var state = await _service.GetStateAsync();
        var myOrders = GetActiveDeliveriesForDriver(state.Orders, actor.Id);

        if (myOrders.Count == 0)
        {
            Theme.Warning("You have no assigned deliveries.");
            return;
        }

        ShowOrdersBrief(myOrders);
        var orderId = Prompt.Required("Order ID");
        var reason = status == DeliveryStatus.Failed ? Prompt.Required("Failure reason") : null;
        var delivery = await _service.UpdateDeliveryStatusAsync(orderId, status, reason, actor.Id);

        if (status == DeliveryStatus.Delivered)
        {
            Theme.Success($"Delivery {delivery.Id} marked as delivered.");
        }
        else
        {
            Theme.Warning($"Delivery {delivery.Id} marked as failed.");
        }
    }

    private async Task ResetSeedAsync()
    {
        Theme.Header("Reset Seed Data");

        if (!Prompt.Confirm("This will overwrite the saved data file. Continue"))
        {
            Theme.Warning("Reset cancelled.");
            return;
        }

        await _service.ResetAsync(RestaurantSeedBuilder.Build());
        Theme.Success("Seed data restored successfully.");
    }

    private static void ShowCustomersBrief(IList<Customer> customers)
    {
        foreach (var customer in customers)
        {
            Console.WriteLine($"{customer.Id} | {customer.FullName}");
        }

        Console.WriteLine();
    }

    private static void ShowEmployeesBrief(IList<Employee> employees)
    {
        foreach (var employee in employees)
        {
            Console.WriteLine($"{employee.Id} | {employee.FullName} | {employee.Position}");
        }

        Console.WriteLine();
    }

    private static void ShowMenuCompact(RestaurantSystemState state)
    {
        foreach (var item in state.MenuItems)
        {
            Console.WriteLine($"{item.Id} | {item.Name}");
        }

        Console.WriteLine();
    }

    private static void ShowOrdersBrief(IList<Order> orders)
    {
        foreach (var order in orders)
        {
            Console.WriteLine($"{order.Id} | {order.Type} | {order.Status} | Total {order.TotalAmount:0.00}");
        }

        Console.WriteLine();
    }

    private static Employee? FindEmployeeById(IList<Employee> employees, string employeeId)
    {
        for (var index = 0; index < employees.Count; index++)
        {
            if (employees[index].Id.Equals(employeeId, StringComparison.OrdinalIgnoreCase))
            {
                return employees[index];
            }
        }

        return null;
    }

    private static MenuItem FindMenuItemById(IList<MenuItem> menuItems, string menuItemId)
    {
        for (var index = 0; index < menuItems.Count; index++)
        {
            if (menuItems[index].Id.Equals(menuItemId, StringComparison.OrdinalIgnoreCase))
            {
                return menuItems[index];
            }
        }

        throw new InvalidOperationException("Menu item was not found.");
    }

    private static BranchMenuItem FindBranchMenuItem(Branch branch, string menuItemId)
    {
        for (var index = 0; index < branch.MenuItems.Count; index++)
        {
            if (branch.MenuItems[index].ItemId.Equals(menuItemId, StringComparison.OrdinalIgnoreCase))
            {
                return branch.MenuItems[index];
            }
        }

        throw new InvalidOperationException("Branch menu item was not found.");
    }

    private static List<Order> GetOrdersForBranch(IList<Order> orders, string branchId)
    {
        var result = new List<Order>();

        for (var index = 0; index < orders.Count; index++)
        {
            if (orders[index].BranchId.Equals(branchId, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(orders[index]);
            }
        }

        return result;
    }

    private static List<Employee> GetManagersForBranch(IList<Employee> employees, string branchId)
    {
        var result = new List<Employee>();

        for (var index = 0; index < employees.Count; index++)
        {
            if (employees[index].Position == EmployeePosition.BranchManager &&
                employees[index].PrimaryBranchId.Equals(branchId, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(employees[index]);
            }
        }

        return result;
    }

    private static List<Order> GetServedOrdersForBranch(IList<Order> orders, string branchId)
    {
        var result = new List<Order>();

        for (var index = 0; index < orders.Count; index++)
        {
            if (orders[index].Status == OrderStatus.Served &&
                orders[index].BranchId.Equals(branchId, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(orders[index]);
            }
        }

        return result;
    }

    private static List<Order> GetAssignableDeliveryOrders(IList<Order> orders, string branchId)
    {
        var result = new List<Order>();

        for (var index = 0; index < orders.Count; index++)
        {
            var order = orders[index];

            if (order.Type == OrderType.Delivery &&
                order.BranchId.Equals(branchId, StringComparison.OrdinalIgnoreCase) &&
                order.Status == OrderStatus.Served &&
                order.Delivery is not null &&
                string.IsNullOrWhiteSpace(order.Delivery.DeliveryStaffId))
            {
                result.Add(order);
            }
        }

        return result;
    }

    private static List<Order> GetCancelableOrders(IList<Order> orders, Employee actor)
    {
        var result = new List<Order>();

        for (var index = 0; index < orders.Count; index++)
        {
            var order = orders[index];

            if (!order.BranchId.Equals(actor.PrimaryBranchId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (actor.Position == EmployeePosition.BranchManager)
            {
                if (order.Status != OrderStatus.Completed &&
                    order.Status != OrderStatus.Cancelled &&
                    (order.Delivery is null || order.Delivery.Status != DeliveryStatus.OnTheWay))
                {
                    result.Add(order);
                }
            }
            else if ((actor.Position == EmployeePosition.Waiter || actor.Position == EmployeePosition.Cashier) &&
                     order.Status == OrderStatus.Pending)
            {
                result.Add(order);
            }
        }

        return result;
    }

    private static List<Employee> GetAvailableDeliveryStaff(IList<Employee> employees, string branchId)
    {
        var result = new List<Employee>();

        for (var index = 0; index < employees.Count; index++)
        {
            var employee = employees[index];

            if (employee.DeliveryProfile is not null &&
                employee.DeliveryProfile.IsAvailable &&
                employee.PrimaryBranchId.Equals(branchId, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(employee);
            }
        }

        return result;
    }

    private static List<Order> GetCompletedOrders(IList<Order> orders)
    {
        var result = new List<Order>();

        for (var index = 0; index < orders.Count; index++)
        {
            if (orders[index].Status == OrderStatus.Completed)
            {
                result.Add(orders[index]);
            }
        }

        return result;
    }

    private static List<Order> GetActiveDeliveriesForDriver(IList<Order> orders, string employeeId)
    {
        var result = new List<Order>();

        for (var index = 0; index < orders.Count; index++)
        {
            var order = orders[index];

            if (order.Delivery is not null &&
                order.Delivery.DeliveryStaffId == employeeId &&
                order.Delivery.Status == DeliveryStatus.OnTheWay)
            {
                result.Add(order);
            }
        }

        return result;
    }
}
