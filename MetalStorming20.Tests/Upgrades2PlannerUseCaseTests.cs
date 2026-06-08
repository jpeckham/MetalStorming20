using MetalStorming20.Core;

namespace MetalStorming20.Tests;

public class Upgrades2PlannerUseCaseTests
{
    [Fact]
    public void BuildPlannerRequest_WhenAircraftLevelsAreClearedOmitsAircraftTarget()
    {
        var request = Upgrades2PlannerUseCase.BuildPlannerRequest(
            new Upgrades2PlannerInput(
                0,
                0,
                [],
                null),
            PlannerV2.GenericSystemSlots);

        Assert.Empty(request.Aircraft);
        Assert.Empty(request.AircraftTargets);
    }

    [Fact]
    public async Task HandleAsync_LoadsCatalogThroughGatewayAndPresentsPlannerResponse()
    {
        var gateway = new RecordingCatalogGateway([
            new SystemSlotDefinitionV2(
                "generic_fuselage",
                PlannerV2.GenericAircraftId,
                "FUSELAGE",
                PlannerV2.Currencies.FuselageParts,
                "Fuselage")
        ]);
        var presenter = new RecordingPlannerPresenter();
        var useCase = new Upgrades2PlannerUseCase(gateway);
        var input = new Upgrades2PlannerInput(
            CurrentAircraftLevel: 5,
            TargetAircraftLevel: 6,
            Systems:
            [
                new Upgrades2SystemPlanInput(
                    "generic_fuselage",
                    new Dictionary<string, Upgrades2NodeSelectionState>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["1"] = Upgrades2NodeSelectionState.Owned,
                        ["2"] = Upgrades2NodeSelectionState.Owned,
                        ["3"] = Upgrades2NodeSelectionState.Owned,
                        ["4"] = Upgrades2NodeSelectionState.Owned,
                        ["5A"] = Upgrades2NodeSelectionState.Desired
                    })
            ],
            MasteryPlan: new MasteryPlanV2(1, 1, GoldMasteryStatus.Off));

        await useCase.HandleAsync(input, presenter);

        Assert.True(gateway.WasCalled);
        var response = Assert.Single(presenter.Responses);
        Assert.Empty(response.Result.Warnings);
        Assert.Equal("Fuselage", Assert.Single(response.SystemSlots).DisplayName);
        Assert.Contains(response.Result.Steps, step =>
            step.Scope == PlanStepScope.Aircraft &&
            step.FromLevel == 5 &&
            step.ToLevel == 6);
        Assert.Contains(response.Result.Steps, step =>
            step.Scope == PlanStepScope.System &&
            step.SystemSlotId == "generic_fuselage" &&
            step.FromLevel == 4 &&
            step.ToLevel == 5 &&
            step.BranchCode == "A");
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

    private sealed class RecordingPlannerPresenter : IUpgrades2PlannerPresenter
    {
        public List<Upgrades2PlannerResponse> Responses { get; } = [];

        public void Present(Upgrades2PlannerResponse response)
        {
            Responses.Add(response);
        }
    }
}
