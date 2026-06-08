using MetalStorming20.Core;

namespace MetalStorming20.Tests;

public class Upgrades2PlannerStateUseCaseTests
{
    [Fact]
    public async Task SaveAsync_WritesStateThroughGateway()
    {
        var gateway = new RecordingPlannerStateGateway();
        var useCase = new SaveUpgrades2PlannerStateUseCase(gateway);
        var state = ExampleState();

        await useCase.HandleAsync(state);

        Assert.Same(state, gateway.SavedStates.Single());
    }

    [Fact]
    public async Task LoadAsync_ReadsStateThroughGatewayAndPresentsLoadedState()
    {
        var state = ExampleState();
        var gateway = new RecordingPlannerStateGateway(state);
        var presenter = new RecordingPlannerStatePresenter();
        var useCase = new LoadUpgrades2PlannerStateUseCase(gateway);

        await useCase.HandleAsync(presenter);

        Assert.True(gateway.LoadWasCalled);
        Assert.Same(state, Assert.Single(presenter.Responses).State);
    }

    [Fact]
    public async Task SaveBuildCollectionAsync_WritesCollectionThroughGateway()
    {
        var gateway = new RecordingPlannerStateGateway();
        var useCase = new SaveUpgrades2SavedBuildCollectionUseCase(gateway);
        var collection = ExampleBuildCollection();

        await useCase.HandleAsync(collection);

        Assert.Same(collection, gateway.SavedBuildCollections.Single());
    }

    [Fact]
    public async Task LoadBuildCollectionAsync_ReadsCollectionThroughGatewayAndPresentsLoadedCollection()
    {
        var collection = ExampleBuildCollection();
        var gateway = new RecordingPlannerStateGateway(buildCollectionToLoad: collection);
        var presenter = new RecordingSavedBuildCollectionPresenter();
        var useCase = new LoadUpgrades2SavedBuildCollectionUseCase(gateway);

        await useCase.HandleAsync(presenter);

        Assert.True(gateway.LoadBuildCollectionWasCalled);
        Assert.Same(collection, Assert.Single(presenter.Responses).Collection);
    }

    private static Upgrades2SavedBuildCollection ExampleBuildCollection() =>
        new(
            SelectedBuildId: "build-1",
            Builds:
            [
                new Upgrades2SavedBuild("build-1", "unnamed", ExampleState())
            ]);

    private static Upgrades2PlannerState ExampleState() =>
        new(
            SchemaVersion: 2,
            CurrentAircraftLevel: 7,
            TargetAircraftLevel: 8,
            SystemPlans:
            [
                new Upgrades2SavedSystemPlan(
                    "generic_engines",
                    ["1", "2"],
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["1"] = "owned",
                        ["2"] = "desired"
                    })
            ],
            CurrentMasteryLevel: 1,
            PlannedMasteryLevel: 6,
            GoldMasteryStatus: GoldMasteryStatus.Planned.ToString());

    private sealed class RecordingPlannerStateGateway : IUpgrades2PlannerStateGateway
    {
        private readonly Upgrades2PlannerState? stateToLoad;
        private readonly Upgrades2SavedBuildCollection? buildCollectionToLoad;

        public RecordingPlannerStateGateway(
            Upgrades2PlannerState? stateToLoad = null,
            Upgrades2SavedBuildCollection? buildCollectionToLoad = null)
        {
            this.stateToLoad = stateToLoad;
            this.buildCollectionToLoad = buildCollectionToLoad;
        }

        public bool LoadWasCalled { get; private set; }
        public bool LoadBuildCollectionWasCalled { get; private set; }

        public List<Upgrades2PlannerState> SavedStates { get; } = [];
        public List<Upgrades2SavedBuildCollection> SavedBuildCollections { get; } = [];

        public Task<Upgrades2PlannerState?> LoadAsync(CancellationToken cancellationToken = default)
        {
            LoadWasCalled = true;
            return Task.FromResult(stateToLoad);
        }

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

    private sealed class RecordingPlannerStatePresenter : IUpgrades2PlannerStatePresenter
    {
        public List<Upgrades2PlannerStateResponse> Responses { get; } = [];

        public void Present(Upgrades2PlannerStateResponse response)
        {
            Responses.Add(response);
        }
    }

    private sealed class RecordingSavedBuildCollectionPresenter : IUpgrades2SavedBuildCollectionPresenter
    {
        public List<Upgrades2SavedBuildCollectionResponse> Responses { get; } = [];

        public void Present(Upgrades2SavedBuildCollectionResponse response)
        {
            Responses.Add(response);
        }
    }
}
