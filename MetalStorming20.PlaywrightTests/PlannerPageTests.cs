using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace MetalStorming20.PlaywrightTests;

[TestClass]
public class PlannerPageTests : PageTest
{
    private string BaseUrl =>
        Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL") ??
        TestContext.Properties["BaseUrl"] as string ??
        "http://localhost:5000/";

    [TestMethod]
    public async Task HomePageLoadsAndCalculates()
    {
        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "MetalStorm Planner" })).ToBeVisibleAsync();

        await Page.GetByLabel("Current Plane Level (1-20)").FillAsync("5");
        await Page.GetByLabel("Current Mastery Level (1-23)").FillAsync("3");
        await Page.GetByLabel("Target Mastery Level (1-23)").FillAsync("6");
        await Page.GetByLabel("Current Universal Parts").FillAsync("2000");
        await Page.GetByLabel("Current Silver").FillAsync("500000");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Calculate Requirements" }).ClickAsync();

        await Expect(Page.GetByText("Requirements from Level 5 → 20")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Future Mastery Rewards")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Remaining Needed from NON-MASTERY Sources")).ToBeVisibleAsync();
    }
}
