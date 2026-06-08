using MetalStorming20.Core;

namespace MetalStorming20.Tests;

public class Upgrades2PlannerSessionViewUseCaseTests
{
    [Fact]
    public void Handle_PresentsSessionSnapshot()
    {
        var session = new Upgrades2PlannerSession();
        session.ResetSystemRows([
            new SystemSlotDefinitionV2(
                "generic_engines",
                PlannerV2.GenericAircraftId,
                "ENGINES",
                PlannerV2.Currencies.EngineParts,
                "Engines")
        ]);
        session.CycleAircraftLevel(6);
        session.CycleAircraftLevel(6);
        session.CycleGoldMasteryStatus();
        session.SystemPlans.Single().Cycle(1, null);
        var presenter = new RecordingSessionPresenter();
        var useCase = new PresentUpgrades2PlannerSessionUseCase();

        useCase.Handle(session, presenter);

        var response = Assert.Single(presenter.Responses);
        Assert.Equal(5, response.CurrentAircraftLevel);
        Assert.Equal(6, response.TargetAircraftLevel);
        Assert.Equal(GoldMasteryStatus.Owned, response.GoldMasteryStatus);
        Assert.Equal(Upgrades2NodeSelectionState.Desired, response.AircraftLevels.Single(level => level.Level == 6).State);
        var system = Assert.Single(response.Systems);
        Assert.Equal("generic_engines", system.SystemSlotId);
        Assert.Equal("Engines", system.DisplayName);
        Assert.Equal(Upgrades2NodeSelectionState.Owned, system.Nodes.Single(node => node.Level == 1 && node.BranchCode is null).State);
    }

    [Fact]
    public void Handle_PresentsAbilityRowsAsUnbranchedTracksWithCatalogMaxLevels()
    {
        var session = new Upgrades2PlannerSession();
        session.ResetSystemRows([
            new SystemSlotDefinitionV2(
                "generic_special",
                PlannerV2.GenericAircraftId,
                "SPECIAL",
                PlannerV2.Currencies.SpecialAbilityBlueprints,
                "Special",
                8,
                3,
                false,
                "SPECIAL"),
            new SystemSlotDefinitionV2(
                "generic_passive",
                PlannerV2.GenericAircraftId,
                "PASSIVE",
                PlannerV2.Currencies.PassiveAbilityBlueprints,
                "Passive",
                12,
                5,
                false,
                "PASSIVE")
        ]);
        var presenter = new RecordingSessionPresenter();
        var useCase = new PresentUpgrades2PlannerSessionUseCase();

        useCase.Handle(session, presenter);

        var response = Assert.Single(presenter.Responses);
        var special = response.Systems.Single(system => system.SystemSlotId == "generic_special");
        var passive = response.Systems.Single(system => system.SystemSlotId == "generic_passive");
        Assert.Equal([1, 2, 3], special.Nodes.Select(node => node.Level));
        Assert.All(special.Nodes, node => Assert.Null(node.BranchCode));
        Assert.Equal([1, 2, 3, 4, 5], passive.Nodes.Select(node => node.Level));
        Assert.All(passive.Nodes, node => Assert.Null(node.BranchCode));
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
