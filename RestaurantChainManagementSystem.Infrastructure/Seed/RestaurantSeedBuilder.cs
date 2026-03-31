using RestaurantChainManagementSystem.Core;
using RestaurantChainManagementSystem.Core.Entities;
using RestaurantChainManagementSystem.Core.Enums;
using RestaurantChainManagementSystem.Core.Factories;

namespace RestaurantChainManagementSystem.Infrastructure.Seed;

public static class RestaurantSeedBuilder
{
    public static RestaurantSystemState Build()
    {
        var branchId = NewId();
        var managerId = NewId();
        var waiterId = NewId();
        var cashierId = NewId();
        var chefId = NewId();
        var driverId = NewId();

        var manager = EntityFactory.CreateEmployee(
            managerId,
            "Sara Hassan",
            EmployeePosition.BranchManager,
            18000m,
            new DateOnly(2021, 2, 10),
            "01010001000",
            "sara.manager@restaurant.local",
            branchId);

        var waiter = EntityFactory.CreateEmployee(
            waiterId,
            "Omar Ali",
            EmployeePosition.Waiter,
            8200m,
            new DateOnly(2022, 5, 1),
            "01010001001",
            "omar.waiter@restaurant.local",
            branchId);

        var cashier = EntityFactory.CreateEmployee(
            cashierId,
            "Mona Adel",
            EmployeePosition.Cashier,
            9000m,
            new DateOnly(2022, 8, 8),
            "01010001002",
            "mona.cashier@restaurant.local",
            branchId);

        var chef = EntityFactory.CreateEmployee(
            chefId,
            "Youssef Nabil",
            EmployeePosition.Chef,
            12500m,
            new DateOnly(2020, 11, 12),
            "01010001003",
            "youssef.chef@restaurant.local",
            branchId);

        var driver = EntityFactory.CreateEmployee(
            driverId,
            "Hany Mahmoud",
            EmployeePosition.DeliveryStaff,
            7800m,
            new DateOnly(2023, 1, 15),
            "01010001004",
            "hany.delivery@restaurant.local",
            branchId);

        driver.AttachDeliveryProfile(DeliveryProfile.Create("Motorbike", "LIC-7781", "Nasr City"));

        var branch = EntityFactory.CreateBranch(
            branchId,
            "Flavor House - Nasr City",
            "15 Abbas El Akkad St, Cairo",
            "0223456789",
            "10:00 AM - 11:00 PM",
            manager.Id);

        var ingredientChickenId = NewId();
        var ingredientRiceId = NewId();
        var ingredientCheeseId = NewId();
        var ingredientBreadId = NewId();
        var ingredientPotatoId = NewId();
        var ingredientSyrupId = NewId();

        var ingredients = new List<Ingredient>
        {
            new(ingredientChickenId, "Chicken Breast", IngredientUnit.Kilogram),
            new(ingredientRiceId, "Rice", IngredientUnit.Kilogram),
            new(ingredientCheeseId, "Cheese", IngredientUnit.Kilogram),
            new(ingredientBreadId, "Bread Bun", IngredientUnit.Piece),
            new(ingredientPotatoId, "Potato", IngredientUnit.Kilogram),
            new(ingredientSyrupId, "Soft Drink Syrup", IngredientUnit.Liter)
        };

        var grilledChicken = EntityFactory.CreateMenuItem(
            NewId(),
            "Grilled Chicken Plate",
            "Charcoal grilled chicken served with seasoned rice.",
            165m,
            MenuCategory.MainCourse);
        grilledChicken.Recipe.AddRange([
            new RecipeItem(ingredientChickenId, "Chicken Breast", IngredientUnit.Kilogram, 0.35m),
            new RecipeItem(ingredientRiceId, "Rice", IngredientUnit.Kilogram, 0.20m)
        ]);
        grilledChicken.AddOns.Add(new AddOn(NewId(), "Extra Cheese", 20m));

        var crispyBurger = EntityFactory.CreateMenuItem(
            NewId(),
            "Crispy Burger",
            "Crispy chicken burger with sauce and fries.",
            140m,
            MenuCategory.MainCourse);
        crispyBurger.Recipe.AddRange([
            new RecipeItem(ingredientChickenId, "Chicken Breast", IngredientUnit.Kilogram, 0.20m),
            new RecipeItem(ingredientCheeseId, "Cheese", IngredientUnit.Kilogram, 0.05m),
            new RecipeItem(ingredientBreadId, "Bread Bun", IngredientUnit.Piece, 1m),
            new RecipeItem(ingredientPotatoId, "Potato", IngredientUnit.Kilogram, 0.18m)
        ]);
        crispyBurger.AddOns.Add(new AddOn(NewId(), "Large Size", 25m));

        var cola = EntityFactory.CreateMenuItem(
            NewId(),
            "Cola",
            "Cold fountain cola.",
            35m,
            MenuCategory.Beverage);
        cola.Recipe.Add(new RecipeItem(ingredientSyrupId, "Soft Drink Syrup", IngredientUnit.Liter, 0.10m));

        branch.MenuItems.AddRange([
            new BranchMenuItem(grilledChicken.Id, null, true),
            new BranchMenuItem(crispyBurger.Id, 145m, true),
            new BranchMenuItem(cola.Id, null, true)
        ]);

        branch.InventoryItems.AddRange([
            new InventoryItem(ingredientChickenId, "Chicken Breast", IngredientUnit.Kilogram, 20m, 3m),
            new InventoryItem(ingredientRiceId, "Rice", IngredientUnit.Kilogram, 15m, 2m),
            new InventoryItem(ingredientCheeseId, "Cheese", IngredientUnit.Kilogram, 8m, 1m),
            new InventoryItem(ingredientBreadId, "Bread Bun", IngredientUnit.Piece, 50m, 10m),
            new InventoryItem(ingredientPotatoId, "Potato", IngredientUnit.Kilogram, 12m, 2m),
            new InventoryItem(ingredientSyrupId, "Soft Drink Syrup", IngredientUnit.Liter, 10m, 2m)
        ]);

        var customers = new List<Customer>
        {
            EntityFactory.CreateCustomer(NewId(), "Ahmed Samir", "01099990001", "ahmed.customer@restaurant.local"),
            EntityFactory.CreateCustomer(NewId(), "Nour Tarek", "01099990002", "nour.customer@restaurant.local")
        };

        var shifts = new List<ShiftSchedule>
        {
            ShiftSchedule.Create(NewId(), waiter.Id, branch.Id, DateOnly.FromDateTime(DateTime.Today), new TimeOnly(10, 0), new TimeOnly(18, 0)),
            ShiftSchedule.Create(NewId(), cashier.Id, branch.Id, DateOnly.FromDateTime(DateTime.Today), new TimeOnly(10, 0), new TimeOnly(18, 0)),
            ShiftSchedule.Create(NewId(), chef.Id, branch.Id, DateOnly.FromDateTime(DateTime.Today), new TimeOnly(11, 0), new TimeOnly(19, 0)),
            ShiftSchedule.Create(NewId(), driver.Id, branch.Id, DateOnly.FromDateTime(DateTime.Today), new TimeOnly(12, 0), new TimeOnly(20, 0))
        };

        return new RestaurantSystemState
        {
            Branches = [branch],
            Employees = [manager, waiter, cashier, chef, driver],
            Customers = customers,
            MenuItems = [grilledChicken, crispyBurger, cola],
            Ingredients = ingredients,
            ShiftSchedules = shifts,
            Orders = [],
            Feedbacks = []
        };
    }

    private static string NewId() => Guid.NewGuid().ToString();
}
