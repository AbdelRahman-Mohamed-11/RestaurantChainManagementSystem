using RestaurantChainManagementSystem.Core.Entities;
using RestaurantChainManagementSystem.Core.Enums;
namespace RestaurantChainManagementSystem.Core.Services;

public static class RestaurantStateValidator
{
    public static void Validate(RestaurantSystemState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        EnsureUniqueBranchIds(state);
        EnsureUniqueEmployeeIds(state);
        EnsureUniqueCustomerIds(state);
        EnsureUniqueMenuItemIds(state);
        EnsureUniqueIngredientIds(state);
        EnsureUniqueOrderIds(state);
        EnsureUniqueFeedbackIds(state);
        EnsureUniqueShiftIds(state);

        EnsureUniqueBranchNameAddress(state);
        EnsureUniqueMenuItemNames(state);
        EnsureUniqueCustomerContacts(state);
        EnsureUniqueEmployeeContacts(state);

        foreach (var branch in state.Branches)
        {
            var manager = FindEmployeeById(state, branch.ManagerEmployeeId, "Branch manager");

            if (manager.Position != EmployeePosition.BranchManager)
            {
                throw new InvalidOperationException($"Employee '{manager.FullName}' must be a branch manager.");
            }

            EnsureUniqueBranchMenuItems(branch);
            EnsureUniqueInventoryItems(branch);

            foreach (var branchMenuItem in branch.MenuItems)
            {
                FindMenuItemById(state, branchMenuItem.ItemId, "Menu item");
            }

            foreach (var inventoryItem in branch.InventoryItems)
            {
                FindIngredientById(state, inventoryItem.IngredientId, "Ingredient");
            }
        }

        foreach (var employee in state.Employees)
        {
            FindBranchById(state, employee.PrimaryBranchId, "Primary branch");

            if (employee.Position == EmployeePosition.DeliveryStaff && employee.DeliveryProfile is null)
            {
                throw new InvalidOperationException($"Delivery staff '{employee.FullName}' must have a delivery profile.");
            }

            if (employee.Position != EmployeePosition.DeliveryStaff && employee.DeliveryProfile is not null)
            {
                throw new InvalidOperationException($"Only delivery staff can have a delivery profile. Employee '{employee.FullName}' is invalid.");
            }
        }

        for (var shiftIndex = 0; shiftIndex < state.ShiftSchedules.Count; shiftIndex++)
        {
            var shift = state.ShiftSchedules[shiftIndex];
            FindEmployeeById(state, shift.EmployeeId, "Shift employee");
            FindBranchById(state, shift.BranchId, "Shift branch");

            for (var nextIndex = shiftIndex + 1; nextIndex < state.ShiftSchedules.Count; nextIndex++)
            {
                var otherShift = state.ShiftSchedules[nextIndex];

                if (shift.EmployeeId.Equals(otherShift.EmployeeId, StringComparison.OrdinalIgnoreCase) &&
                    shift.Overlaps(otherShift))
                {
                    throw new InvalidOperationException($"Employee '{shift.EmployeeId}' has overlapping shifts.");
                }
            }
        }

        foreach (var menuItem in state.MenuItems)
        {
            EnsureUniqueAddOnIds(menuItem);
            EnsureUniqueAddOnNames(menuItem);
            EnsureUniqueRecipeIngredients(menuItem);

            foreach (var recipeItem in menuItem.Recipe)
            {
                FindIngredientById(state, recipeItem.IngredientId, "Recipe ingredient");
            }
        }

        foreach (var order in state.Orders)
        {
            var branch = FindBranchById(state, order.BranchId, "Order branch");
            FindCustomerById(state, order.CustomerId, "Order customer");
            var staff = FindEmployeeById(state, order.StaffId, "Order staff");

            if (!staff.PrimaryBranchId.Equals(branch.Id, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Order '{order.Id}' references a staff member from a different branch.");
            }

            if (order.Items.Count == 0)
            {
                throw new InvalidOperationException($"Order '{order.Id}' must contain at least one item.");
            }

            if (order.Type == OrderType.Delivery && order.Delivery is null)
            {
                throw new InvalidOperationException($"Delivery order '{order.Id}' must have a delivery record.");
            }

            if (order.Type != OrderType.Delivery && order.Delivery is not null)
            {
                throw new InvalidOperationException($"Non-delivery order '{order.Id}' cannot have a delivery record.");
            }

            if (order.Type == OrderType.Delivery &&
                order.Delivery is not null &&
                order.Delivery.Status == DeliveryStatus.Delivered &&
                order.Status != OrderStatus.Completed)
            {
                throw new InvalidOperationException($"Delivered order '{order.Id}' must be completed.");
            }

            foreach (var orderItem in order.Items)
            {
                var menuItem = FindMenuItemById(state, orderItem.ItemId, "Order menu item");

                foreach (var selectedAddOn in orderItem.SelectedAddOns)
                {
                    FindAddOnById(menuItem, selectedAddOn.AddOnId, "Selected add-on");
                }
            }

            if (order.Delivery is not null && !string.IsNullOrWhiteSpace(order.Delivery.DeliveryStaffId))
            {
                var deliveryEmployee = FindEmployeeById(state, order.Delivery.DeliveryStaffId!, "Delivery staff");

                if (deliveryEmployee.DeliveryProfile is null)
                {
                    throw new InvalidOperationException($"Assigned delivery staff for order '{order.Id}' must have a delivery profile.");
                }

                if (!deliveryEmployee.PrimaryBranchId.Equals(order.BranchId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"Assigned delivery staff for order '{order.Id}' must belong to the same branch.");
                }
            }
        }

        EnsureUniqueFeedbackPairs(state);

        foreach (var feedback in state.Feedbacks)
        {
            var order = FindOrderById(state, feedback.OrderId, "Feedback order");

            if (order.Status != OrderStatus.Completed)
            {
                throw new InvalidOperationException($"Feedback '{feedback.Id}' must belong to a completed order.");
            }

            if (!order.CustomerId.Equals(feedback.CustomerId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Feedback '{feedback.Id}' customer does not match its order.");
            }
        }
    }

    private static void EnsureUniqueBranchIds(RestaurantSystemState state)
    {
        var values = new List<string>();

        for (var index = 0; index < state.Branches.Count; index++)
        {
            var id = state.Branches[index].Id;

            if (ContainsValue(values, id))
            {
                throw new InvalidOperationException("Duplicate values found for Branch IDs.");
            }

            values.Add(id);
        }
    }

    private static void EnsureUniqueEmployeeIds(RestaurantSystemState state)
    {
        var values = new List<string>();

        for (var index = 0; index < state.Employees.Count; index++)
        {
            var id = state.Employees[index].Id;

            if (ContainsValue(values, id))
            {
                throw new InvalidOperationException("Duplicate values found for Employee IDs.");
            }

            values.Add(id);
        }
    }

    private static void EnsureUniqueCustomerIds(RestaurantSystemState state)
    {
        var values = new List<string>();

        for (var index = 0; index < state.Customers.Count; index++)
        {
            var id = state.Customers[index].Id;

            if (ContainsValue(values, id))
            {
                throw new InvalidOperationException("Duplicate values found for Customer IDs.");
            }

            values.Add(id);
        }
    }

    private static void EnsureUniqueMenuItemIds(RestaurantSystemState state)
    {
        var values = new List<string>();

        for (var index = 0; index < state.MenuItems.Count; index++)
        {
            var id = state.MenuItems[index].Id;

            if (ContainsValue(values, id))
            {
                throw new InvalidOperationException("Duplicate values found for Menu item IDs.");
            }

            values.Add(id);
        }
    }

    private static void EnsureUniqueIngredientIds(RestaurantSystemState state)
    {
        var values = new List<string>();

        for (var index = 0; index < state.Ingredients.Count; index++)
        {
            var id = state.Ingredients[index].Id;

            if (ContainsValue(values, id))
            {
                throw new InvalidOperationException("Duplicate values found for Ingredient IDs.");
            }

            values.Add(id);
        }
    }

    private static void EnsureUniqueOrderIds(RestaurantSystemState state)
    {
        var values = new List<string>();

        for (var index = 0; index < state.Orders.Count; index++)
        {
            var id = state.Orders[index].Id;

            if (ContainsValue(values, id))
            {
                throw new InvalidOperationException("Duplicate values found for Order IDs.");
            }

            values.Add(id);
        }
    }

    private static void EnsureUniqueFeedbackIds(RestaurantSystemState state)
    {
        var values = new List<string>();

        for (var index = 0; index < state.Feedbacks.Count; index++)
        {
            var id = state.Feedbacks[index].Id;

            if (ContainsValue(values, id))
            {
                throw new InvalidOperationException("Duplicate values found for Feedback IDs.");
            }

            values.Add(id);
        }
    }

    private static void EnsureUniqueShiftIds(RestaurantSystemState state)
    {
        var values = new List<string>();

        for (var index = 0; index < state.ShiftSchedules.Count; index++)
        {
            var id = state.ShiftSchedules[index].Id;

            if (ContainsValue(values, id))
            {
                throw new InvalidOperationException("Duplicate values found for Shift IDs.");
            }

            values.Add(id);
        }
    }

    private static void EnsureUniqueBranchNameAddress(RestaurantSystemState state)
    {
        var values = new List<string>();

        for (var index = 0; index < state.Branches.Count; index++)
        {
            var value = $"{state.Branches[index].Name.Trim().ToUpperInvariant()}|{state.Branches[index].Address.Trim().ToUpperInvariant()}";

            if (ContainsValue(values, value))
            {
                throw new InvalidOperationException("Duplicate values found for branch name + address.");
            }

            values.Add(value);
        }
    }

    private static void EnsureUniqueMenuItemNames(RestaurantSystemState state)
    {
        var values = new List<string>();

        for (var index = 0; index < state.MenuItems.Count; index++)
        {
            var value = state.MenuItems[index].Name.Trim().ToUpperInvariant();

            if (ContainsValue(values, value))
            {
                throw new InvalidOperationException("Duplicate values found for menu item names.");
            }

            values.Add(value);
        }
    }

    private static void EnsureUniqueCustomerContacts(RestaurantSystemState state)
    {
        var phones = new List<string>();
        var emails = new List<string>();

        for (var index = 0; index < state.Customers.Count; index++)
        {
            var phone = state.Customers[index].PhoneNumber.Trim().ToUpperInvariant();
            var email = state.Customers[index].Email.Trim().ToUpperInvariant();

            if (ContainsValue(phones, phone))
            {
                throw new InvalidOperationException("Duplicate values found for customer phone numbers.");
            }

            if (ContainsValue(emails, email))
            {
                throw new InvalidOperationException("Duplicate values found for customer emails.");
            }

            phones.Add(phone);
            emails.Add(email);
        }
    }

    private static void EnsureUniqueEmployeeContacts(RestaurantSystemState state)
    {
        var phones = new List<string>();
        var emails = new List<string>();

        for (var index = 0; index < state.Employees.Count; index++)
        {
            var phone = state.Employees[index].PhoneNumber.Trim().ToUpperInvariant();
            var email = state.Employees[index].Email.Trim().ToUpperInvariant();

            if (ContainsValue(phones, phone))
            {
                throw new InvalidOperationException("Duplicate values found for employee phone numbers.");
            }

            if (ContainsValue(emails, email))
            {
                throw new InvalidOperationException("Duplicate values found for employee emails.");
            }

            phones.Add(phone);
            emails.Add(email);
        }
    }

    private static void EnsureUniqueBranchMenuItems(Branch branch)
    {
        var values = new List<string>();

        for (var index = 0; index < branch.MenuItems.Count; index++)
        {
            var value = branch.MenuItems[index].ItemId;

            if (ContainsValue(values, value))
            {
                throw new InvalidOperationException($"Duplicate values found for branch menu items for branch '{branch.Name}'.");
            }

            values.Add(value);
        }
    }

    private static void EnsureUniqueInventoryItems(Branch branch)
    {
        var values = new List<string>();

        for (var index = 0; index < branch.InventoryItems.Count; index++)
        {
            var value = branch.InventoryItems[index].IngredientId;

            if (ContainsValue(values, value))
            {
                throw new InvalidOperationException($"Duplicate values found for branch inventory ingredients for branch '{branch.Name}'.");
            }

            values.Add(value);
        }
    }

    private static void EnsureUniqueAddOnIds(MenuItem menuItem)
    {
        var values = new List<string>();

        for (var index = 0; index < menuItem.AddOns.Count; index++)
        {
            var value = menuItem.AddOns[index].Id;

            if (ContainsValue(values, value))
            {
                throw new InvalidOperationException($"Duplicate values found for add-on IDs for menu item '{menuItem.Name}'.");
            }

            values.Add(value);
        }
    }

    private static void EnsureUniqueAddOnNames(MenuItem menuItem)
    {
        var values = new List<string>();

        for (var index = 0; index < menuItem.AddOns.Count; index++)
        {
            var value = menuItem.AddOns[index].Name.Trim().ToUpperInvariant();

            if (ContainsValue(values, value))
            {
                throw new InvalidOperationException($"Duplicate values found for add-on names for menu item '{menuItem.Name}'.");
            }

            values.Add(value);
        }
    }

    private static void EnsureUniqueRecipeIngredients(MenuItem menuItem)
    {
        var values = new List<string>();

        for (var index = 0; index < menuItem.Recipe.Count; index++)
        {
            var value = menuItem.Recipe[index].IngredientId;

            if (ContainsValue(values, value))
            {
                throw new InvalidOperationException($"Duplicate values found for recipe ingredients for menu item '{menuItem.Name}'.");
            }

            values.Add(value);
        }
    }

    private static void EnsureUniqueFeedbackPairs(RestaurantSystemState state)
    {
        var values = new List<string>();

        for (var index = 0; index < state.Feedbacks.Count; index++)
        {
            var value = $"{state.Feedbacks[index].CustomerId.ToUpperInvariant()}|{state.Feedbacks[index].OrderId.ToUpperInvariant()}";

            if (ContainsValue(values, value))
            {
                throw new InvalidOperationException("Duplicate values found for feedback customer + order combinations.");
            }

            values.Add(value);
        }
    }

    private static bool ContainsValue(List<string> values, string value)
    {
        for (var index = 0; index < values.Count; index++)
        {
            if (values[index].Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static Branch FindBranchById(RestaurantSystemState state, string branchId, string entityName)
    {
        for (var index = 0; index < state.Branches.Count; index++)
        {
            if (state.Branches[index].Id.Equals(branchId, StringComparison.OrdinalIgnoreCase))
            {
                return state.Branches[index];
            }
        }

        throw new InvalidOperationException($"{entityName} '{branchId}' was not found.");
    }

    private static Employee FindEmployeeById(RestaurantSystemState state, string employeeId, string entityName)
    {
        for (var index = 0; index < state.Employees.Count; index++)
        {
            if (state.Employees[index].Id.Equals(employeeId, StringComparison.OrdinalIgnoreCase))
            {
                return state.Employees[index];
            }
        }

        throw new InvalidOperationException($"{entityName} '{employeeId}' was not found.");
    }

    private static Customer FindCustomerById(RestaurantSystemState state, string customerId, string entityName)
    {
        for (var index = 0; index < state.Customers.Count; index++)
        {
            if (state.Customers[index].Id.Equals(customerId, StringComparison.OrdinalIgnoreCase))
            {
                return state.Customers[index];
            }
        }

        throw new InvalidOperationException($"{entityName} '{customerId}' was not found.");
    }

    private static MenuItem FindMenuItemById(RestaurantSystemState state, string menuItemId, string entityName)
    {
        for (var index = 0; index < state.MenuItems.Count; index++)
        {
            if (state.MenuItems[index].Id.Equals(menuItemId, StringComparison.OrdinalIgnoreCase))
            {
                return state.MenuItems[index];
            }
        }

        throw new InvalidOperationException($"{entityName} '{menuItemId}' was not found.");
    }

    private static Ingredient FindIngredientById(RestaurantSystemState state, string ingredientId, string entityName)
    {
        for (var index = 0; index < state.Ingredients.Count; index++)
        {
            if (state.Ingredients[index].Id.Equals(ingredientId, StringComparison.OrdinalIgnoreCase))
            {
                return state.Ingredients[index];
            }
        }

        throw new InvalidOperationException($"{entityName} '{ingredientId}' was not found.");
    }

    private static AddOn FindAddOnById(MenuItem menuItem, string addOnId, string entityName)
    {
        for (var index = 0; index < menuItem.AddOns.Count; index++)
        {
            if (menuItem.AddOns[index].Id.Equals(addOnId, StringComparison.OrdinalIgnoreCase))
            {
                return menuItem.AddOns[index];
            }
        }

        throw new InvalidOperationException($"{entityName} '{addOnId}' was not found.");
    }

    private static Order FindOrderById(RestaurantSystemState state, string orderId, string entityName)
    {
        for (var index = 0; index < state.Orders.Count; index++)
        {
            if (state.Orders[index].Id.Equals(orderId, StringComparison.OrdinalIgnoreCase))
            {
                return state.Orders[index];
            }
        }

        throw new InvalidOperationException($"{entityName} '{orderId}' was not found.");
    }
}
