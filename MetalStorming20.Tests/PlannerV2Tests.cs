using System.Text.Json;
using MetalStorming20.Core;

namespace MetalStorming20.Tests;

public class PlannerV2CostTests
{
    [Fact]
    public void AircraftCostLookup_ReturnsSeedRows()
    {
        var fourToFive = PlannerV2.GetAircraftUpgradeCost(4, 5);
        var fiveToSix = PlannerV2.GetAircraftUpgradeCost(5, 6);

        Assert.Equal(600, fourToFive.SilverCost);
        Assert.Equal(150, fourToFive.AircraftPartsCost);
        Assert.Equal(1000, fiveToSix.SilverCost);
        Assert.Equal(225, fiveToSix.AircraftPartsCost);
    }

    [Fact]
    public void AircraftCostSum_FromOneToTwenty_ReturnsPublishedAggregate()
    {
        var total = PlannerV2.SumAircraftCosts(1, 20);

        Assert.Equal(50000, total[PlannerV2.Currencies.Silver]);
        Assert.Equal(11200, total[PlannerV2.Currencies.AircraftParts]);
    }

    [Fact]
    public void SystemCostLookup_ReturnsSeedRows()
    {
        var zeroToOne = PlannerV2.GetSystemUpgradeCost(0, 1);
        var fourToFive = PlannerV2.GetSystemUpgradeCost(4, 5);

        Assert.Equal(400, zeroToOne.SilverCost);
        Assert.Equal(200, zeroToOne.SystemPartsCost);
        Assert.Equal(0, zeroToOne.AdvancedPartsCost);
        Assert.Equal(2100, fourToFive.SilverCost);
        Assert.Equal(850, fourToFive.SystemPartsCost);
        Assert.Equal(1, fourToFive.AdvancedPartsCost);
    }

    [Fact]
    public void SystemChosenOnlySum_FromZeroToEight_ReturnsPublishedAggregate()
    {
        var total = PlannerV2.SumSystemCosts(0, 8, BranchOwnershipMode.ChosenOnly, PlannerV2.Currencies.EngineParts);

        Assert.Equal(20600, total[PlannerV2.Currencies.Silver]);
        Assert.Equal(8650, total[PlannerV2.Currencies.EngineParts]);
        Assert.Equal(4, total[PlannerV2.Currencies.AdvancedParts]);
    }

    [Fact]
    public void SystemBothBranchesSum_FromZeroToEight_DoublesOnlyBranchLevels()
    {
        var total = PlannerV2.SumSystemCosts(0, 8, BranchOwnershipMode.Both, PlannerV2.Currencies.EngineParts);

        Assert.Equal(38000, total[PlannerV2.Currencies.Silver]);
        Assert.Equal(15750, total[PlannerV2.Currencies.EngineParts]);
        Assert.Equal(8, total[PlannerV2.Currencies.AdvancedParts]);
    }

    [Fact]
    public void SpecialCostSum_FromZeroToThree_ReturnsAbilityBlueprintRows()
    {
        var total = PlannerV2.SumAbilityCosts("SPECIAL", 0, 3, PlannerV2.Currencies.SpecialAbilityBlueprints);

        Assert.Equal(35000, total[PlannerV2.Currencies.Silver]);
        Assert.Equal(8, total[PlannerV2.Currencies.SpecialAbilityBlueprints]);
    }

    [Fact]
    public void PassiveCostSum_FromZeroToFive_ReturnsAbilityBlueprintRows()
    {
        var total = PlannerV2.SumAbilityCosts("PASSIVE", 0, 5, PlannerV2.Currencies.PassiveAbilityBlueprints);

        Assert.Equal(75000, total[PlannerV2.Currencies.Silver]);
        Assert.Equal(19, total[PlannerV2.Currencies.PassiveAbilityBlueprints]);
    }
}

public class PlannerV2MasteryTests
{
    [Fact]
    public void Plan_NonGoldMasteryRebate_ReducesNetGrindNeeded()
    {
        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2(PlannerV2.GenericAircraftId, true, 1)],
            OwnedSystemNodes: [],
            EquippedSystemBranches: [],
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2(PlannerV2.GenericAircraftId, 20)],
            SystemTargets: [],
            SystemSlots: PlannerV2.GenericSystemSlots,
            MasteryPlan: new MasteryPlanV2(1, 23, GoldMasteryStatus.Off)));

        Assert.Empty(result.Warnings);
        Assert.Equal(1900, result.MasteryRebate.Single(c => c.CurrencyCode == PlannerV2.Currencies.AircraftParts).Amount);
        Assert.Equal(4400, result.MasteryRebate.Single(c => c.CurrencyCode == PlannerV2.Currencies.Silver).Amount);
        Assert.Equal(9300, result.NetGrindNeeded.Single(c => c.CurrencyCode == PlannerV2.Currencies.AircraftParts).Amount);
        Assert.Equal(45600, result.NetGrindNeeded.Single(c => c.CurrencyCode == PlannerV2.Currencies.Silver).Amount);
    }

    [Fact]
    public void Plan_GoldMasteryRebate_IncludesGoldBonusOnTopOfNonGoldRewards()
    {
        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2(PlannerV2.GenericAircraftId, true, 1)],
            OwnedSystemNodes: [],
            EquippedSystemBranches: [],
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2(PlannerV2.GenericAircraftId, 20)],
            SystemTargets: [],
            SystemSlots: PlannerV2.GenericSystemSlots,
            MasteryPlan: new MasteryPlanV2(1, 23, GoldMasteryStatus.Planned)));

        Assert.Empty(result.Warnings);
        Assert.Equal(1900, result.MasteryNormalRebate.Single(c => c.CurrencyCode == PlannerV2.Currencies.AircraftParts).Amount);
        Assert.Equal(4400, result.MasteryNormalRebate.Single(c => c.CurrencyCode == PlannerV2.Currencies.Silver).Amount);
        Assert.Equal(3000, result.MasteryGoldRebate.Single(c => c.CurrencyCode == PlannerV2.Currencies.AircraftParts).Amount);
        Assert.Equal(12500, result.MasteryGoldRebate.Single(c => c.CurrencyCode == PlannerV2.Currencies.Silver).Amount);
        Assert.Equal(4900, result.MasteryRebate.Single(c => c.CurrencyCode == PlannerV2.Currencies.AircraftParts).Amount);
        Assert.Equal(16900, result.MasteryRebate.Single(c => c.CurrencyCode == PlannerV2.Currencies.Silver).Amount);
        Assert.Equal(6300, result.NetGrindNeeded.Single(c => c.CurrencyCode == PlannerV2.Currencies.AircraftParts).Amount);
        Assert.Equal(33100, result.NetGrindNeeded.Single(c => c.CurrencyCode == PlannerV2.Currencies.Silver).Amount);
    }

    [Fact]
    public void Plan_PlannedGoldMastery_AddsGoldPurchaseCostToTotalsAndNetGrindNeeded()
    {
        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2(PlannerV2.GenericAircraftId, true, 5)],
            OwnedSystemNodes: [],
            EquippedSystemBranches: [],
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2(PlannerV2.GenericAircraftId, 5)],
            SystemTargets: [],
            SystemSlots: PlannerV2.GenericSystemSlots,
            MasteryPlan: new MasteryPlanV2(1, 1, GoldMasteryStatus.Planned)));

        Assert.Empty(result.Warnings);
        Assert.Equal(269, result.TotalsRequired.Single(c => c.CurrencyCode == PlannerV2.Currencies.Gold).Amount);
        Assert.Equal(269, result.NetGrindNeeded.Single(c => c.CurrencyCode == PlannerV2.Currencies.Gold).Amount);
        var step = Assert.Single(result.Steps);
        Assert.Equal(PlanStepScope.Mastery, step.Scope);
        Assert.Equal(269, step.Costs.Single(c => c.CurrencyCode == PlannerV2.Currencies.Gold).Amount);
    }

    [Fact]
    public void Plan_OwnedGoldMastery_DoesNotAddGoldPurchaseCost()
    {
        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2(PlannerV2.GenericAircraftId, true, 5)],
            OwnedSystemNodes: [],
            EquippedSystemBranches: [],
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2(PlannerV2.GenericAircraftId, 5)],
            SystemTargets: [],
            SystemSlots: PlannerV2.GenericSystemSlots,
            MasteryPlan: new MasteryPlanV2(1, 1, GoldMasteryStatus.Owned)));

        Assert.Empty(result.Warnings);
        Assert.Empty(result.Steps);
        Assert.DoesNotContain(result.TotalsRequired, c => c.CurrencyCode == PlannerV2.Currencies.Gold);
        Assert.DoesNotContain(result.NetGrindNeeded, c => c.CurrencyCode == PlannerV2.Currencies.Gold);
    }

    [Fact]
    public void Plan_OwnedGoldMastery_DoesNotRebateGoldRewardsFromGreenMasteryBoxes()
    {
        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2(PlannerV2.GenericAircraftId, true, 7)],
            OwnedSystemNodes: [],
            EquippedSystemBranches: [],
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2(PlannerV2.GenericAircraftId, 20)],
            SystemTargets: [],
            SystemSlots: PlannerV2.GenericSystemSlots,
            MasteryPlan: new MasteryPlanV2(7, 7, GoldMasteryStatus.Owned)));

        Assert.Empty(result.MasteryRebate);
    }

    [Fact]
    public void Plan_PlannedGoldMastery_RebatesGoldRewardsFromGreenMasteryBoxes()
    {
        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2(PlannerV2.GenericAircraftId, true, 7)],
            OwnedSystemNodes: [],
            EquippedSystemBranches: [],
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2(PlannerV2.GenericAircraftId, 20)],
            SystemTargets: [],
            SystemSlots: PlannerV2.GenericSystemSlots,
            MasteryPlan: new MasteryPlanV2(7, 7, GoldMasteryStatus.Planned)));

        Assert.Empty(result.MasteryNormalRebate);
        Assert.Equal(1000, result.MasteryRebate.Single(c => c.CurrencyCode == PlannerV2.Currencies.AircraftParts).Amount);
        Assert.Equal(1500, result.MasteryRebate.Single(c => c.CurrencyCode == PlannerV2.Currencies.Silver).Amount);
        Assert.Equal(1000, result.MasteryGoldRebate.Single(c => c.CurrencyCode == PlannerV2.Currencies.AircraftParts).Amount);
        Assert.Equal(1500, result.MasteryGoldRebate.Single(c => c.CurrencyCode == PlannerV2.Currencies.Silver).Amount);
    }
}

public class PlannerV2DependencyTests
{
    [Fact]
    public void Plan_SystemTargetOnLevelFiveAircraft_InsertsAircraftUnlockBeforeSystemStep()
    {
        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2("f106_delta_dart", true, 5)],
            OwnedSystemNodes: [],
            EquippedSystemBranches: [],
            ResourceBalances: [
                new ResourceBalanceV2(PlannerV2.Currencies.Silver, 2000),
                new ResourceBalanceV2(PlannerV2.Currencies.AircraftParts, 100),
                new ResourceBalanceV2(PlannerV2.Currencies.EngineParts, 500)
            ],
            AircraftTargets: [new AircraftTargetV2("f106_delta_dart", 5)],
            SystemTargets: [new SystemTargetV2("f106_engines_main", 3, BranchOwnershipMode.ChosenOnly)],
            SystemSlots: PlannerV2.DefaultSystemSlots));

        Assert.Empty(result.Warnings);
        Assert.Equal(4, result.Steps.Count);
        Assert.Equal(PlanStepScope.Aircraft, result.Steps[0].Scope);
        Assert.Equal(5, result.Steps[0].FromLevel);
        Assert.Equal(6, result.Steps[0].ToLevel);
        Assert.Equal(2900, result.TotalsRequired.Single(c => c.CurrencyCode == PlannerV2.Currencies.Silver).Amount);
        Assert.Equal(125, result.Deficits.Single(c => c.CurrencyCode == PlannerV2.Currencies.AircraftParts).Amount);
        Assert.Equal(450, result.Deficits.Single(c => c.CurrencyCode == PlannerV2.Currencies.EngineParts).Amount);
    }

    [Fact]
    public void Plan_SpecialTargetOnLevelSevenAircraft_InsertsAircraftLevelEightUnlockAndUsesSpecialBlueprints()
    {
        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2(PlannerV2.GenericAircraftId, true, 7)],
            OwnedSystemNodes: [],
            EquippedSystemBranches: [],
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2(PlannerV2.GenericAircraftId, 7)],
            SystemTargets: [new SystemTargetV2("generic_special", 3, BranchOwnershipMode.ChosenOnly)],
            SystemSlots: PlannerV2.GenericSystemSlots));

        Assert.Empty(result.Warnings);
        Assert.Equal(4, result.Steps.Count);
        Assert.Equal(PlanStepScope.Aircraft, result.Steps[0].Scope);
        Assert.Equal(7, result.Steps[0].FromLevel);
        Assert.Equal(8, result.Steps[0].ToLevel);
        Assert.Equal(36625, result.TotalsRequired.Single(c => c.CurrencyCode == PlannerV2.Currencies.Silver).Amount);
        Assert.Equal(375, result.TotalsRequired.Single(c => c.CurrencyCode == PlannerV2.Currencies.AircraftParts).Amount);
        Assert.Equal(8, result.TotalsRequired.Single(c => c.CurrencyCode == PlannerV2.Currencies.SpecialAbilityBlueprints).Amount);
    }

    [Fact]
    public void Plan_PassiveTargetOnLevelElevenAircraft_InsertsAircraftLevelTwelveUnlockAndDoesNotRequireBranchChoice()
    {
        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2(PlannerV2.GenericAircraftId, true, 11)],
            OwnedSystemNodes: [],
            EquippedSystemBranches: [],
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2(PlannerV2.GenericAircraftId, 11)],
            SystemTargets: [new SystemTargetV2("generic_passive", 5, BranchOwnershipMode.ChosenOnly)],
            SystemSlots: PlannerV2.GenericSystemSlots));

        Assert.Empty(result.Warnings);
        Assert.Equal(6, result.Steps.Count);
        Assert.Equal(PlanStepScope.Aircraft, result.Steps[0].Scope);
        Assert.Equal(11, result.Steps[0].FromLevel);
        Assert.Equal(12, result.Steps[0].ToLevel);
        Assert.Equal(77950, result.TotalsRequired.Single(c => c.CurrencyCode == PlannerV2.Currencies.Silver).Amount);
        Assert.Equal(650, result.TotalsRequired.Single(c => c.CurrencyCode == PlannerV2.Currencies.AircraftParts).Amount);
        Assert.Equal(19, result.TotalsRequired.Single(c => c.CurrencyCode == PlannerV2.Currencies.PassiveAbilityBlueprints).Amount);
    }

    [Fact]
    public void Plan_EquipOnlyChangeWhenBothLevelFiveBranchesOwned_HasZeroCost()
    {
        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2("f106_delta_dart", true, 6)],
            OwnedSystemNodes: [
                new OwnedSystemNodeV2("f106_engines_main", 1, null),
                new OwnedSystemNodeV2("f106_engines_main", 2, null),
                new OwnedSystemNodeV2("f106_engines_main", 3, null),
                new OwnedSystemNodeV2("f106_engines_main", 4, null),
                new OwnedSystemNodeV2("f106_engines_main", 5, "A"),
                new OwnedSystemNodeV2("f106_engines_main", 5, "B")
            ],
            EquippedSystemBranches: [new EquippedSystemBranchV2("f106_engines_main", 5, "A")],
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2("f106_delta_dart", 6)],
            SystemTargets: [new SystemTargetV2("f106_engines_main", 5, BranchOwnershipMode.ChosenOnly, new Dictionary<int, string> { [5] = "B" })],
            SystemSlots: PlannerV2.DefaultSystemSlots));

        Assert.Empty(result.Warnings);
        Assert.Empty(result.Steps);
        Assert.Empty(result.TotalsRequired);
        Assert.Empty(result.Deficits);
    }

    [Fact]
    public void Plan_BothModeBelowBranchLevel_MatchesChosenOnly()
    {
        var chosen = PlannerV2.SumSystemCosts(0, 4, BranchOwnershipMode.ChosenOnly, PlannerV2.Currencies.EngineParts);
        var both = PlannerV2.SumSystemCosts(0, 4, BranchOwnershipMode.Both, PlannerV2.Currencies.EngineParts);

        Assert.Equal(chosen, both);
    }

    [Fact]
    public void Plan_BothModeFromFourToFive_DoublesBranchLevelCost()
    {
        var total = PlannerV2.SumSystemCosts(4, 5, BranchOwnershipMode.Both, PlannerV2.Currencies.EngineParts);

        Assert.Equal(4200, total[PlannerV2.Currencies.Silver]);
        Assert.Equal(1700, total[PlannerV2.Currencies.EngineParts]);
        Assert.Equal(2, total[PlannerV2.Currencies.AdvancedParts]);
    }

    [Fact]
    public void Plan_DuplicateSystemSlots_AreTrackedSeparatelyBySlotId()
    {
        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2("f106_delta_dart", true, 6)],
            OwnedSystemNodes: [],
            EquippedSystemBranches: [],
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2("f106_delta_dart", 6)],
            SystemTargets: [
                new SystemTargetV2("f106_missile_left", 1, BranchOwnershipMode.ChosenOnly),
                new SystemTargetV2("f106_missile_right", 2, BranchOwnershipMode.ChosenOnly)
            ],
            SystemSlots: PlannerV2.DefaultSystemSlots));

        Assert.Empty(result.Warnings);
        Assert.Equal(3, result.Steps.Count(s => s.Scope == PlanStepScope.System));
        Assert.Contains(result.Steps, s => s.SystemSlotId == "f106_missile_left" && s.FromLevel == 0 && s.ToLevel == 1);
        Assert.Contains(result.Steps, s => s.SystemSlotId == "f106_missile_right" && s.FromLevel == 0 && s.ToLevel == 1);
        Assert.Contains(result.Steps, s => s.SystemSlotId == "f106_missile_right" && s.FromLevel == 1 && s.ToLevel == 2);
    }

    [Fact]
    public void Plan_MissingBranchChoiceForChosenOnlyTarget_ReturnsWarningAndNoPlan()
    {
        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2("f106_delta_dart", true, 6)],
            OwnedSystemNodes: [],
            EquippedSystemBranches: [],
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2("f106_delta_dart", 6)],
            SystemTargets: [new SystemTargetV2("f106_engines_main", 5, BranchOwnershipMode.ChosenOnly)],
            SystemSlots: PlannerV2.DefaultSystemSlots));

        Assert.Contains(result.Warnings, w => w.Contains("branch choice", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(result.Steps);
    }

    [Fact]
    public void Plan_SystemTargetBelowCurrentOwnedLevel_ReturnsWarningAndNoPlan()
    {
        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2("f106_delta_dart", true, 6)],
            OwnedSystemNodes: [
                new OwnedSystemNodeV2("f106_engines_main", 1, null),
                new OwnedSystemNodeV2("f106_engines_main", 2, null),
                new OwnedSystemNodeV2("f106_engines_main", 3, null)
            ],
            EquippedSystemBranches: [],
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2("f106_delta_dart", 6)],
            SystemTargets: [new SystemTargetV2("f106_engines_main", 2, BranchOwnershipMode.ChosenOnly)],
            SystemSlots: PlannerV2.DefaultSystemSlots));

        Assert.Contains(result.Warnings, w => w.Contains("below current owned level", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(result.Steps);
    }

    [Fact]
    public void Plan_SystemTargetOnUnownedAircraft_ReturnsWarningAndNoPlan()
    {
        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2("f106_delta_dart", false, 1)],
            OwnedSystemNodes: [],
            EquippedSystemBranches: [],
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2("f106_delta_dart", 6)],
            SystemTargets: [new SystemTargetV2("f106_engines_main", 1, BranchOwnershipMode.ChosenOnly)],
            SystemSlots: PlannerV2.DefaultSystemSlots));

        Assert.Contains(result.Warnings, w => w.Contains("unowned aircraft", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(result.Steps);
    }

    [Fact]
    public void Plan_EquipBranchThatIsNotOwned_ReturnsWarningAndNoPlan()
    {
        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2("f106_delta_dart", true, 6)],
            OwnedSystemNodes: [
                new OwnedSystemNodeV2("f106_engines_main", 1, null),
                new OwnedSystemNodeV2("f106_engines_main", 2, null),
                new OwnedSystemNodeV2("f106_engines_main", 3, null),
                new OwnedSystemNodeV2("f106_engines_main", 4, null),
                new OwnedSystemNodeV2("f106_engines_main", 5, "A")
            ],
            EquippedSystemBranches: [new EquippedSystemBranchV2("f106_engines_main", 5, "A")],
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2("f106_delta_dart", 6)],
            SystemTargets: [new SystemTargetV2("f106_engines_main", 5, BranchOwnershipMode.ChosenOnly, new Dictionary<int, string> { [5] = "B" })],
            SystemSlots: PlannerV2.DefaultSystemSlots));

        Assert.Contains(result.Warnings, w => w.Contains("not owned", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(result.Steps);
    }

    [Fact]
    public void Plan_InconsistentBranchGraph_ReturnsWarningAndNoPlan()
    {
        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2("f106_delta_dart", true, 6)],
            OwnedSystemNodes: [
                new OwnedSystemNodeV2("f106_engines_main", 1, null),
                new OwnedSystemNodeV2("f106_engines_main", 2, null),
                new OwnedSystemNodeV2("f106_engines_main", 3, null),
                new OwnedSystemNodeV2("f106_engines_main", 4, null),
                new OwnedSystemNodeV2("f106_engines_main", 6, "B")
            ],
            EquippedSystemBranches: [],
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2("f106_delta_dart", 6)],
            SystemTargets: [new SystemTargetV2("f106_engines_main", 6, BranchOwnershipMode.ChosenOnly, new Dictionary<int, string> { [5] = "B", [6] = "B" })],
            SystemSlots: PlannerV2.DefaultSystemSlots));

        Assert.Contains(result.Warnings, w => w.Contains("inconsistent", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(result.Steps);
    }

    [Fact]
    public void Plan_MixedBranchOwnershipAtPriorLevels_IsConsistent()
    {
        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2(PlannerV2.GenericAircraftId, true, 6)],
            OwnedSystemNodes: [
                new OwnedSystemNodeV2("generic_fuselage", 1, null),
                new OwnedSystemNodeV2("generic_fuselage", 2, null),
                new OwnedSystemNodeV2("generic_fuselage", 3, null),
                new OwnedSystemNodeV2("generic_fuselage", 4, null),
                new OwnedSystemNodeV2("generic_fuselage", 5, "A"),
                new OwnedSystemNodeV2("generic_fuselage", 6, "A"),
                new OwnedSystemNodeV2("generic_fuselage", 7, "B")
            ],
            EquippedSystemBranches: [
                new EquippedSystemBranchV2("generic_fuselage", 5, "A"),
                new EquippedSystemBranchV2("generic_fuselage", 6, "A"),
                new EquippedSystemBranchV2("generic_fuselage", 7, "B")
            ],
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2(PlannerV2.GenericAircraftId, 6)],
            SystemTargets: [
                new SystemTargetV2(
                    "generic_fuselage",
                    8,
                    BranchOwnershipMode.ChosenOnly,
                    new Dictionary<int, string> { [5] = "A", [6] = "A", [7] = "B", [8] = "B" },
                    new Dictionary<int, IReadOnlyList<string>>
                    {
                        [5] = ["A"],
                        [6] = ["A"],
                        [7] = ["B"],
                        [8] = ["B"]
                    })
            ],
            SystemSlots: PlannerV2.GenericSystemSlots));

        Assert.Empty(result.Warnings);
        var step = Assert.Single(result.Steps.Where(s => s.SystemSlotId == "generic_fuselage"));
        Assert.Equal(7, step.FromLevel);
        Assert.Equal(8, step.ToLevel);
        Assert.Equal("B", step.BranchCode);
    }

    [Fact]
    public void Plan_ExplicitAlternateBranchTargetsBelowHighestOwnedLevel_AreCalculated()
    {
        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2(PlannerV2.GenericAircraftId, true, 6)],
            OwnedSystemNodes: [
                new OwnedSystemNodeV2("generic_cannons", 1, null),
                new OwnedSystemNodeV2("generic_cannons", 2, null),
                new OwnedSystemNodeV2("generic_cannons", 3, null),
                new OwnedSystemNodeV2("generic_cannons", 4, null),
                new OwnedSystemNodeV2("generic_cannons", 5, "A"),
                new OwnedSystemNodeV2("generic_cannons", 6, "A"),
                new OwnedSystemNodeV2("generic_cannons", 7, "B"),
                new OwnedSystemNodeV2("generic_cannons", 8, "B")
            ],
            EquippedSystemBranches: [
                new EquippedSystemBranchV2("generic_cannons", 5, "A"),
                new EquippedSystemBranchV2("generic_cannons", 6, "A"),
                new EquippedSystemBranchV2("generic_cannons", 7, "B"),
                new EquippedSystemBranchV2("generic_cannons", 8, "B")
            ],
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2(PlannerV2.GenericAircraftId, 6)],
            SystemTargets: [
                new SystemTargetV2(
                    "generic_cannons",
                    8,
                    BranchOwnershipMode.Both,
                    new Dictionary<int, string> { [5] = "A", [6] = "A", [7] = "B", [8] = "B" },
                    new Dictionary<int, IReadOnlyList<string>>
                    {
                        [5] = ["A", "B"],
                        [6] = ["A", "B"],
                        [7] = ["B"],
                        [8] = ["B"]
                    })
            ],
            SystemSlots: PlannerV2.GenericSystemSlots));

        Assert.Empty(result.Warnings);
        Assert.Collection(
            result.Steps.Where(s => s.SystemSlotId == "generic_cannons"),
            step =>
            {
                Assert.Equal(4, step.FromLevel);
                Assert.Equal(5, step.ToLevel);
                Assert.Equal("B", step.BranchCode);
            },
            step =>
            {
                Assert.Equal(5, step.FromLevel);
                Assert.Equal(6, step.ToLevel);
                Assert.Equal("B", step.BranchCode);
            });
    }

    [Fact]
    public void Plan_TargetEquippedBranchThatIsAlsoPlannedToOwn_DoesNotWarnAsUnowned()
    {
        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2(PlannerV2.GenericAircraftId, true, 6)],
            OwnedSystemNodes: [
                new OwnedSystemNodeV2("generic_cannons", 1, null),
                new OwnedSystemNodeV2("generic_cannons", 2, null),
                new OwnedSystemNodeV2("generic_cannons", 3, null),
                new OwnedSystemNodeV2("generic_cannons", 4, null),
                new OwnedSystemNodeV2("generic_cannons", 5, "B"),
                new OwnedSystemNodeV2("generic_cannons", 6, "B"),
                new OwnedSystemNodeV2("generic_cannons", 7, "B"),
                new OwnedSystemNodeV2("generic_cannons", 8, "B")
            ],
            EquippedSystemBranches: [
                new EquippedSystemBranchV2("generic_cannons", 5, "B"),
                new EquippedSystemBranchV2("generic_cannons", 6, "B"),
                new EquippedSystemBranchV2("generic_cannons", 7, "B"),
                new EquippedSystemBranchV2("generic_cannons", 8, "B")
            ],
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2(PlannerV2.GenericAircraftId, 6)],
            SystemTargets: [
                new SystemTargetV2(
                    "generic_cannons",
                    8,
                    BranchOwnershipMode.ChosenOnly,
                    new Dictionary<int, string> { [5] = "B", [6] = "B", [7] = "A", [8] = "A" },
                    new Dictionary<int, IReadOnlyList<string>>
                    {
                        [5] = ["B"],
                        [6] = ["B"],
                        [7] = ["A"],
                        [8] = ["A"]
                    })
            ],
            SystemSlots: PlannerV2.GenericSystemSlots));

        Assert.Empty(result.Warnings);
        Assert.Collection(
            result.Steps.Where(s => s.SystemSlotId == "generic_cannons"),
            step =>
            {
                Assert.Equal(6, step.FromLevel);
                Assert.Equal(7, step.ToLevel);
                Assert.Equal("A", step.BranchCode);
            },
            step =>
            {
                Assert.Equal(7, step.FromLevel);
                Assert.Equal(8, step.ToLevel);
                Assert.Equal("A", step.BranchCode);
            });
    }

    [Fact]
    public void Plan_OwnedAnyLevelFiveBranch_AllowsPlanningEitherLevelSixBranch()
    {
        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2(PlannerV2.GenericAircraftId, true, 6)],
            OwnedSystemNodes: [
                new OwnedSystemNodeV2("generic_rockets", 1, null),
                new OwnedSystemNodeV2("generic_rockets", 2, null),
                new OwnedSystemNodeV2("generic_rockets", 3, null),
                new OwnedSystemNodeV2("generic_rockets", 4, null),
                new OwnedSystemNodeV2("generic_rockets", 5, "B")
            ],
            EquippedSystemBranches: [new EquippedSystemBranchV2("generic_rockets", 5, "B")],
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2(PlannerV2.GenericAircraftId, 6)],
            SystemTargets: [
                new SystemTargetV2(
                    "generic_rockets",
                    6,
                    BranchOwnershipMode.ChosenOnly,
                    new Dictionary<int, string> { [5] = "B", [6] = "A" },
                    new Dictionary<int, IReadOnlyList<string>>
                    {
                        [5] = ["B"],
                        [6] = ["A"]
                    })
            ],
            SystemSlots: PlannerV2.GenericSystemSlots));

        Assert.Empty(result.Warnings);
        var step = Assert.Single(result.Steps.Where(s => s.SystemSlotId == "generic_rockets"));
        Assert.Equal(5, step.FromLevel);
        Assert.Equal(6, step.ToLevel);
        Assert.Equal("A", step.BranchCode);
    }

    [Fact]
    public void Plan_PlannedAnyLevelFiveBranch_AllowsPlanningEitherLevelSixBranch()
    {
        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2(PlannerV2.GenericAircraftId, true, 6)],
            OwnedSystemNodes: [
                new OwnedSystemNodeV2("generic_rockets", 1, null),
                new OwnedSystemNodeV2("generic_rockets", 2, null),
                new OwnedSystemNodeV2("generic_rockets", 3, null),
                new OwnedSystemNodeV2("generic_rockets", 4, null)
            ],
            EquippedSystemBranches: [],
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2(PlannerV2.GenericAircraftId, 6)],
            SystemTargets: [
                new SystemTargetV2(
                    "generic_rockets",
                    6,
                    BranchOwnershipMode.ChosenOnly,
                    new Dictionary<int, string> { [5] = "B", [6] = "A" },
                    new Dictionary<int, IReadOnlyList<string>>
                    {
                        [5] = ["B"],
                        [6] = ["A"]
                    })
            ],
            SystemSlots: PlannerV2.GenericSystemSlots));

        Assert.Empty(result.Warnings);
        Assert.Collection(
            result.Steps.Where(s => s.SystemSlotId == "generic_rockets"),
            step =>
            {
                Assert.Equal(4, step.FromLevel);
                Assert.Equal(5, step.ToLevel);
                Assert.Equal("B", step.BranchCode);
            },
            step =>
            {
                Assert.Equal(5, step.FromLevel);
                Assert.Equal(6, step.ToLevel);
                Assert.Equal("A", step.BranchCode);
            });
    }

    [Theory]
    [InlineData(5, "A", 6, "B")]
    [InlineData(5, "B", 6, "A")]
    [InlineData(6, "A", 7, "B")]
    [InlineData(6, "B", 7, "A")]
    [InlineData(7, "A", 8, "B")]
    [InlineData(7, "B", 8, "A")]
    public void Plan_OwnedAnyPriorBranchLevel_AllowsPlanningEitherBranchAtNextLevel(
        int ownedLevel,
        string ownedBranch,
        int targetLevel,
        string targetBranch)
    {
        var ownedNodes = new List<OwnedSystemNodeV2>
        {
            new("generic_main_radar_missile", 1, null),
            new("generic_main_radar_missile", 2, null),
            new("generic_main_radar_missile", 3, null),
            new("generic_main_radar_missile", 4, null)
        };

        for (var level = 5; level <= ownedLevel; level++)
        {
            ownedNodes.Add(new OwnedSystemNodeV2("generic_main_radar_missile", level, ownedBranch));
        }

        var targetBranches = Enumerable.Range(5, targetLevel - 4)
            .ToDictionary(
                level => level,
                level => level == targetLevel
                    ? targetBranch
                    : ownedBranch);

        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2(PlannerV2.GenericAircraftId, true, 6)],
            OwnedSystemNodes: ownedNodes,
            EquippedSystemBranches: ownedNodes
                .Where(node => node.SystemLevel >= 5)
                .Select(node => new EquippedSystemBranchV2(node.SystemSlotId, node.SystemLevel, node.BranchCode!))
                .ToArray(),
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2(PlannerV2.GenericAircraftId, 6)],
            SystemTargets: [
                new SystemTargetV2(
                    "generic_main_radar_missile",
                    targetLevel,
                    BranchOwnershipMode.ChosenOnly,
                    targetBranches,
                    targetBranches.ToDictionary(
                        target => target.Key,
                        target => (IReadOnlyList<string>)[target.Value]))
            ],
            SystemSlots: PlannerV2.GenericSystemSlots));

        Assert.Empty(result.Warnings);
        var step = Assert.Single(result.Steps.Where(s => s.SystemSlotId == "generic_main_radar_missile"));
        Assert.Equal(ownedLevel, step.FromLevel);
        Assert.Equal(targetLevel, step.ToLevel);
        Assert.Equal(targetBranch, step.BranchCode);
    }

    [Theory]
    [InlineData(5, "A", 6, "B")]
    [InlineData(5, "B", 6, "A")]
    [InlineData(6, "A", 7, "B")]
    [InlineData(6, "B", 7, "A")]
    [InlineData(7, "A", 8, "B")]
    [InlineData(7, "B", 8, "A")]
    public void Plan_PlannedAnyPriorBranchLevel_AllowsPlanningEitherBranchAtNextLevel(
        int plannedPriorLevel,
        string plannedPriorBranch,
        int targetLevel,
        string targetBranch)
    {
        var targetBranches = Enumerable.Range(5, targetLevel - 4)
            .ToDictionary(
                level => level,
                level => level == targetLevel
                    ? targetBranch
                    : plannedPriorBranch);

        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2(PlannerV2.GenericAircraftId, true, 6)],
            OwnedSystemNodes: [
                new OwnedSystemNodeV2("generic_secondary_ir_missile", 1, null),
                new OwnedSystemNodeV2("generic_secondary_ir_missile", 2, null),
                new OwnedSystemNodeV2("generic_secondary_ir_missile", 3, null),
                new OwnedSystemNodeV2("generic_secondary_ir_missile", 4, null)
            ],
            EquippedSystemBranches: [],
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2(PlannerV2.GenericAircraftId, 6)],
            SystemTargets: [
                new SystemTargetV2(
                    "generic_secondary_ir_missile",
                    targetLevel,
                    BranchOwnershipMode.ChosenOnly,
                    targetBranches,
                    targetBranches.ToDictionary(
                        target => target.Key,
                        target => (IReadOnlyList<string>)[target.Value]))
            ],
            SystemSlots: PlannerV2.GenericSystemSlots));

        Assert.Empty(result.Warnings);
        var steps = result.Steps.Where(s => s.SystemSlotId == "generic_secondary_ir_missile").ToArray();
        Assert.Contains(steps, step => step.FromLevel == plannedPriorLevel && step.ToLevel == targetLevel && step.BranchCode == targetBranch);
    }

    [Fact]
    public void Plan_DuplicateSystemSlotIds_ReturnsWarningAndNoPlan()
    {
        var duplicatedSlots = PlannerV2.DefaultSystemSlots.Concat([PlannerV2.DefaultSystemSlots[0]]).ToArray();

        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [new AircraftStateV2("f106_delta_dart", true, 6)],
            OwnedSystemNodes: [],
            EquippedSystemBranches: [],
            ResourceBalances: [],
            AircraftTargets: [new AircraftTargetV2("f106_delta_dart", 6)],
            SystemTargets: [new SystemTargetV2("f106_engines_main", 1, BranchOwnershipMode.ChosenOnly)],
            SystemSlots: duplicatedSlots));

        Assert.Contains(result.Warnings, w => w.Contains("duplicate system-slot", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(result.Steps);
    }

    [Fact]
    public void Plan_TwoAircraftFleet_CombinesSharedCurrencyTotalsAndOrdersByAircraft()
    {
        var result = PlannerV2.Plan(new PlannerRequestV2(
            Aircraft: [
                new AircraftStateV2("f106_delta_dart", true, 5),
                new AircraftStateV2("f5_tiger_ii", true, 8)
            ],
            OwnedSystemNodes: [],
            EquippedSystemBranches: [],
            ResourceBalances: [
                new ResourceBalanceV2(PlannerV2.Currencies.Silver, 2500),
                new ResourceBalanceV2(PlannerV2.Currencies.AircraftParts, 200),
                new ResourceBalanceV2(PlannerV2.Currencies.EngineParts, 600)
            ],
            AircraftTargets: [
                new AircraftTargetV2("f106_delta_dart", 6),
                new AircraftTargetV2("f5_tiger_ii", 9)
            ],
            SystemTargets: [
                new SystemTargetV2("f106_engines_main", 1, BranchOwnershipMode.ChosenOnly),
                new SystemTargetV2("f5_engines_main", 2, BranchOwnershipMode.ChosenOnly)
            ],
            SystemSlots: PlannerV2.DefaultSystemSlots));

        Assert.Empty(result.Warnings);
        Assert.Equal(5, result.Steps.Count);
        Assert.Collection(result.Steps,
            step => Assert.Equal("f106_delta_dart", step.AircraftId),
            step => Assert.Equal("f106_delta_dart", step.AircraftId),
            step => Assert.Equal("f5_tiger_ii", step.AircraftId),
            step => Assert.Equal("f5_tiger_ii", step.AircraftId),
            step => Assert.Equal("f5_tiger_ii", step.AircraftId));
        Assert.Equal(4400, result.TotalsRequired.Single(c => c.CurrencyCode == PlannerV2.Currencies.Silver).Amount);
        Assert.Equal(675, result.TotalsRequired.Single(c => c.CurrencyCode == PlannerV2.Currencies.AircraftParts).Amount);
        Assert.Equal(700, result.TotalsRequired.Single(c => c.CurrencyCode == PlannerV2.Currencies.EngineParts).Amount);
        Assert.Equal(1900, result.Deficits.Single(c => c.CurrencyCode == PlannerV2.Currencies.Silver).Amount);
        Assert.Equal(475, result.Deficits.Single(c => c.CurrencyCode == PlannerV2.Currencies.AircraftParts).Amount);
        Assert.Equal(100, result.Deficits.Single(c => c.CurrencyCode == PlannerV2.Currencies.EngineParts).Amount);
    }
}

public class Upgrades2CatalogFiles
{
    [Fact]
    public void RequiredStaticCatalogFiles_ExistWithSeedRows()
    {
        var root = FindRepoRoot();
        var dataRoot = Path.Combine(root, "MetalStorming20.Web", "wwwroot", "data", "v2");
        var required = new[]
        {
            "schema-version.json",
            "currencies.json",
            "aircraft.json",
            "aircraft-milestones.json",
            "system-types.json",
            "aircraft-system-slots.json",
            "branch-families.json",
            "system-node-definitions.json",
            "upgrade-costs.json"
        };

        foreach (var file in required)
        {
            Assert.True(File.Exists(Path.Combine(dataRoot, file)), $"{file} should exist.");
        }

        using var costs = JsonDocument.Parse(File.ReadAllText(Path.Combine(dataRoot, "upgrade-costs.json")));
        var rows = costs.RootElement.EnumerateArray().ToList();
        Assert.Equal(35, rows.Count);
        Assert.Equal(19, rows.Count(r => r.GetProperty("upgradeKind").GetString() == "AIRCRAFT"));
        Assert.Equal(8, rows.Count(r => r.GetProperty("upgradeKind").GetString() == "SYSTEM"));
        Assert.Equal(3, rows.Count(r => r.GetProperty("upgradeKind").GetString() == "SPECIAL"));
        Assert.Equal(5, rows.Count(r => r.GetProperty("upgradeKind").GetString() == "PASSIVE"));

        using var slots = JsonDocument.Parse(File.ReadAllText(Path.Combine(dataRoot, "aircraft-system-slots.json")));
        var slotRows = slots.RootElement.EnumerateArray().ToList();
        var special = Assert.Single(slotRows, r => r.GetProperty("systemSlotId").GetString() == "generic_special");
        var passive = Assert.Single(slotRows, r => r.GetProperty("systemSlotId").GetString() == "generic_passive");
        Assert.Equal(8, special.GetProperty("unlockAircraftLevel").GetInt32());
        Assert.Equal(3, special.GetProperty("maxSystemLevel").GetInt32());
        Assert.Equal(12, passive.GetProperty("unlockAircraftLevel").GetInt32());
        Assert.Equal(5, passive.GetProperty("maxSystemLevel").GetInt32());
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "MetalStorming20.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }
}
