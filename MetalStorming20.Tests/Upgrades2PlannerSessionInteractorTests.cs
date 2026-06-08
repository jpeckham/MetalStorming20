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
        var saved = Assert.Single(stateGateway.SavedBuildCollections);
        var savedBuild = Assert.Single(saved.Builds!);
        Assert.Equal(savedBuild.Id, saved.SelectedBuildId);
        Assert.Equal("has", savedBuild.State.SystemPlans!.Single().NodeStates!["1"]);
        Assert.Equal(2, plannerPresenter.Responses.Count);
        Assert.Equal(2, sessionPresenter.Responses.Count);
    }

    [Fact]
    public async Task StartNewBuildAsync_CreatesSeparateSelectedBuildAndPresentsOutputs()
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

        Assert.Equal(2, session.SavedBuilds.Count);
        Assert.Equal(0, session.CurrentAircraftLevel);
        Assert.Equal(0, session.TargetAircraftLevel);
        Assert.Empty(session.SystemPlans.Single().NodeStates);
        var saved = stateGateway.SavedBuildCollections.Last();
        Assert.Equal(2, saved.Builds!.Count);
        var selectedBuild = saved.Builds.Single(build => build.Id == saved.SelectedBuildId);
        Assert.Equal("unnamed", selectedBuild.Name);
        Assert.Equal(0, selectedBuild.State.CurrentAircraftLevel);
        Assert.Equal(0, selectedBuild.State.TargetAircraftLevel);
        Assert.Equal(0, selectedBuild.State.CurrentMasteryLevel);
        Assert.Equal(0, selectedBuild.State.PlannedMasteryLevel);
        Assert.Empty(selectedBuild.State.SystemPlans!.Single().NodeStates!);
        Assert.Equal(5, plannerPresenter.Responses.Count);
        Assert.Equal(5, sessionPresenter.Responses.Count);
    }

    [Fact]
    public async Task LoadSavedStateAsync_LoadsSavedBuildCollectionAndSelectedBuild()
    {
        var catalogGateway = new RecordingCatalogGateway([
            new SystemSlotDefinitionV2(
                "generic_engines",
                PlannerV2.GenericAircraftId,
                "ENGINES",
                PlannerV2.Currencies.EngineParts,
                "Engines")
        ]);
        var selectedState = new Upgrades2PlannerState(
            2,
            7,
            8,
            [
                new Upgrades2SavedSystemPlan(
                    "generic_engines",
                    ["1"],
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["1"] = "has"
                    })
            ],
            2,
            4,
            GoldMasteryStatus.Owned.ToString());
        var stateGateway = new RecordingStateGateway(
            new Upgrades2SavedBuildCollection(
                "build-2",
                [
                    new Upgrades2SavedBuild("build-1", "First", selectedState with { CurrentAircraftLevel = 3 }),
                    new Upgrades2SavedBuild("build-2", "Second", selectedState)
                ]));
        var plannerPresenter = new RecordingPlannerPresenter();
        var sessionPresenter = new RecordingSessionPresenter();
        var session = new Upgrades2PlannerSession();
        var interactor = new Upgrades2PlannerSessionInteractor(catalogGateway, stateGateway);
        await interactor.InitializeAsync(session, plannerPresenter, sessionPresenter);

        await interactor.LoadSavedStateAsync(session, plannerPresenter, sessionPresenter);

        Assert.True(stateGateway.LoadBuildCollectionWasCalled);
        Assert.Equal("build-2", session.SelectedBuildId);
        Assert.Equal("Second", session.SelectedBuildName);
        Assert.Equal(7, session.CurrentAircraftLevel);
        Assert.Equal(8, session.TargetAircraftLevel);
        Assert.Equal(2, plannerPresenter.Responses.Count);
        Assert.Equal(2, sessionPresenter.Responses.Count);
    }

    [Fact]
    public async Task RenameSelectedBuildAsync_SavesRenamedCollectionAndPresentsSession()
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

        await interactor.RenameSelectedBuildAsync(session, "Interceptor", plannerPresenter, sessionPresenter);

        Assert.Equal("Interceptor", session.SelectedBuildName);
        Assert.Equal("Interceptor", stateGateway.SavedBuildCollections.Last().Builds!.Single().Name);
        Assert.Equal(3, sessionPresenter.Responses.Count);
    }

    [Fact]
    public async Task SelectBuildAsync_LoadsSelectedBuildStateAndPresentsOutputs()
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
        var firstState = session.BuildState();
        var secondState = firstState with { CurrentAircraftLevel = 9, TargetAircraftLevel = 10 };
        session.LoadBuildCollection(new Upgrades2SavedBuildCollection(
            "build-1",
            [
                new Upgrades2SavedBuild("build-1", "First", firstState),
                new Upgrades2SavedBuild("build-2", "Second", secondState)
            ]));

        await interactor.SelectBuildAsync(session, "build-2", plannerPresenter, sessionPresenter);

        Assert.Equal("build-2", session.SelectedBuildId);
        Assert.Equal(9, session.CurrentAircraftLevel);
        Assert.Equal(10, session.TargetAircraftLevel);
        Assert.Equal("build-2", stateGateway.SavedBuildCollections.Last().SelectedBuildId);
    }

    [Fact]
    public async Task DeleteSelectedBuildAsync_RemovesBuildAndSelectsNextBuild()
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
        var firstState = session.BuildState();
        var secondState = firstState with { CurrentAircraftLevel = 9, TargetAircraftLevel = 10 };
        session.LoadBuildCollection(new Upgrades2SavedBuildCollection(
            "build-1",
            [
                new Upgrades2SavedBuild("build-1", "First", firstState),
                new Upgrades2SavedBuild("build-2", "Second", secondState)
            ]));

        await interactor.DeleteSelectedBuildAsync(session, plannerPresenter, sessionPresenter);

        Assert.Equal("build-2", session.SelectedBuildId);
        Assert.Single(session.SavedBuilds);
        Assert.Equal(9, session.CurrentAircraftLevel);
        Assert.Equal(10, session.TargetAircraftLevel);
        Assert.Equal("build-2", stateGateway.SavedBuildCollections.Last().SelectedBuildId);
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
        private readonly Upgrades2SavedBuildCollection? buildCollectionToLoad;

        public RecordingStateGateway(Upgrades2SavedBuildCollection? buildCollectionToLoad = null)
        {
            this.buildCollectionToLoad = buildCollectionToLoad;
        }

        public List<Upgrades2PlannerState> SavedStates { get; } = [];
        public List<Upgrades2SavedBuildCollection> SavedBuildCollections { get; } = [];
        public bool LoadBuildCollectionWasCalled { get; private set; }

        public Task<Upgrades2PlannerState?> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<Upgrades2PlannerState?>(null);

        public Task SaveAsync(Upgrades2PlannerState state, CancellationToken cancellationToken = default)
        {
            SavedStates.Add(state);
            return Task.CompletedTask;
        }

        public Task<Upgrades2SavedBuildCollection?> LoadBuildCollectionAsync(CancellationToken cancellationToken = default)
        {
            LoadBuildCollectionWasCalled = true;
            return Task.FromResult(buildCollectionToLoad);
        }

        public Task SaveBuildCollectionAsync(
            Upgrades2SavedBuildCollection collection,
            CancellationToken cancellationToken = default)
        {
            SavedBuildCollections.Add(collection);
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
