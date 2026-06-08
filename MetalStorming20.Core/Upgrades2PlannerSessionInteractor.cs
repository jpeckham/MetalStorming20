namespace MetalStorming20.Core;

public sealed class Upgrades2PlannerSessionInteractor
{
    private readonly IUpgrades2CatalogGateway catalogGateway;
    private readonly IUpgrades2PlannerStateGateway stateGateway;
    private readonly PresentUpgrades2PlannerSessionUseCase presentSessionUseCase = new();

    public Upgrades2PlannerSessionInteractor(
        IUpgrades2CatalogGateway catalogGateway,
        IUpgrades2PlannerStateGateway stateGateway)
    {
        this.catalogGateway = catalogGateway;
        this.stateGateway = stateGateway;
    }

    public async Task InitializeAsync(
        Upgrades2PlannerSession session,
        IUpgrades2PlannerPresenter plannerPresenter,
        IUpgrades2PlannerSessionPresenter sessionPresenter,
        CancellationToken cancellationToken = default)
    {
        var systemSlots = await GetGenericSystemSlotsAsync(cancellationToken);
        session.ResetSystemRows(systemSlots);
        session.EnsureSelectedBuild();
        PresentPlanner(session, plannerPresenter);
        PresentSession(session, sessionPresenter);
    }

    public async Task LoadSavedStateAsync(
        Upgrades2PlannerSession session,
        IUpgrades2PlannerPresenter plannerPresenter,
        IUpgrades2PlannerSessionPresenter sessionPresenter,
        CancellationToken cancellationToken = default)
    {
        if (session.SystemPlans.Count == 0)
        {
            return;
        }

        var collection = await stateGateway.LoadBuildCollectionAsync(cancellationToken);
        session.LoadBuildCollection(collection);
        PresentPlanner(session, plannerPresenter);
        PresentSession(session, sessionPresenter);
    }

    public Task CycleAircraftLevelAsync(
        Upgrades2PlannerSession session,
        int level,
        IUpgrades2PlannerPresenter plannerPresenter,
        IUpgrades2PlannerSessionPresenter sessionPresenter,
        CancellationToken cancellationToken = default)
    {
        session.CycleAircraftLevel(level);
        return RecalculateSaveAndPresentAsync(session, plannerPresenter, sessionPresenter, cancellationToken);
    }

    public Task CycleMasteryLevelAsync(
        Upgrades2PlannerSession session,
        int level,
        IUpgrades2PlannerPresenter plannerPresenter,
        IUpgrades2PlannerSessionPresenter sessionPresenter,
        CancellationToken cancellationToken = default)
    {
        session.CycleMasteryLevel(level);
        return RecalculateSaveAndPresentAsync(session, plannerPresenter, sessionPresenter, cancellationToken);
    }

    public Task CycleGoldMasteryStatusAsync(
        Upgrades2PlannerSession session,
        IUpgrades2PlannerPresenter plannerPresenter,
        IUpgrades2PlannerSessionPresenter sessionPresenter,
        CancellationToken cancellationToken = default)
    {
        session.CycleGoldMasteryStatus();
        return RecalculateSaveAndPresentAsync(session, plannerPresenter, sessionPresenter, cancellationToken);
    }

    public async Task CycleNodeAsync(
        Upgrades2PlannerSession session,
        string systemSlotId,
        int level,
        string? branchCode,
        IUpgrades2PlannerPresenter plannerPresenter,
        IUpgrades2PlannerSessionPresenter sessionPresenter,
        CancellationToken cancellationToken = default)
    {
        var row = session.SystemPlans.FirstOrDefault(system => system.SystemSlotId == systemSlotId);
        if (row is null)
        {
            return;
        }

        row.Cycle(level, branchCode);
        await RecalculateSaveAndPresentAsync(session, plannerPresenter, sessionPresenter, cancellationToken);
    }

    public Task StartNewBuildAsync(
        Upgrades2PlannerSession session,
        IUpgrades2PlannerPresenter plannerPresenter,
        IUpgrades2PlannerSessionPresenter sessionPresenter,
        CancellationToken cancellationToken = default)
    {
        session.CreateNewBuild();
        return RecalculateSaveAndPresentAsync(session, plannerPresenter, sessionPresenter, cancellationToken);
    }

    public Task SelectBuildAsync(
        Upgrades2PlannerSession session,
        string buildId,
        IUpgrades2PlannerPresenter plannerPresenter,
        IUpgrades2PlannerSessionPresenter sessionPresenter,
        CancellationToken cancellationToken = default)
    {
        session.SelectBuild(buildId);
        return RecalculateSaveAndPresentAsync(session, plannerPresenter, sessionPresenter, cancellationToken);
    }

    public Task RenameSelectedBuildAsync(
        Upgrades2PlannerSession session,
        string? name,
        IUpgrades2PlannerPresenter plannerPresenter,
        IUpgrades2PlannerSessionPresenter sessionPresenter,
        CancellationToken cancellationToken = default)
    {
        session.RenameSelectedBuild(name);
        return SaveAndPresentSessionAsync(session, sessionPresenter, cancellationToken);
    }

    public Task DeleteSelectedBuildAsync(
        Upgrades2PlannerSession session,
        IUpgrades2PlannerPresenter plannerPresenter,
        IUpgrades2PlannerSessionPresenter sessionPresenter,
        CancellationToken cancellationToken = default)
    {
        session.DeleteSelectedBuild();
        return RecalculateSaveAndPresentAsync(session, plannerPresenter, sessionPresenter, cancellationToken);
    }

    public void PresentCurrent(
        Upgrades2PlannerSession session,
        IUpgrades2PlannerPresenter plannerPresenter,
        IUpgrades2PlannerSessionPresenter sessionPresenter)
    {
        PresentPlanner(session, plannerPresenter);
        PresentSession(session, sessionPresenter);
    }

    private async Task RecalculateSaveAndPresentAsync(
        Upgrades2PlannerSession session,
        IUpgrades2PlannerPresenter plannerPresenter,
        IUpgrades2PlannerSessionPresenter sessionPresenter,
        CancellationToken cancellationToken)
    {
        PresentPlanner(session, plannerPresenter);
        await stateGateway.SaveBuildCollectionAsync(session.BuildBuildCollection(), cancellationToken);
        PresentSession(session, sessionPresenter);
    }

    private async Task SaveAndPresentSessionAsync(
        Upgrades2PlannerSession session,
        IUpgrades2PlannerSessionPresenter sessionPresenter,
        CancellationToken cancellationToken)
    {
        await stateGateway.SaveBuildCollectionAsync(session.BuildBuildCollection(), cancellationToken);
        PresentSession(session, sessionPresenter);
    }

    private async Task<IReadOnlyList<SystemSlotDefinitionV2>> GetGenericSystemSlotsAsync(CancellationToken cancellationToken)
    {
        var systemSlots = await catalogGateway.GetSystemSlotsAsync(cancellationToken);
        var genericSystemSlots = systemSlots
            .Where(s => s.AircraftId == PlannerV2.GenericAircraftId)
            .DefaultIfEmpty()
            .Where(s => s is not null)
            .Cast<SystemSlotDefinitionV2>()
            .ToArray();

        return genericSystemSlots.Length == 0
            ? PlannerV2.GenericSystemSlots
            : genericSystemSlots;
    }

    private static void PresentPlanner(Upgrades2PlannerSession session, IUpgrades2PlannerPresenter presenter)
    {
        var request = Upgrades2PlannerUseCase.BuildPlannerRequest(session.BuildPlannerInput(), session.SystemSlots);
        presenter.Present(new Upgrades2PlannerResponse(session.SystemSlots, PlannerV2.Plan(request)));
    }

    private void PresentSession(Upgrades2PlannerSession session, IUpgrades2PlannerSessionPresenter presenter)
    {
        presentSessionUseCase.Handle(session, presenter);
    }
}
