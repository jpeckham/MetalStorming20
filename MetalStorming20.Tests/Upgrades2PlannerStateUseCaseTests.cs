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

        public RecordingPlannerStateGateway(Upgrades2PlannerState? stateToLoad = null)
        {
            this.stateToLoad = stateToLoad;
        }

        public bool LoadWasCalled { get; private set; }

        public List<Upgrades2PlannerState> SavedStates { get; } = [];

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
    }

    private sealed class RecordingPlannerStatePresenter : IUpgrades2PlannerStatePresenter
    {
        public List<Upgrades2PlannerStateResponse> Responses { get; } = [];

        public void Present(Upgrades2PlannerStateResponse response)
        {
            Responses.Add(response);
        }
    }
}
