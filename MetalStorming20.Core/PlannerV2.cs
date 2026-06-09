namespace MetalStorming20.Core;

public enum BranchOwnershipMode
{
    ChosenOnly,
    Both
}

public enum PlanStepScope
{
    Aircraft,
    System,
    Mastery
}

public enum GoldMasteryStatus
{
    Off,
    Owned,
    Planned
}

public sealed record CurrencyAmountV2(string CurrencyCode, int Amount);

public sealed record MasteryPlanV2(
    int CurrentMasteryLevel,
    int PlannedMasteryLevel,
    GoldMasteryStatus GoldStatus);

public sealed record UpgradeCostRowV2(
    string UpgradeKind,
    int FromLevel,
    int ToLevel,
    int SilverCost,
    int AircraftPartsCost,
    int SystemPartsCost,
    int AdvancedPartsCost,
    string SourceProvenance,
    string ConfidenceFlag);

public sealed record AircraftStateV2(string AircraftId, bool IsOwned, int CurrentAircraftLevel);

public sealed record OwnedSystemNodeV2(string SystemSlotId, int SystemLevel, string? BranchCode);

public sealed record EquippedSystemBranchV2(string SystemSlotId, int SystemLevel, string BranchCode);

public sealed record ResourceBalanceV2(string CurrencyCode, int Amount);

public sealed record AircraftTargetV2(string AircraftId, int TargetAircraftLevel);

public sealed record SystemTargetV2(
    string SystemSlotId,
    int TargetSystemLevel,
    BranchOwnershipMode OwnershipMode,
    IReadOnlyDictionary<int, string>? TargetEquippedBranches = null,
    IReadOnlyDictionary<int, IReadOnlyList<string>>? TargetBranchesToOwn = null);

public sealed record SystemSlotDefinitionV2(
    string SystemSlotId,
    string AircraftId,
    string SystemTypeId,
    string PartCurrencyCode,
    string DisplayName,
    int UnlockAircraftLevel = 6,
    int MaxSystemLevel = 8,
    bool UsesBranches = true,
    string UpgradeKind = "SYSTEM");

public sealed record PlannerRequestV2(
    IReadOnlyList<AircraftStateV2> Aircraft,
    IReadOnlyList<OwnedSystemNodeV2> OwnedSystemNodes,
    IReadOnlyList<EquippedSystemBranchV2> EquippedSystemBranches,
    IReadOnlyList<ResourceBalanceV2> ResourceBalances,
    IReadOnlyList<AircraftTargetV2> AircraftTargets,
    IReadOnlyList<SystemTargetV2> SystemTargets,
    IReadOnlyList<SystemSlotDefinitionV2> SystemSlots,
    MasteryPlanV2? MasteryPlan = null);

public sealed record PlanStepV2(
    int Order,
    string AircraftId,
    PlanStepScope Scope,
    int FromLevel,
    int ToLevel,
    IReadOnlyList<CurrencyAmountV2> Costs,
    string? SystemSlotId = null,
    string? BranchCode = null);

public sealed record PlannerResultV2(
    IReadOnlyList<string> Warnings,
    IReadOnlyList<CurrencyAmountV2> TotalsRequired,
    IReadOnlyList<CurrencyAmountV2> Deficits,
    IReadOnlyList<PlanStepV2> Steps,
    IReadOnlyList<CurrencyAmountV2> MasteryRebate,
    IReadOnlyList<CurrencyAmountV2> MasteryNormalRebate,
    IReadOnlyList<CurrencyAmountV2> MasteryGoldRebate,
    IReadOnlyList<CurrencyAmountV2> NetGrindNeeded);

public static class PlannerV2
{
    public const string GenericAircraftId = "generic_aircraft";

    public static class Currencies
    {
        public const string Gold = "GOLD";
        public const string Silver = "SILVER";
        public const string AircraftParts = "AIRCRAFT_PARTS";
        public const string AdvancedParts = "ADVANCED_PARTS";
        public const string FuselageParts = "FUSELAGE_PARTS";
        public const string EngineParts = "ENGINE_PARTS";
        public const string AvionicsParts = "AVIONICS_PARTS";
        public const string CannonParts = "CANNON_PARTS";
        public const string MissileParts = "MISSILE_PARTS";
        public const string RocketParts = "ROCKET_PARTS";
        public const string SpecialAbilityBlueprints = "SPECIAL_ABILITY_BLUEPRINT";
        public const string PassiveAbilityBlueprints = "PASSIVE_ABILITY_BLUEPRINT";
    }

    public static IReadOnlyList<SystemSlotDefinitionV2> DefaultSystemSlots { get; } =
    [
        new("f106_fuselage_main", "f106_delta_dart", "FUSELAGE", Currencies.FuselageParts, "Fuselage"),
        new("f106_engines_main", "f106_delta_dart", "ENGINES", Currencies.EngineParts, "Engines"),
        new("f106_avionics_main", "f106_delta_dart", "AVIONICS", Currencies.AvionicsParts, "Avionics"),
        new("f106_cannons_main", "f106_delta_dart", "CANNONS", Currencies.CannonParts, "Cannons"),
        new("f106_missile_left", "f106_delta_dart", "MISSILE", Currencies.MissileParts, "Missile Left"),
        new("f106_missile_right", "f106_delta_dart", "MISSILE", Currencies.MissileParts, "Missile Right"),
        new("f5_fuselage_main", "f5_tiger_ii", "FUSELAGE", Currencies.FuselageParts, "Fuselage"),
        new("f5_engines_main", "f5_tiger_ii", "ENGINES", Currencies.EngineParts, "Engines"),
        new("f5_avionics_main", "f5_tiger_ii", "AVIONICS", Currencies.AvionicsParts, "Avionics"),
        new("f5_cannons_main", "f5_tiger_ii", "CANNONS", Currencies.CannonParts, "Cannons"),
        new("f5_missile_main", "f5_tiger_ii", "MISSILE", Currencies.MissileParts, "Missile")
    ];

    public static IReadOnlyList<SystemSlotDefinitionV2> GenericSystemSlots { get; } =
    [
        new("generic_fuselage", GenericAircraftId, "FUSELAGE", Currencies.FuselageParts, "Fuselage"),
        new("generic_engines", GenericAircraftId, "ENGINES", Currencies.EngineParts, "Engines"),
        new("generic_avionics", GenericAircraftId, "AVIONICS", Currencies.AvionicsParts, "Avionics"),
        new("generic_cannons", GenericAircraftId, "CANNONS", Currencies.CannonParts, "Cannons"),
        new("generic_main_radar_missile", GenericAircraftId, "MISSILE", Currencies.MissileParts, "Main/Radar Missile"),
        new("generic_secondary_ir_missile", GenericAircraftId, "MISSILE", Currencies.MissileParts, "Secondary/IR Missile"),
        new("generic_rockets", GenericAircraftId, "ROCKETS", Currencies.RocketParts, "Rockets"),
        new("generic_special", GenericAircraftId, "SPECIAL", Currencies.SpecialAbilityBlueprints, "Special", 8, 3, false, "SPECIAL"),
        new("generic_passive", GenericAircraftId, "PASSIVE", Currencies.PassiveAbilityBlueprints, "Passive", 12, 5, false, "PASSIVE")
    ];

    private static readonly UpgradeCostRowV2[] AircraftCosts =
    [
        AircraftRow(1, 2, 75, 25, "MEDIUM_HIGH"),
        AircraftRow(2, 3, 150, 50, "MEDIUM_HIGH"),
        AircraftRow(3, 4, 300, 75, "MEDIUM_HIGH"),
        AircraftRow(4, 5, 600, 150, "HIGH"),
        AircraftRow(5, 6, 1000, 225, "HIGH"),
        AircraftRow(6, 7, 1300, 300, "MEDIUM_HIGH"),
        AircraftRow(7, 8, 1625, 375, "MEDIUM_HIGH"),
        AircraftRow(8, 9, 2000, 450, "HIGH"),
        AircraftRow(9, 10, 2250, 525, "HIGH"),
        AircraftRow(10, 11, 2600, 600, "HIGH"),
        AircraftRow(11, 12, 2950, 650, "MEDIUM_HIGH"),
        AircraftRow(12, 13, 3250, 725, "MEDIUM_HIGH"),
        AircraftRow(13, 14, 3600, 800, "HIGH"),
        AircraftRow(14, 15, 3900, 875, "MEDIUM_HIGH"),
        AircraftRow(15, 16, 4250, 925, "MEDIUM_HIGH"),
        AircraftRow(16, 17, 4550, 1000, "MEDIUM_HIGH"),
        AircraftRow(17, 18, 4900, 1075, "MEDIUM_HIGH"),
        AircraftRow(18, 19, 5200, 1150, "MEDIUM_HIGH"),
        AircraftRow(19, 20, 5500, 1225, "MEDIUM_HIGH")
    ];

    private static readonly UpgradeCostRowV2[] SystemCosts =
    [
        SystemRow(0, 1, 400, 200, 0, "HIGH"),
        SystemRow(1, 2, 600, 300, 0, "MEDIUM_HIGH"),
        SystemRow(2, 3, 900, 450, 0, "HIGH"),
        SystemRow(3, 4, 1300, 600, 0, "HIGH"),
        SystemRow(4, 5, 2100, 850, 1, "HIGH"),
        SystemRow(5, 6, 3300, 1250, 1, "MEDIUM_HIGH"),
        SystemRow(6, 7, 5000, 2000, 1, "MEDIUM_HIGH"),
        SystemRow(7, 8, 7000, 3000, 1, "MEDIUM_HIGH")
    ];

    private static readonly UpgradeCostRowV2[] SpecialCosts =
    [
        AbilityRow("SPECIAL", 0, 1, 5000, 1),
        AbilityRow("SPECIAL", 1, 2, 10000, 2),
        AbilityRow("SPECIAL", 2, 3, 20000, 5)
    ];

    private static readonly UpgradeCostRowV2[] PassiveCosts =
    [
        AbilityRow("PASSIVE", 0, 1, 3000, 1),
        AbilityRow("PASSIVE", 1, 2, 6000, 2),
        AbilityRow("PASSIVE", 2, 3, 12000, 3),
        AbilityRow("PASSIVE", 3, 4, 21000, 5),
        AbilityRow("PASSIVE", 4, 5, 33000, 8)
    ];

    private static readonly Dictionary<int, (int aircraftParts, int silver)> MasteryNonGoldRewards = new()
    {
        { 1,  (100, 0) },
        { 3,  (150, 0) },
        { 5,  (0, 700) },
        { 7,  (0, 900) },
        { 11, (250, 0) },
        { 13, (300, 0) },
        { 15, (350, 0) },
        { 17, (0, 1200) },
        { 19, (0, 1600) },
        { 21, (400, 0) },
        { 23, (450, 0) }
    };

    private static readonly Dictionary<int, (int aircraftParts, int silver)> MasteryGoldRewards = new()
    {
        { 2,  (0, 1500) },
        { 6,  (1000, 0) },
        { 10, (0, 2500) },
        { 14, (0, 3500) },
        { 18, (2000, 0) },
        { 22, (0, 5000) }
    };

    private const int GoldMasteryPurchaseCost = 269;

    public static IReadOnlyList<UpgradeCostRowV2> AllUpgradeCosts =>
        AircraftCosts.Concat(SystemCosts).Concat(SpecialCosts).Concat(PassiveCosts).ToArray();

    public static UpgradeCostRowV2 GetAircraftUpgradeCost(int fromLevel, int toLevel) =>
        AircraftCosts.Single(c => c.FromLevel == fromLevel && c.ToLevel == toLevel);

    public static UpgradeCostRowV2 GetSystemUpgradeCost(int fromLevel, int toLevel) =>
        SystemCosts.Single(c => c.FromLevel == fromLevel && c.ToLevel == toLevel);

    public static UpgradeCostRowV2 GetAbilityUpgradeCost(string upgradeKind, int fromLevel, int toLevel) =>
        AbilityCosts(upgradeKind).Single(c => c.FromLevel == fromLevel && c.ToLevel == toLevel);

    public static IReadOnlyDictionary<string, int> SumAircraftCosts(int fromLevel, int toLevel)
    {
        var totals = new Dictionary<string, int>();
        foreach (var cost in AircraftCosts.Where(c => c.FromLevel >= fromLevel && c.ToLevel <= toLevel))
        {
            Add(totals, Currencies.Silver, cost.SilverCost);
            Add(totals, Currencies.AircraftParts, cost.AircraftPartsCost);
        }

        return totals;
    }

    public static IReadOnlyDictionary<string, int> SumSystemCosts(
        int fromLevel,
        int toLevel,
        BranchOwnershipMode mode,
        string systemPartCurrencyCode)
    {
        var totals = new Dictionary<string, int>();
        foreach (var cost in SystemCosts.Where(c => c.FromLevel >= fromLevel && c.ToLevel <= toLevel))
        {
            var multiplier = mode == BranchOwnershipMode.Both && cost.ToLevel >= 5 ? 2 : 1;
            Add(totals, Currencies.Silver, cost.SilverCost * multiplier);
            Add(totals, systemPartCurrencyCode, cost.SystemPartsCost * multiplier);
            Add(totals, Currencies.AdvancedParts, cost.AdvancedPartsCost * multiplier);
        }

        return totals;
    }

    public static IReadOnlyDictionary<string, int> SumAbilityCosts(
        string upgradeKind,
        int fromLevel,
        int toLevel,
        string blueprintCurrencyCode)
    {
        var totals = new Dictionary<string, int>();
        foreach (var cost in AbilityCosts(upgradeKind).Where(c => c.FromLevel >= fromLevel && c.ToLevel <= toLevel))
        {
            Add(totals, Currencies.Silver, cost.SilverCost);
            Add(totals, blueprintCurrencyCode, cost.SystemPartsCost);
        }

        return totals;
    }

    public static PlannerResultV2 Plan(PlannerRequestV2 request)
    {
        var warnings = Validate(request);
        if (warnings.Count > 0)
        {
            return new PlannerResultV2(warnings, [], [], [], [], [], [], []);
        }

        var steps = new List<PlanStepV2>();
        var aircraftById = request.Aircraft.ToDictionary(a => a.AircraftId, StringComparer.OrdinalIgnoreCase);
        var aircraftTargets = request.AircraftTargets.ToDictionary(t => t.AircraftId, StringComparer.OrdinalIgnoreCase);
        var slotsById = request.SystemSlots.ToDictionary(s => s.SystemSlotId, StringComparer.OrdinalIgnoreCase);
        var requiredAircraftTargets = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var target in aircraftTargets.Values)
        {
            requiredAircraftTargets[target.AircraftId] = target.TargetAircraftLevel;
        }

        foreach (var target in request.SystemTargets)
        {
            var slot = slotsById[target.SystemSlotId];
            if (!requiredAircraftTargets.TryGetValue(slot.AircraftId, out var level) || level < slot.UnlockAircraftLevel)
            {
                requiredAircraftTargets[slot.AircraftId] = slot.UnlockAircraftLevel;
            }
        }

        foreach (var target in requiredAircraftTargets.OrderBy(t => t.Key, StringComparer.OrdinalIgnoreCase))
        {
            var currentLevel = aircraftById.TryGetValue(target.Key, out var state) ? state.CurrentAircraftLevel : 1;
            for (var level = currentLevel; level < target.Value; level++)
            {
                var cost = GetAircraftUpgradeCost(level, level + 1);
                steps.Add(new PlanStepV2(
                    steps.Count + 1,
                    target.Key,
                    PlanStepScope.Aircraft,
                    level,
                    level + 1,
                    [
                        new(Currencies.Silver, cost.SilverCost),
                        new(Currencies.AircraftParts, cost.AircraftPartsCost)
                    ]));
            }

            foreach (var systemTarget in request.SystemTargets
                .Where(t => slotsById[t.SystemSlotId].AircraftId.Equals(target.Key, StringComparison.OrdinalIgnoreCase))
                .OrderBy(t => t.SystemSlotId, StringComparer.OrdinalIgnoreCase))
            {
                var slot = slotsById[systemTarget.SystemSlotId];
                for (var level = 0; level < systemTarget.TargetSystemLevel; level++)
                {
                    var toLevel = level + 1;
                    var branchCodes = BranchCodesToPurchase(systemTarget, slot, toLevel);
                    foreach (var branchCode in branchCodes)
                    {
                        if (IsOwned(request.OwnedSystemNodes, systemTarget.SystemSlotId, toLevel, branchCode))
                        {
                            continue;
                        }

                        var cost = GetSlotUpgradeCost(slot, level, toLevel);
                        steps.Add(new PlanStepV2(
                            steps.Count + 1,
                            slot.AircraftId,
                            PlanStepScope.System,
                            level,
                            toLevel,
                            [
                                new(Currencies.Silver, cost.SilverCost),
                                new(slot.PartCurrencyCode, cost.SystemPartsCost),
                                new(Currencies.AdvancedParts, cost.AdvancedPartsCost)
                            ],
                            systemTarget.SystemSlotId,
                            branchCode));
                    }
                }
            }
        }

        AddMasteryPurchaseSteps(steps, request.MasteryPlan);
        var totals = TotalsFor(steps);
        var deficits = DeficitsFor(totals, request.ResourceBalances);
        var masteryNormalRebate = MasteryNormalRebateFor(request.MasteryPlan);
        var masteryGoldRebate = MasteryGoldRebateFor(request.MasteryPlan);
        var masteryRebate = CombineCurrencyAmounts(masteryNormalRebate, masteryGoldRebate);
        var netGrindNeeded = NetGrindNeededFor(deficits, masteryRebate);
        return new PlannerResultV2([], totals, deficits, steps, masteryRebate, masteryNormalRebate, masteryGoldRebate, netGrindNeeded);
    }

    private static List<string> Validate(PlannerRequestV2 request)
    {
        var warnings = new List<string>();
        var duplicateSlots = request.SystemSlots
            .GroupBy(s => s.SystemSlotId, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();
        foreach (var duplicate in duplicateSlots)
        {
            warnings.Add($"Duplicate system-slot ID found in catalog: {duplicate}.");
        }

        var slotsById = request.SystemSlots
            .GroupBy(s => s.SystemSlotId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        var aircraftById = request.Aircraft.ToDictionary(a => a.AircraftId, StringComparer.OrdinalIgnoreCase);

        foreach (var balance in request.ResourceBalances)
        {
            if (balance.Amount < 0)
            {
                warnings.Add($"{balance.CurrencyCode} balance cannot be negative.");
            }
        }

        foreach (var target in request.AircraftTargets)
        {
            var current = request.Aircraft.FirstOrDefault(a => a.AircraftId.Equals(target.AircraftId, StringComparison.OrdinalIgnoreCase));
            if (current is not null && target.TargetAircraftLevel < current.CurrentAircraftLevel)
            {
                warnings.Add($"{target.AircraftId} target aircraft level cannot be below current level.");
            }
        }

        foreach (var target in request.SystemTargets)
        {
            if (!slotsById.TryGetValue(target.SystemSlotId, out var slot))
            {
                warnings.Add($"{target.SystemSlotId} is not a known system slot.");
                continue;
            }

            if (!aircraftById.TryGetValue(slot.AircraftId, out var aircraft) || !aircraft.IsOwned)
            {
                warnings.Add($"{target.SystemSlotId} targets an unowned aircraft.");
            }

            if (target.TargetSystemLevel < 0 || target.TargetSystemLevel > slot.MaxSystemLevel)
            {
                warnings.Add($"{target.SystemSlotId} target system level must be between 0 and {slot.MaxSystemLevel}.");
            }

            var currentSystemLevel = CurrentSystemLevel(request.OwnedSystemNodes, target.SystemSlotId);
            if (target.TargetSystemLevel < currentSystemLevel)
            {
                warnings.Add($"{target.SystemSlotId} target system level cannot be below current owned level.");
            }

            if (slot.UsesBranches && target.OwnershipMode == BranchOwnershipMode.ChosenOnly && target.TargetSystemLevel >= 5)
            {
                for (var level = 5; level <= target.TargetSystemLevel; level++)
                {
                    if (target.TargetEquippedBranches is null ||
                        !target.TargetEquippedBranches.TryGetValue(level, out var branchCode) ||
                        !IsBranchCode(branchCode))
                    {
                        warnings.Add($"{target.SystemSlotId} requires a branch choice for system level {level}.");
                    }
                }
            }

            if (target.TargetEquippedBranches is not null)
            {
                foreach (var equipped in target.TargetEquippedBranches)
                {
                    if (!slot.UsesBranches)
                    {
                        warnings.Add($"{target.SystemSlotId} does not support branch choices.");
                    }
                    else if (equipped.Key < 5)
                    {
                        warnings.Add($"{target.SystemSlotId} cannot equip a branch below system level 5.");
                    }
                    else if (!IsBranchCode(equipped.Value))
                    {
                        warnings.Add($"{target.SystemSlotId} branch choice must be A or B.");
                    }
                    else if (equipped.Key <= currentSystemLevel &&
                        !IsOwned(request.OwnedSystemNodes, target.SystemSlotId, equipped.Key, equipped.Value) &&
                        !IsPlannedTargetBranch(target, equipped.Key, equipped.Value))
                    {
                        warnings.Add($"{target.SystemSlotId} branch {equipped.Value} at level {equipped.Key} is not owned.");
                    }
                }
            }
        }

        foreach (var equipped in request.EquippedSystemBranches)
        {
            if (slotsById.TryGetValue(equipped.SystemSlotId, out var slot) && !slot.UsesBranches)
            {
                warnings.Add($"{equipped.SystemSlotId} does not support branch choices.");
            }
            else if (equipped.SystemLevel < 5)
            {
                warnings.Add($"{equipped.SystemSlotId} cannot equip a branch below system level 5.");
            }
            else if (!IsBranchCode(equipped.BranchCode))
            {
                warnings.Add($"{equipped.SystemSlotId} equipped branch must be A or B.");
            }
            else if (!IsOwned(request.OwnedSystemNodes, equipped.SystemSlotId, equipped.SystemLevel, equipped.BranchCode))
            {
                warnings.Add($"{equipped.SystemSlotId} equipped branch {equipped.BranchCode} at level {equipped.SystemLevel} is not owned.");
            }
        }

        foreach (var slotGroup in request.OwnedSystemNodes.GroupBy(n => n.SystemSlotId, StringComparer.OrdinalIgnoreCase))
        {
            if (!slotsById.TryGetValue(slotGroup.Key, out var slot) || !slot.UsesBranches)
            {
                continue;
            }

            var nodes = slotGroup.ToArray();
            for (var trunkLevel = 1; trunkLevel <= Math.Min(4, nodes.Select(n => n.SystemLevel).DefaultIfEmpty(0).Max()); trunkLevel++)
            {
                if (!nodes.Any(n => n.SystemLevel == trunkLevel && n.BranchCode is null))
                {
                    warnings.Add($"{slotGroup.Key} has an inconsistent ownership graph: missing trunk level {trunkLevel}.");
                }
            }

            foreach (var branchNode in nodes.Where(n => n.SystemLevel >= 5))
            {
                if (!IsBranchCode(branchNode.BranchCode ?? ""))
                {
                    warnings.Add($"{slotGroup.Key} has an inconsistent ownership graph: branch levels require A or B.");
                    continue;
                }

                if (!Enumerable.Range(1, 4).All(level => nodes.Any(n => n.SystemLevel == level && n.BranchCode is null)))
                {
                    warnings.Add($"{slotGroup.Key} has an inconsistent ownership graph: branch ownership requires trunk levels 1-4.");
                }

                for (var level = 5; level < branchNode.SystemLevel; level++)
                {
                    if (!nodes.Any(n => n.SystemLevel == level && IsBranchCode(n.BranchCode ?? "")))
                    {
                        warnings.Add($"{slotGroup.Key} has an inconsistent ownership graph: branch {branchNode.BranchCode} level {branchNode.SystemLevel} requires level {level}.");
                    }
                }
            }
        }

        return warnings.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string?[] BranchCodesToPurchase(SystemTargetV2 target, SystemSlotDefinitionV2 slot, int toLevel)
    {
        if (!slot.UsesBranches || toLevel < 5)
        {
            return [null];
        }

        if (target.TargetBranchesToOwn is not null &&
            target.TargetBranchesToOwn.TryGetValue(toLevel, out var branchCodes) &&
            branchCodes.Count > 0)
        {
            return branchCodes
                .Where(IsBranchCode)
                .Select(branchCode => branchCode.ToUpperInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        if (target.OwnershipMode == BranchOwnershipMode.Both)
        {
            return ["A", "B"];
        }

        return [target.TargetEquippedBranches![toLevel].ToUpperInvariant()];
    }

    private static int CurrentSystemLevel(IReadOnlyList<OwnedSystemNodeV2> ownedNodes, string systemSlotId)
    {
        return ownedNodes
            .Where(n => n.SystemSlotId.Equals(systemSlotId, StringComparison.OrdinalIgnoreCase))
            .Select(n => n.SystemLevel)
            .DefaultIfEmpty(0)
            .Max();
    }

    private static bool IsOwned(IReadOnlyList<OwnedSystemNodeV2> ownedNodes, string systemSlotId, int level, string? branchCode)
    {
        return ownedNodes.Any(n =>
            n.SystemSlotId.Equals(systemSlotId, StringComparison.OrdinalIgnoreCase) &&
            n.SystemLevel == level &&
            string.Equals(n.BranchCode, branchCode, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsPlannedTargetBranch(SystemTargetV2 target, int level, string branchCode)
    {
        return target.TargetBranchesToOwn is not null &&
            target.TargetBranchesToOwn.TryGetValue(level, out var branches) &&
            branches.Any(branch => branch.Equals(branchCode, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<CurrencyAmountV2> TotalsFor(IReadOnlyList<PlanStepV2> steps)
    {
        var totals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cost in steps.SelectMany(s => s.Costs))
        {
            Add(totals, cost.CurrencyCode, cost.Amount);
        }

        return SortCurrencyAmounts(totals);
    }

    private static void AddMasteryPurchaseSteps(List<PlanStepV2> steps, MasteryPlanV2? masteryPlan)
    {
        if (masteryPlan?.GoldStatus != GoldMasteryStatus.Planned)
        {
            return;
        }

        steps.Add(new PlanStepV2(
            steps.Count + 1,
            GenericAircraftId,
            PlanStepScope.Mastery,
            0,
            1,
            [new CurrencyAmountV2(Currencies.Gold, GoldMasteryPurchaseCost)]));
    }

    private static IReadOnlyList<CurrencyAmountV2> DeficitsFor(
        IReadOnlyList<CurrencyAmountV2> totals,
        IReadOnlyList<ResourceBalanceV2> balances)
    {
        var balancesByCurrency = balances
            .GroupBy(b => b.CurrencyCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Sum(b => Math.Max(0, b.Amount)), StringComparer.OrdinalIgnoreCase);
        var deficits = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var total in totals)
        {
            balancesByCurrency.TryGetValue(total.CurrencyCode, out var balance);
            var deficit = Math.Max(0, total.Amount - balance);
            if (deficit > 0)
            {
                deficits[total.CurrencyCode] = deficit;
            }
        }

        return SortCurrencyAmounts(deficits);
    }

    private static IReadOnlyList<CurrencyAmountV2> MasteryNormalRebateFor(MasteryPlanV2? masteryPlan)
    {
        if (masteryPlan is null)
        {
            return [];
        }

        var currentLevel = Math.Clamp(masteryPlan.CurrentMasteryLevel, 1, 24);
        var plannedLevel = Math.Clamp(masteryPlan.PlannedMasteryLevel, currentLevel, 24);
        var totals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (var level = currentLevel + 1; level <= plannedLevel; level++)
        {
            if (MasteryNonGoldRewards.TryGetValue(level, out var reward))
            {
                Add(totals, Currencies.AircraftParts, reward.aircraftParts);
                Add(totals, Currencies.Silver, reward.silver);
            }
        }

        return SortCurrencyAmounts(totals);
    }

    private static IReadOnlyList<CurrencyAmountV2> MasteryGoldRebateFor(MasteryPlanV2? masteryPlan)
    {
        if (masteryPlan is null)
        {
            return [];
        }

        var plannedLevel = Math.Clamp(masteryPlan.PlannedMasteryLevel, 1, 24);
        var totals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        if (masteryPlan.GoldStatus == GoldMasteryStatus.Planned)
        {
            for (var level = 1; level <= plannedLevel; level++)
            {
                if (MasteryGoldRewards.TryGetValue(level, out var reward))
                {
                    Add(totals, Currencies.AircraftParts, reward.aircraftParts);
                    Add(totals, Currencies.Silver, reward.silver);
                }
            }
        }

        return SortCurrencyAmounts(totals);
    }

    private static IReadOnlyList<CurrencyAmountV2> CombineCurrencyAmounts(
        params IReadOnlyList<CurrencyAmountV2>[] groups)
    {
        var totals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var group in groups)
        {
            foreach (var amount in group)
            {
                Add(totals, amount.CurrencyCode, amount.Amount);
            }
        }

        return SortCurrencyAmounts(totals);
    }

    private static IReadOnlyList<CurrencyAmountV2> NetGrindNeededFor(
        IReadOnlyList<CurrencyAmountV2> deficits,
        IReadOnlyList<CurrencyAmountV2> masteryRebate)
    {
        var rebateByCurrency = masteryRebate.ToDictionary(r => r.CurrencyCode, r => r.Amount, StringComparer.OrdinalIgnoreCase);
        var net = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var deficit in deficits)
        {
            rebateByCurrency.TryGetValue(deficit.CurrencyCode, out var rebate);
            var amount = Math.Max(0, deficit.Amount - rebate);
            if (amount > 0)
            {
                net[deficit.CurrencyCode] = amount;
            }
        }

        return SortCurrencyAmounts(net);
    }

    private static IReadOnlyList<CurrencyAmountV2> SortCurrencyAmounts(IDictionary<string, int> values)
    {
        return values
            .Where(v => v.Value > 0)
            .OrderBy(v => CurrencySort(v.Key))
            .ThenBy(v => v.Key, StringComparer.OrdinalIgnoreCase)
            .Select(v => new CurrencyAmountV2(v.Key, v.Value))
            .ToArray();
    }

    private static int CurrencySort(string currencyCode) => currencyCode switch
    {
        Currencies.Gold => 0,
        Currencies.Silver => 1,
        Currencies.AircraftParts => 2,
        Currencies.FuselageParts => 3,
        Currencies.EngineParts => 4,
        Currencies.AvionicsParts => 5,
        Currencies.CannonParts => 6,
        Currencies.MissileParts => 7,
        Currencies.RocketParts => 8,
        Currencies.SpecialAbilityBlueprints => 9,
        Currencies.PassiveAbilityBlueprints => 10,
        Currencies.AdvancedParts => 11,
        _ => 100
    };

    private static bool IsBranchCode(string branchCode) =>
        branchCode.Equals("A", StringComparison.OrdinalIgnoreCase) ||
        branchCode.Equals("B", StringComparison.OrdinalIgnoreCase);

    private static void Add(IDictionary<string, int> totals, string currencyCode, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        totals.TryGetValue(currencyCode, out var current);
        totals[currencyCode] = current + amount;
    }

    private static UpgradeCostRowV2 AircraftRow(int from, int to, int silver, int aircraftParts, string confidence) =>
        new("AIRCRAFT", from, to, silver, aircraftParts, 0, 0, "USER_CHART", confidence);

    private static UpgradeCostRowV2 SystemRow(int from, int to, int silver, int systemParts, int advancedParts, string confidence) =>
        new("SYSTEM", from, to, silver, 0, systemParts, advancedParts, "USER_CHART", confidence);

    private static UpgradeCostRowV2 AbilityRow(string upgradeKind, int from, int to, int silver, int blueprints) =>
        new(upgradeKind, from, to, silver, 0, blueprints, 0, "USER_SUPPLIED", "HIGH");

    private static UpgradeCostRowV2[] AbilityCosts(string upgradeKind) =>
        upgradeKind.ToUpperInvariant() switch
        {
            "SPECIAL" => SpecialCosts,
            "PASSIVE" => PassiveCosts,
            _ => throw new InvalidOperationException($"Unknown ability upgrade kind: {upgradeKind}.")
        };

    private static UpgradeCostRowV2 GetSlotUpgradeCost(SystemSlotDefinitionV2 slot, int fromLevel, int toLevel) =>
        slot.UpgradeKind.Equals("SYSTEM", StringComparison.OrdinalIgnoreCase)
            ? GetSystemUpgradeCost(fromLevel, toLevel)
            : GetAbilityUpgradeCost(slot.UpgradeKind, fromLevel, toLevel);
}
