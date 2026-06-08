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

    private async Task MarkMasteryLevelAsDesired(int level)
    {
        var masteryLevel = Page
            .GetByRole(AriaRole.Group, new() { Name = "Mastery level nodes", Exact = true })
            .GetByRole(AriaRole.Button, new() { Name = level.ToString(), Exact = true });

        await masteryLevel.ClickAsync();
        await masteryLevel.ClickAsync();
    }

    private async Task MarkGoldMasteryAsPlanned()
    {
        var goldButton = Page.GetByRole(AriaRole.Button, new() { Name = "Gold", Exact = true });

        await goldButton.ClickAsync();
        await goldButton.ClickAsync();
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
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Export JSON" })).ToHaveCountAsync(0);
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Export Markdown" })).ToHaveCountAsync(0);
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Copy Share Summary" })).ToHaveCountAsync(0);
        await Expect(Page.GetByLabel("Export output")).ToHaveCountAsync(0);
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
    public async Task Upgrades2GenericSystemRowsShowAllUpgradeTypesWithoutExportShareControls()
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

        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Export JSON" })).ToHaveCountAsync(0);
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Export Markdown" })).ToHaveCountAsync(0);
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Copy Share Summary" })).ToHaveCountAsync(0);
        await Expect(Page.GetByLabel("Export output")).ToHaveCountAsync(0);
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
    public async Task Upgrades2AircraftSelectingNextLevelDoesNotConvertDesiredPrerequisiteToOwned()
    {
        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.EvaluateAsync("localStorage.clear()");
        await Page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });

        var aircraftLevels = Page.GetByRole(AriaRole.Group, new() { Name = "Aircraft level nodes" });
        var levelSix = aircraftLevels.GetByRole(AriaRole.Button, new() { Name = "6", Exact = true });
        var levelSeven = aircraftLevels.GetByRole(AriaRole.Button, new() { Name = "7", Exact = true });

        await levelSix.ClickAsync();
        await levelSix.ClickAsync();
        await levelSeven.ClickAsync();

        await Expect(levelSix).ToHaveAttributeAsync("data-state", "desired");
        await Expect(levelSeven).ToHaveAttributeAsync("data-state", "desired");
        await Expect(Page.GetByText("Aircraft 5->6")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Aircraft 6->7")).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Upgrades2MasteryControlsRenderOneLevelTrackAndOneGoldToggle()
    {
        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.EvaluateAsync("localStorage.clear()");
        await Page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Mastery", Exact = true })).ToBeVisibleAsync();
        var masteryLevels = Page.GetByRole(AriaRole.Group, new() { Name = "Mastery level nodes", Exact = true });
        var goldButton = Page.GetByRole(AriaRole.Button, new() { Name = "Gold", Exact = true });

        await Expect(masteryLevels.GetByRole(AriaRole.Button, new() { Name = "1", Exact = true })).ToHaveAttributeAsync("data-state", "has");
        await Expect(masteryLevels.GetByRole(AriaRole.Button, new() { Name = "24", Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Group, new() { Name = "Gold mastery level nodes", Exact = true })).ToHaveCountAsync(0);
        await Expect(goldButton).ToHaveAttributeAsync("data-state", "off");
        await goldButton.ClickAsync();
        await Expect(goldButton).ToHaveAttributeAsync("data-state", "has");
        await goldButton.ClickAsync();
        await Expect(goldButton).ToHaveAttributeAsync("data-state", "desired");
    }

    [TestMethod]
    public async Task Upgrades2MasteryRebateReducesNetGrindNeeded()
    {
        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.EvaluateAsync("localStorage.clear()");
        await Page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });

        var aircraftLevels = Page.GetByRole(AriaRole.Group, new() { Name = "Aircraft level nodes" });
        var aircraftLevelSix = aircraftLevels.GetByRole(AriaRole.Button, new() { Name = "6", Exact = true });
        await aircraftLevelSix.ClickAsync();
        await aircraftLevelSix.ClickAsync();
        await MarkMasteryLevelAsDesired(6);
        await MarkGoldMasteryAsPlanned();

        await Expect(Page.GetByText("Mastery Rebate")).ToBeVisibleAsync();
        await Expect(Page.GetByText("AIRCRAFT_PARTS 1,000", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Net Grind Needed")).ToBeVisibleAsync();
        var netGrindNeeded = Page.GetByTestId("net-grind-needed");
        await Expect(netGrindNeeded.GetByText("SILVER 1,000", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(netGrindNeeded.GetByText("AIRCRAFT_PARTS 225", new() { Exact = true })).ToHaveCountAsync(0);
    }

    [TestMethod]
    public async Task Upgrades2MasterySelectingNextLevelDoesNotConvertDesiredPrerequisiteToOwned()
    {
        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.EvaluateAsync("localStorage.clear()");
        await Page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });

        var masteryLevels = Page.GetByRole(AriaRole.Group, new() { Name = "Mastery level nodes", Exact = true });
        var levelSix = masteryLevels.GetByRole(AriaRole.Button, new() { Name = "6", Exact = true });
        var levelSeven = masteryLevels.GetByRole(AriaRole.Button, new() { Name = "7", Exact = true });

        await levelSix.ClickAsync();
        await levelSix.ClickAsync();
        await levelSeven.ClickAsync();

        await Expect(levelSix).ToHaveAttributeAsync("data-state", "desired");
        await Expect(levelSeven).ToHaveAttributeAsync("data-state", "desired");
        await Expect(Page.GetByText("Mastery Rebate")).ToBeVisibleAsync();
        await Expect(Page.GetByText("SILVER 900", new() { Exact = true })).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Upgrades2PersistsAircraftLevelsAndSystemNodesAfterReload()
    {
        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.EvaluateAsync("localStorage.clear()");
        await Page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });

        var aircraftLevels = Page.GetByRole(AriaRole.Group, new() { Name = "Aircraft level nodes" });
        var aircraftLevelEight = aircraftLevels.GetByRole(AriaRole.Button, new() { Name = "8", Exact = true });
        var engines = Page.GetByTestId("system-engines");
        var engineLevelOne = engines.GetByRole(AriaRole.Button, new() { Name = "1", Exact = true });
        var engineLevelTwo = engines.GetByRole(AriaRole.Button, new() { Name = "2", Exact = true });

        await aircraftLevelEight.ClickAsync();
        await aircraftLevelEight.ClickAsync();
        await engineLevelOne.ClickAsync();
        await engineLevelTwo.ClickAsync();
        await engineLevelTwo.ClickAsync();

        await Expect(aircraftLevelEight).ToHaveAttributeAsync("data-state", "desired");
        await Expect(engineLevelOne).ToHaveAttributeAsync("data-state", "has");
        await Expect(engineLevelTwo).ToHaveAttributeAsync("data-state", "desired");
        await Expect(Page).ToHaveTitleAsync("Upgrades 2.0 Planner");
        await Page.WaitForFunctionAsync(
            """
            () => {
                const state = JSON.parse(localStorage.getItem('metalstorming20.upgrades2.state') || '{}');
                const engines = state.systemPlans?.find(plan => plan.systemSlotId === 'generic_engines');
                return state.currentAircraftLevel === 7 &&
                    state.targetAircraftLevel === 8 &&
                    engines?.nodeStates?.['1'] === 'has' &&
                    engines?.nodeStates?.['2'] === 'desired';
            }
            """);

        await Page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });

        aircraftLevels = Page.GetByRole(AriaRole.Group, new() { Name = "Aircraft level nodes" });
        aircraftLevelEight = aircraftLevels.GetByRole(AriaRole.Button, new() { Name = "8", Exact = true });
        engines = Page.GetByTestId("system-engines");
        engineLevelOne = engines.GetByRole(AriaRole.Button, new() { Name = "1", Exact = true });
        engineLevelTwo = engines.GetByRole(AriaRole.Button, new() { Name = "2", Exact = true });

        await Expect(aircraftLevelEight).ToHaveAttributeAsync("data-state", "desired");
        await Expect(engineLevelOne).ToHaveAttributeAsync("data-state", "has");
        await Expect(engineLevelTwo).ToHaveAttributeAsync("data-state", "desired");
        await Expect(Page.GetByText("Engines 1->2")).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Upgrades2RestoresSystemNodesFromLocalStorage()
    {
        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.EvaluateAsync(
            """
            localStorage.setItem('metalstorming20.upgrades2.state', JSON.stringify({
                schemaVersion: 2,
                currentAircraftLevel: 7,
                targetAircraftLevel: 8,
                systemPlans: [
                    {
                        systemSlotId: 'generic_engines',
                        selectedNodes: ['1', '2'],
                        nodeStates: { '1': 'has', '2': 'desired' }
                    }
                ]
            }))
            """);
        await Page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });

        var aircraftLevels = Page.GetByRole(AriaRole.Group, new() { Name = "Aircraft level nodes" });
        var engines = Page.GetByTestId("system-engines");

        await Expect(aircraftLevels.GetByRole(AriaRole.Button, new() { Name = "8", Exact = true })).ToHaveAttributeAsync("data-state", "desired");
        await Expect(engines.GetByRole(AriaRole.Button, new() { Name = "1", Exact = true })).ToHaveAttributeAsync("data-state", "has");
        await Expect(engines.GetByRole(AriaRole.Button, new() { Name = "2", Exact = true })).ToHaveAttributeAsync("data-state", "desired");
        await Expect(Page.GetByText("Engines 1->2")).ToBeVisibleAsync();
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
