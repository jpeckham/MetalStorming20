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
        Assert.Equal(4900, result.MasteryRebate.Single(c => c.CurrencyCode == PlannerV2.Currencies.AircraftParts).Amount);
        Assert.Equal(16900, result.MasteryRebate.Single(c => c.CurrencyCode == PlannerV2.Currencies.Silver).Amount);
        Assert.Equal(6300, result.NetGrindNeeded.Single(c => c.CurrencyCode == PlannerV2.Currencies.AircraftParts).Amount);
        Assert.Equal(33100, result.NetGrindNeeded.Single(c => c.CurrencyCode == PlannerV2.Currencies.Silver).Amount);
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
        Assert.Equal(27, rows.Count);
        Assert.Equal(19, rows.Count(r => r.GetProperty("upgradeKind").GetString() == "AIRCRAFT"));
        Assert.Equal(8, rows.Count(r => r.GetProperty("upgradeKind").GetString() == "SYSTEM"));
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
