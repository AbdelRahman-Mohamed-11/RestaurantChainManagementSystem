namespace RestaurantChainManagementSystem.Core.Interfaces;

public interface IRestaurantDataStore
{
    Task<RestaurantSystemState> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(RestaurantSystemState state, CancellationToken cancellationToken = default);
}
