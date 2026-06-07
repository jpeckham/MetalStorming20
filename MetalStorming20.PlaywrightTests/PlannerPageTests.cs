using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MetalStorming20.PlaywrightTests;

[TestClass]
public class PlannerPageTests : PageTest
{
    private string BaseUrl =>
        Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL") ??
        TestContext.Properties["BaseUrl"] as string ??
        throw new NullReferenceException("BaseURL");

    private async Task MarkAircraftLevelAsHas(int level)
    {
        var aircraftLevel = Page
            .GetByRole(AriaRole.Group, new() { Name = "Aircraft level nodes" })
            .GetByRole(AriaRole.Button, new() { Name = level.ToString(), Exact = true });

        await aircraftLevel.ClickAsync();
    }

    private static async Task MarkSystemNodeAsDesired(ILocator systemRow, string node)
    {
        var button = systemRow.GetByRole(AriaRole.Button, new() { Name = node });
        await button.ClickAsync();
        await button.ClickAsync();
    }

    [TestMethod]
    public async Task HomePageLoadsNewPlannerWithoutNavigation()
    {
        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.EvaluateAsync("localStorage.clear()");
        await Page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Upgrades 2.0 Planner" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Navigation)).ToHaveCountAsync(0);
        await Expect(Page.GetByText("MetalStorm Planner")).ToHaveCountAsync(0);
        await Expect(Page.GetByRole(AriaRole.Rowheader, new() { Name = "Main/Radar Missile" })).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Upgrades2PageLoadsAndCalculates()
    {
        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.EvaluateAsync("localStorage.clear()");
        await Page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Upgrades 2.0 Planner" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Add Aircraft" })).ToHaveCountAsync(0);
        await Expect(Page.GetByLabel("Aircraft", new() { Exact = true })).ToHaveCountAsync(0);

        var engines = Page.GetByTestId("system-engines");
        await Expect(engines.GetByRole(AriaRole.Button, new() { Name = "1" })).ToBeVisibleAsync();
        await Expect(engines.GetByRole(AriaRole.Button, new() { Name = "5A" })).ToBeVisibleAsync();
        await Expect(engines.GetByRole(AriaRole.Button, new() { Name = "5B" })).ToBeVisibleAsync();
        await MarkSystemNodeAsDesired(engines, "1");
        await MarkSystemNodeAsDesired(engines, "2");
        await MarkSystemNodeAsDesired(engines, "3");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Planner Results" })).ToBeVisibleAsync();
        await Expect(Page.GetByText("SILVER 2,900")).ToBeVisibleAsync();
        await Expect(Page.GetByText("AIRCRAFT_PARTS 225", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("ENGINE_PARTS 950", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Aircraft", Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Aircraft 5->6")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Export JSON" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Export Markdown" })).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Upgrades2CalculatesAutomaticallyWithoutResourceBalances()
    {
        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.EvaluateAsync("localStorage.clear()");
        await Page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Resource Balances" })).ToHaveCountAsync(0);
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Run Planner" })).ToHaveCountAsync(0);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Planner Results" })).ToBeVisibleAsync();

        var engines = Page.GetByTestId("system-engines");
        await MarkSystemNodeAsDesired(engines, "1");
        await MarkSystemNodeAsDesired(engines, "2");
        await MarkSystemNodeAsDesired(engines, "3");

        await Expect(Page.GetByText("SILVER 2,900")).ToBeVisibleAsync();
        await Expect(Page.GetByText("AIRCRAFT_PARTS 225", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("ENGINE_PARTS 950", new() { Exact = true })).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Upgrades2GenericSystemRowsShowAllUpgradeTypesAndCopyShareSummary()
    {
        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.EvaluateAsync("localStorage.clear()");
        await Page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });

        await Expect(Page.GetByRole(AriaRole.Rowheader, new() { Name = "Fuselage" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Rowheader, new() { Name = "Engines" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Rowheader, new() { Name = "Avionics" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Rowheader, new() { Name = "Cannons" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Rowheader, new() { Name = "Main/Radar Missile" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Rowheader, new() { Name = "Secondary/IR Missile" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Rowheader, new() { Name = "Rockets" })).ToBeVisibleAsync();

        await MarkAircraftLevelAsHas(6);
        await MarkSystemNodeAsDesired(Page.GetByTestId("system-cannons"), "1");
        await MarkSystemNodeAsDesired(Page.GetByTestId("system-main-radar-missile"), "1");
        await MarkSystemNodeAsDesired(Page.GetByTestId("system-secondary-ir-missile"), "1");
        await MarkSystemNodeAsDesired(Page.GetByTestId("system-rockets"), "1");

        await Expect(Page.GetByText("SILVER 1,600")).ToBeVisibleAsync();
        await Expect(Page.GetByText("CANNON_PARTS 200", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("MISSILE_PARTS 400", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("ROCKET_PARTS 200", new() { Exact = true })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Copy Share Summary" }).ClickAsync();
        await Expect(Page.GetByLabel("Export output")).ToContainTextAsync("# MetalStorming20 Upgrades 2.0 Plan");
        await Expect(Page.GetByLabel("Export output")).ToContainTextAsync("Main/Radar Missile");
        await Expect(Page.GetByLabel("Export output")).ToContainTextAsync("Secondary/IR Missile");
    }

    [TestMethod]
    public async Task Upgrades2NodeButtonsTrackHasAndDesiredStates()
    {
        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.EvaluateAsync("localStorage.clear()");
        await Page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });

        await MarkAircraftLevelAsHas(6);

        var engines = Page.GetByTestId("system-engines");
        var levelOne = engines.GetByRole(AriaRole.Button, new() { Name = "1" });
        var levelTwo = engines.GetByRole(AriaRole.Button, new() { Name = "2" });

        await levelOne.ClickAsync();
        await Expect(levelOne).ToHaveAttributeAsync("data-state", "has");
        await levelOne.ClickAsync();
        await Expect(levelOne).ToHaveAttributeAsync("data-state", "desired");
        await levelOne.ClickAsync();
        await Expect(levelOne).ToHaveAttributeAsync("data-state", "off");
        await levelOne.ClickAsync();
        await Expect(levelOne).ToHaveAttributeAsync("data-state", "has");
        await levelTwo.ClickAsync();
        await Expect(levelTwo).ToHaveAttributeAsync("data-state", "has");
        await levelTwo.ClickAsync();
        await Expect(levelTwo).ToHaveAttributeAsync("data-state", "desired");

        await Expect(Page.GetByText("SILVER 600", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("ENGINE_PARTS 300", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Engines 1->2")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Engines 0->1")).ToHaveCountAsync(0);
    }

    [TestMethod]
    public async Task Upgrades2AircraftLevelButtonsTrackHasAndDesiredStates()
    {
        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.EvaluateAsync("localStorage.clear()");
        await Page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });

        var aircraftLevels = Page.GetByRole(AriaRole.Group, new() { Name = "Aircraft level nodes" });
        var levelEight = aircraftLevels.GetByRole(AriaRole.Button, new() { Name = "8", Exact = true });

        await levelEight.ClickAsync();

        foreach (var level in Enumerable.Range(1, 7))
        {
            await Expect(aircraftLevels.GetByRole(AriaRole.Button, new() { Name = level.ToString(), Exact = true })).ToHaveAttributeAsync("data-state", "has");
        }

        await Expect(levelEight).ToHaveAttributeAsync("data-state", "has");
        await levelEight.ClickAsync();
        await Expect(levelEight).ToHaveAttributeAsync("data-state", "desired");

        await Expect(Page.GetByText("Aircraft 7->8")).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Upgrades2JumpingToBranchNodeFillsPrerequisitesAsOwned()
    {
        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.EvaluateAsync("localStorage.clear()");
        await Page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });

        await Expect(Page.GetByText("Green owned nodes are your current state.")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Blue desired nodes are your target state.")).ToBeVisibleAsync();

        var engines = Page.GetByTestId("system-engines");
        await engines.GetByRole(AriaRole.Button, new() { Name = "7A" }).ClickAsync();
        await engines.GetByRole(AriaRole.Button, new() { Name = "7A" }).ClickAsync();

        foreach (var node in new[] { "1", "2", "3", "4", "5A", "6A" })
        {
            await Expect(engines.GetByRole(AriaRole.Button, new() { Name = node })).ToHaveAttributeAsync("data-state", "has");
        }

        await Expect(engines.GetByRole(AriaRole.Button, new() { Name = "7A" })).ToHaveAttributeAsync("data-state", "desired");
        await Expect(engines.GetByRole(AriaRole.Button, new() { Name = "5B" })).ToHaveAttributeAsync("data-state", "off");
        await Expect(engines.GetByRole(AriaRole.Button, new() { Name = "6B" })).ToHaveAttributeAsync("data-state", "off");
    }

    [TestMethod]
    public async Task Upgrades2SwitchingBranchesDoesNotFillAlternateBranchPrerequisitesWhenLevelPathIsOwned()
    {
        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.EvaluateAsync("localStorage.clear()");
        await Page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });

        var engines = Page.GetByTestId("system-engines");
        await engines.GetByRole(AriaRole.Button, new() { Name = "6A" }).ClickAsync();

        await Expect(engines.GetByRole(AriaRole.Button, new() { Name = "5A" })).ToHaveAttributeAsync("data-state", "has");
        await Expect(engines.GetByRole(AriaRole.Button, new() { Name = "6A" })).ToHaveAttributeAsync("data-state", "has");

        await engines.GetByRole(AriaRole.Button, new() { Name = "7B" }).ClickAsync();
        await engines.GetByRole(AriaRole.Button, new() { Name = "7B" }).ClickAsync();

        await Expect(engines.GetByRole(AriaRole.Button, new() { Name = "7B" })).ToHaveAttributeAsync("data-state", "desired");
        await Expect(engines.GetByRole(AriaRole.Button, new() { Name = "5B" })).ToHaveAttributeAsync("data-state", "off");
        await Expect(engines.GetByRole(AriaRole.Button, new() { Name = "6B" })).ToHaveAttributeAsync("data-state", "off");
    }

    [TestMethod]
    public async Task Upgrades2SelectingNextBranchLevelDoesNotConvertDesiredPrerequisiteToOwned()
    {
        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.EvaluateAsync("localStorage.clear()");
        await Page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });

        var engines = Page.GetByTestId("system-engines");
        await engines.GetByRole(AriaRole.Button, new() { Name = "6A" }).ClickAsync();
        await MarkSystemNodeAsDesired(engines, "7A");
        await MarkSystemNodeAsDesired(engines, "8A");

        await Expect(engines.GetByRole(AriaRole.Button, new() { Name = "5A" })).ToHaveAttributeAsync("data-state", "has");
        await Expect(engines.GetByRole(AriaRole.Button, new() { Name = "6A" })).ToHaveAttributeAsync("data-state", "has");
        await Expect(engines.GetByRole(AriaRole.Button, new() { Name = "7A" })).ToHaveAttributeAsync("data-state", "desired");
        await Expect(engines.GetByRole(AriaRole.Button, new() { Name = "8A" })).ToHaveAttributeAsync("data-state", "desired");
    }
}
