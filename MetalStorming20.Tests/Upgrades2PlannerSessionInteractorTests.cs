using MetalStorming20.Core;

namespace MetalStorming20.Tests;

public class Upgrades2PlannerSessionInteractorTests
{
    [Fact]
    public async Task InitializeAsync_LoadsCatalogPlansAndPresentsSession()
    {
        var catalogGateway = new RecordingCatalogGateway([
            new SystemSlotDefinitionV2(
                "generic_engines",
                PlannerV2.GenericAircraftId,
                "ENGINES",
                PlannerV2.Currencies.EngineParts,
                "Engines")
        ]);
        var stateGateway = new RecordingStateGateway();
        var plannerPresenter = new RecordingPlannerPresenter();
        var sessionPresenter = new RecordingSessionPresenter();
        var session = new Upgrades2PlannerSession();
        var interactor = new Upgrades2PlannerSessionInteractor(catalogGateway, stateGateway);

        await interactor.InitializeAsync(session, plannerPresenter, sessionPresenter);

        Assert.True(catalogGateway.WasCalled);
        Assert.Equal("generic_engines", Assert.Single(session.SystemPlans).SystemSlotId);
        Assert.Single(plannerPresenter.Responses);
        Assert.Single(sessionPresenter.Responses);
        Assert.Empty(stateGateway.SavedStates);
    }

    [Fact]
    public async Task CycleNodeAsync_MutatesSessionSavesStateAndPresentsOutputs()
    {
        var catalogGateway = new RecordingCatalogGateway([
            new SystemSlotDefinitionV2(
                "generic_engines",
                PlannerV2.GenericAircraftId,
                "ENGINES",
                PlannerV2.Currencies.EngineParts,
                "Engines")
        ]);
        var stateGateway = new RecordingStateGateway();
        var plannerPresenter = new RecordingPlannerPresenter();
        var sessionPresenter = new RecordingSessionPresenter();
        var session = new Upgrades2PlannerSession();
        var interactor = new Upgrades2PlannerSessionInteractor(catalogGateway, stateGateway);
        await interactor.InitializeAsync(session, plannerPresenter, sessionPresenter);

        await interactor.CycleNodeAsync(session, "generic_engines", 1, null, plannerPresenter, sessionPresenter);

        Assert.Equal(Upgrades2NodeSelectionState.Owned, session.SystemPlans.Single().StateFor(1, null));
        var saved = Assert.Single(stateGateway.SavedStates);
        Assert.Equal("has", saved.SystemPlans!.Single().NodeStates!["1"]);
        Assert.Equal(2, plannerPresenter.Responses.Count);
        Assert.Equal(2, sessionPresenter.Responses.Count);
    }

    [Fact]
    public async Task StartNewBuildAsync_ClearsSelectionsSavesStateAndPresentsOutputs()
    {
        var catalogGateway = new RecordingCatalogGateway([
            new SystemSlotDefinitionV2(
                "generic_engines",
                PlannerV2.GenericAircraftId,
                "ENGINES",
                PlannerV2.Currencies.EngineParts,
                "Engines")
        ]);
        var stateGateway = new RecordingStateGateway();
        var plannerPresenter = new RecordingPlannerPresenter();
        var sessionPresenter = new RecordingSessionPresenter();
        var session = new Upgrades2PlannerSession();
        var interactor = new Upgrades2PlannerSessionInteractor(catalogGateway, stateGateway);
        await interactor.InitializeAsync(session, plannerPresenter, sessionPresenter);
        await interactor.CycleNodeAsync(session, "generic_engines", 1, null, plannerPresenter, sessionPresenter);
        await interactor.CycleAircraftLevelAsync(session, 8, plannerPresenter, sessionPresenter);
        await interactor.CycleAircraftLevelAsync(session, 8, plannerPresenter, sessionPresenter);

        await interactor.StartNewBuildAsync(session, plannerPresenter, sessionPresenter);

        Assert.Equal(0, session.CurrentAircraftLevel);
        Assert.Equal(0, session.TargetAircraftLevel);
        Assert.Empty(session.SystemPlans.Single().NodeStates);
        var saved = stateGateway.SavedStates.Last();
        Assert.Equal(0, saved.CurrentAircraftLevel);
        Assert.Equal(0, saved.TargetAircraftLevel);
        Assert.Equal(0, saved.CurrentMasteryLevel);
        Assert.Equal(0, saved.PlannedMasteryLevel);
        Assert.Empty(saved.SystemPlans!.Single().NodeStates!);
        Assert.Equal(5, plannerPresenter.Responses.Count);
        Assert.Equal(5, sessionPresenter.Responses.Count);
    }

    private sealed class RecordingCatalogGateway : IUpgrades2CatalogGateway
    {
        private readonly IReadOnlyList<SystemSlotDefinitionV2> systemSlots;

        public RecordingCatalogGateway(IReadOnlyList<SystemSlotDefinitionV2> systemSlots)
        {
            this.systemSlots = systemSlots;
        }

        public bool WasCalled { get; private set; }

        public Task<IReadOnlyList<SystemSlotDefinitionV2>> GetSystemSlotsAsync(CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(systemSlots);
        }
    }

    private sealed class RecordingStateGateway : IUpgrades2PlannerStateGateway
    {
        public List<Upgrades2PlannerState> SavedStates { get; } = [];

        public Task<Upgrades2PlannerState?> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<Upgrades2PlannerState?>(null);

        public Task SaveAsync(Upgrades2PlannerState state, CancellationToken cancellationToken = default)
        {
            SavedStates.Add(state);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingPlannerPresenter : IUpgrades2PlannerPresenter
    {
        public List<Upgrades2PlannerResponse> Responses { get; } = [];

        public void Present(Upgrades2PlannerResponse response)
        {
            Responses.Add(response);
        }
    }

    private sealed class RecordingSessionPresenter : IUpgrades2PlannerSessionPresenter
    {
        public List<Upgrades2PlannerSessionResponse> Responses { get; } = [];

        public void Present(Upgrades2PlannerSessionResponse response)
        {
            Responses.Add(response);
        }
    }
}
