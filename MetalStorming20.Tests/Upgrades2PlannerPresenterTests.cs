using MetalStorming20.Core;
using MetalStorming20.Web.Presenters;

namespace MetalStorming20.Tests;

public class Upgrades2PlannerPresenterTests
{
    [Fact]
    public void Present_MapsPlannerResultToDisplayReadyViewModel()
    {
        var presenter = new Upgrades2PlannerPresenter();
        var response = new Upgrades2PlannerResponse(
            SystemSlots:
            [
                new SystemSlotDefinitionV2(
                    "generic_engines",
                    PlannerV2.GenericAircraftId,
                    "ENGINES",
                    PlannerV2.Currencies.EngineParts,
                    "Engines")
            ],
            Result: new PlannerResultV2(
                Warnings: ["Check target."],
                TotalsRequired:
                [
                    new CurrencyAmountV2(PlannerV2.Currencies.Silver, 2900),
                    new CurrencyAmountV2(PlannerV2.Currencies.EngineParts, 950)
                ],
                Deficits: [],
                Steps:
                [
                    new PlanStepV2(
                        1,
                        PlannerV2.GenericAircraftId,
                        PlanStepScope.Aircraft,
                        5,
                        6,
                        [new CurrencyAmountV2(PlannerV2.Currencies.Silver, 1000)]),
                    new PlanStepV2(
                        2,
                        PlannerV2.GenericAircraftId,
                        PlanStepScope.System,
                        0,
                        1,
                        [new CurrencyAmountV2(PlannerV2.Currencies.EngineParts, 200)],
                        "generic_engines"),
                    new PlanStepV2(
                        3,
                        PlannerV2.GenericAircraftId,
                        PlanStepScope.Mastery,
                        0,
                        1,
                        [new CurrencyAmountV2(PlannerV2.Currencies.Gold, 269)])
                ],
                MasteryRebate: [new CurrencyAmountV2(PlannerV2.Currencies.Silver, 1500)],
                MasteryNormalRebate: [new CurrencyAmountV2(PlannerV2.Currencies.Silver, 900)],
                MasteryGoldRebate: [new CurrencyAmountV2(PlannerV2.Currencies.Silver, 600)],
                NetGrindNeeded: [new CurrencyAmountV2(PlannerV2.Currencies.EngineParts, 750)]));

        presenter.Present(response);

        var result = presenter.ViewModel.Result;
        Assert.NotNull(result);
        Assert.True(result.HasWarnings);
        Assert.Equal(["Check target."], result.Warnings);
        Assert.Equal(["SILVER 2,900", "ENGINE_PARTS 950"], result.TotalsRequired.Select(total => total.DisplayText));
        Assert.Equal(["SILVER 900"], result.MasteryNormalRebate.Select(rebate => rebate.DisplayText));
        Assert.Equal(["SILVER 600"], result.MasteryGoldRebate.Select(rebate => rebate.DisplayText));
        Assert.Equal(["ENGINE_PARTS 750"], result.NetGrindNeeded.Select(net => net.DisplayText));
        Assert.Equal(
            ["", PlannerV2.Currencies.Gold, PlannerV2.Currencies.Silver, PlannerV2.Currencies.AircraftParts],
            presenter.ViewModel.CurrencyFilters.Take(4).Select(filter => filter.Value));
        Assert.Collection(
            result.Steps,
            step =>
            {
                Assert.Equal(1, step.Order);
                Assert.Equal("Aircraft 5->6", step.StepText);
                Assert.Equal("SILVER 1,000", step.CostsText);
            },
            step =>
            {
                Assert.Equal(2, step.Order);
                Assert.Equal("Engines 0->1", step.StepText);
                Assert.Equal("ENGINE_PARTS 200", step.CostsText);
            },
            step =>
            {
                Assert.Equal(3, step.Order);
                Assert.Equal("Gold Mastery", step.StepText);
                Assert.Equal("GOLD 269", step.CostsText);
            });
    }
}
