namespace MetalStorming20.Tests;

public class Upgrades2ControllerArchitectureTests
{
    [Fact]
    public void ControllerDelegatesPlannerWorkflowToSessionInteractor()
    {
        var source = File.ReadAllText(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "MetalStorming20.Web",
            "Controllers",
            "Upgrades2Controller.cs"));

        Assert.Contains("Upgrades2PlannerSessionInteractor", source);
        Assert.DoesNotContain("Upgrades2PlannerUseCase", source);
        Assert.DoesNotContain("LoadUpgrades2PlannerStateUseCase", source);
        Assert.DoesNotContain("SaveUpgrades2PlannerStateUseCase", source);
        Assert.DoesNotContain("PresentUpgrades2PlannerSessionUseCase", source);
        Assert.DoesNotContain("BuildPlannerInput", source);
        Assert.DoesNotContain("BuildState", source);
        Assert.DoesNotContain(".Cycle(", source);
    }
}
