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

    [TestMethod]
    public async Task HomePageLoadsAndCalculates()
    {
        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "MetalStorm Planner" })).ToBeVisibleAsync();

        var numericInputs = Page.Locator("input.form-control[type='number']");
        await Expect(numericInputs).ToHaveCountAsync(5);
        await numericInputs.Nth(0).FillAsync("5");
        await numericInputs.Nth(1).FillAsync("3");
        await numericInputs.Nth(2).FillAsync("6");
        await numericInputs.Nth(3).FillAsync("2000");
        await numericInputs.Nth(4).FillAsync("500000");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Calculate Requirements" }).ClickAsync();

        await Expect(Page.GetByText("Requirements from Level 5 → 20")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Future Mastery Rewards")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Remaining Needed from NON-MASTERY Sources")).ToBeVisibleAsync();
    }
}
