using System.Text.Json;
using System.Text.Json.Serialization;
using MetalStorming20.Core;

namespace MetalStorming20.Web.Services;

public sealed class Upgrades2CatalogService
{
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web);
    private Upgrades2Catalog? cachedCatalog;

    public Upgrades2CatalogService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<Upgrades2Catalog> GetCatalogAsync()
    {
        if (cachedCatalog is not null)
        {
            return cachedCatalog;
        }

        var aircraft = await ReadAsync<AircraftCatalogDto[]>("data/v2/aircraft.json");
        var systemTypes = await ReadAsync<SystemTypeCatalogDto[]>("data/v2/system-types.json");
        var slots = await ReadAsync<AircraftSystemSlotCatalogDto[]>("data/v2/aircraft-system-slots.json");
        var systemTypeById = systemTypes.ToDictionary(s => s.SystemTypeId, StringComparer.OrdinalIgnoreCase);

        cachedCatalog = new Upgrades2Catalog(
            aircraft
                .Where(a => a.IsOwnedSupported)
                .OrderBy(a => a.SortOrder)
                .Select(a => new AircraftCatalogItem(a.AircraftId, a.DisplayName))
                .ToArray(),
            slots
                .OrderBy(s => s.AircraftId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(s => s.SlotOrder)
                .Select(s => new SystemSlotDefinitionV2(
                    s.SystemSlotId,
                    s.AircraftId,
                    s.SystemTypeId,
                    systemTypeById[s.SystemTypeId].CurrencyCode,
                    s.SlotLabel,
                    s.UnlockAircraftLevel))
                .ToArray());

        return cachedCatalog;
    }

    private async Task<T> ReadAsync<T>(string url)
    {
        await using var stream = await httpClient.GetStreamAsync(url);
        return await JsonSerializer.DeserializeAsync<T>(stream, jsonOptions) ??
            throw new InvalidOperationException($"Catalog file {url} was empty.");
    }

    private sealed record AircraftCatalogDto(
        string AircraftId,
        string DisplayName,
        bool IsOwnedSupported,
        int SortOrder);

    private sealed record SystemTypeCatalogDto(
        string SystemTypeId,
        string DisplayName,
        string CurrencyCode,
        bool IsCoreSystem,
        bool AllowsDuplicateSlots);

    private sealed record AircraftSystemSlotCatalogDto(
        string SystemSlotId,
        string AircraftId,
        string SystemTypeId,
        string SlotLabel,
        int SlotOrder,
        int UnlockAircraftLevel,
        int MaxSystemLevel,
        string CatalogConfidence);
}

public sealed record Upgrades2Catalog(
    IReadOnlyList<AircraftCatalogItem> Aircraft,
    IReadOnlyList<SystemSlotDefinitionV2> SystemSlots);

public sealed record AircraftCatalogItem(string AircraftId, string DisplayName);
