namespace MetalStorming20.Core;

public sealed record Upgrades2SavedSystemPlan(
    string SystemSlotId,
    IReadOnlyList<string>? SelectedNodes,
    IReadOnlyDictionary<string, string>? NodeStates);

public sealed record Upgrades2PlannerState(
    int SchemaVersion,
    int CurrentAircraftLevel,
    int TargetAircraftLevel,
    IReadOnlyList<Upgrades2SavedSystemPlan>? SystemPlans,
    int CurrentMasteryLevel,
    int PlannedMasteryLevel,
    string? GoldMasteryStatus);

public sealed record Upgrades2PlannerStateResponse(Upgrades2PlannerState? State);

public interface IUpgrades2PlannerStateGateway
{
    Task<Upgrades2PlannerState?> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(Upgrades2PlannerState state, CancellationToken cancellationToken = default);
}

public interface IUpgrades2PlannerStatePresenter
{
    void Present(Upgrades2PlannerStateResponse response);
}

public sealed class LoadUpgrades2PlannerStateUseCase
{
    private readonly IUpgrades2PlannerStateGateway stateGateway;

    public LoadUpgrades2PlannerStateUseCase(IUpgrades2PlannerStateGateway stateGateway)
    {
        this.stateGateway = stateGateway;
    }

    public async Task HandleAsync(
        IUpgrades2PlannerStatePresenter presenter,
        CancellationToken cancellationToken = default)
    {
        var state = await stateGateway.LoadAsync(cancellationToken);
        presenter.Present(new Upgrades2PlannerStateResponse(state));
    }
}

public sealed class SaveUpgrades2PlannerStateUseCase
{
    private readonly IUpgrades2PlannerStateGateway stateGateway;

    public SaveUpgrades2PlannerStateUseCase(IUpgrades2PlannerStateGateway stateGateway)
    {
        this.stateGateway = stateGateway;
    }

    public Task HandleAsync(Upgrades2PlannerState state, CancellationToken cancellationToken = default) =>
        stateGateway.SaveAsync(state, cancellationToken);
}
