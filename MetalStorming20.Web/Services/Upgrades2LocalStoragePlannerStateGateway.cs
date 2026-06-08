using System.Text.Json;
using MetalStorming20.Core;
using Microsoft.JSInterop;

namespace MetalStorming20.Web.Services;

public sealed class Upgrades2LocalStoragePlannerStateGateway : IUpgrades2PlannerStateGateway
{
    private const string StorageKey = "metalstorming20.upgrades2.state";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IJSRuntime jsRuntime;

    public Upgrades2LocalStoragePlannerStateGateway(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime;
    }

    public async Task<Upgrades2PlannerState?> LoadAsync(CancellationToken cancellationToken = default)
    {
        var saved = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, StorageKey);
        return string.IsNullOrWhiteSpace(saved)
            ? null
            : JsonSerializer.Deserialize<Upgrades2PlannerState>(saved, JsonOptions);
    }

    public async Task SaveAsync(Upgrades2PlannerState state, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(state, JsonOptions);
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, StorageKey, json);
    }
}
