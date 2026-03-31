using System.Text.Json;
using RestaurantChainManagementSystem.Core;
using RestaurantChainManagementSystem.Core.Interfaces;
using RestaurantChainManagementSystem.Core.Services;
using RestaurantChainManagementSystem.Infrastructure.Seed;

namespace RestaurantChainManagementSystem.Infrastructure;

public sealed class JsonRestaurantDataStore : IRestaurantDataStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;

    public JsonRestaurantDataStore(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<RestaurantSystemState> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            var seed = RestaurantSeedBuilder.Build();
            await SaveAsync(seed, cancellationToken);
            return seed;
        }

        try
        {
            await using var stream = File.Open(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var state = await JsonSerializer.DeserializeAsync<RestaurantSystemState>(stream, SerializerOptions, cancellationToken);
            var result = state ?? new RestaurantSystemState();
            RestaurantStateValidator.Validate(result);
            return result;
        }
        catch (Exception) when (File.Exists(_filePath))
        {
            var seed = RestaurantSeedBuilder.Build();
            await SaveAsync(seed, cancellationToken);
            return seed;
        }
    }

    public async Task SaveAsync(RestaurantSystemState state, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Open(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, state, SerializerOptions, cancellationToken);
    }
}
