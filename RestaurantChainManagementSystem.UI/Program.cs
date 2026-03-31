using RestaurantChainManagementSystem.Core.Services;
using RestaurantChainManagementSystem.Infrastructure;
using RestaurantChainManagementSystem.UI.ConsoleUi;

var projectRoot = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
    "OOP Project",
    "RestaurantChainManagementSystem");

var dataFilePath = Path.Combine(projectRoot, "RestaurantChainManagementSystem.Infrastructure", "Data", "restaurant-data.json");
var dataStore = new JsonRestaurantDataStore(dataFilePath);
var service = new RestaurantApplicationService(dataStore);
var app = new RestaurantConsoleApp(service);

await app.RunAsync();
