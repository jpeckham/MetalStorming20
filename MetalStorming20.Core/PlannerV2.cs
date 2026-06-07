namespace MetalStorming20.Core;

public enum BranchOwnershipMode
{
    ChosenOnly,
    Both
}

public enum PlanStepScope
{
    Aircraft,
    System
}

public sealed record CurrencyAmountV2(string CurrencyCode, int Amount);

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
    IReadOnlyDictionary<int, string>? TargetEquippedBranches = null);

public sealed record SystemSlotDefinitionV2(
    string SystemSlotId,
    string AircraftId,
    string SystemTypeId,
    string PartCurrencyCode,
    string DisplayName,
    int UnlockAircraftLevel = 6);

public sealed record PlannerRequestV2(
    IReadOnlyList<AircraftStateV2> Aircraft,
    IReadOnlyList<OwnedSystemNodeV2> OwnedSystemNodes,
    IReadOnlyList<EquippedSystemBranchV2> EquippedSystemBranches,
    IReadOnlyList<ResourceBalanceV2> ResourceBalances,
    IReadOnlyList<AircraftTargetV2> AircraftTargets,
    IReadOnlyList<SystemTargetV2> SystemTargets,
    IReadOnlyList<SystemSlotDefinitionV2> SystemSlots);

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
    IReadOnlyList<PlanStepV2> Steps);

public static class PlannerV2
{
    public const string GenericAircraftId = "generic_aircraft";

    public static class Currencies
    {
        public const string Silver = "SILVER";
        public const string AircraftParts = "AIRCRAFT_PARTS";
        public const string AdvancedParts = "ADVANCED_PARTS";
        public const string FuselageParts = "FUSELAGE_PARTS";
        public const string EngineParts = "ENGINE_PARTS";
        public const string AvionicsParts = "AVIONICS_PARTS";
        public const string CannonParts = "CANNON_PARTS";
        public const string MissileParts = "MISSILE_PARTS";
        public const string RocketParts = "ROCKET_PARTS";
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
        new("generic_rockets", GenericAircraftId, "ROCKETS", Currencies.RocketParts, "Rockets")
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

    public static IReadOnlyList<UpgradeCostRowV2> AllUpgradeCosts =>
        AircraftCosts.Concat(SystemCosts).ToArray();

    public static UpgradeCostRowV2 GetAircraftUpgradeCost(int fromLevel, int toLevel) =>
        AircraftCosts.Single(c => c.FromLevel == fromLevel && c.ToLevel == toLevel);

    public static UpgradeCostRowV2 GetSystemUpgradeCost(int fromLevel, int toLevel) =>
        SystemCosts.Single(c => c.FromLevel == fromLevel && c.ToLevel == toLevel);

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

    public static PlannerResultV2 Plan(PlannerRequestV2 request)
    {
        var warnings = Validate(request);
        if (warnings.Count > 0)
        {
            return new PlannerResultV2(warnings, [], [], []);
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
                var currentSystemLevel = CurrentSystemLevel(request.OwnedSystemNodes, systemTarget.SystemSlotId);
                for (var level = currentSystemLevel; level < systemTarget.TargetSystemLevel; level++)
                {
                    var toLevel = level + 1;
                    var branchCodes = BranchCodesToPurchase(systemTarget, toLevel);
                    foreach (var branchCode in branchCodes)
                    {
                        if (IsOwned(request.OwnedSystemNodes, systemTarget.SystemSlotId, toLevel, branchCode))
                        {
                            continue;
                        }

                        var cost = GetSystemUpgradeCost(level, toLevel);
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

        var totals = TotalsFor(steps);
        var deficits = DeficitsFor(totals, request.ResourceBalances);
        return new PlannerResultV2([], totals, deficits, steps);
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

            if (target.TargetSystemLevel < 0 || target.TargetSystemLevel > 8)
            {
                warnings.Add($"{target.SystemSlotId} target system level must be between 0 and 8.");
            }

            var currentSystemLevel = CurrentSystemLevel(request.OwnedSystemNodes, target.SystemSlotId);
            if (target.TargetSystemLevel < currentSystemLevel)
            {
                warnings.Add($"{target.SystemSlotId} target system level cannot be below current owned level.");
            }

            if (target.OwnershipMode == BranchOwnershipMode.ChosenOnly && target.TargetSystemLevel >= 5)
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
                    if (equipped.Key < 5)
                    {
                        warnings.Add($"{target.SystemSlotId} cannot equip a branch below system level 5.");
                    }
                    else if (!IsBranchCode(equipped.Value))
                    {
                        warnings.Add($"{target.SystemSlotId} branch choice must be A or B.");
                    }
                    else if (equipped.Key <= currentSystemLevel &&
                        !IsOwned(request.OwnedSystemNodes, target.SystemSlotId, equipped.Key, equipped.Value))
                    {
                        warnings.Add($"{target.SystemSlotId} branch {equipped.Value} at level {equipped.Key} is not owned.");
                    }
                }
            }
        }

        foreach (var equipped in request.EquippedSystemBranches)
        {
            if (equipped.SystemLevel < 5)
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
                    if (!nodes.Any(n =>
                            n.SystemLevel == level &&
                            string.Equals(n.BranchCode, branchNode.BranchCode, StringComparison.OrdinalIgnoreCase)))
                    {
                        warnings.Add($"{slotGroup.Key} has an inconsistent ownership graph: branch {branchNode.BranchCode} level {branchNode.SystemLevel} requires level {level}.");
                    }
                }
            }
        }

        return warnings.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string?[] BranchCodesToPurchase(SystemTargetV2 target, int toLevel)
    {
        if (toLevel < 5)
        {
            return [null];
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

    private static IReadOnlyList<CurrencyAmountV2> TotalsFor(IReadOnlyList<PlanStepV2> steps)
    {
        var totals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cost in steps.SelectMany(s => s.Costs))
        {
            Add(totals, cost.CurrencyCode, cost.Amount);
        }

        return SortCurrencyAmounts(totals);
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
        Currencies.Silver => 0,
        Currencies.AircraftParts => 1,
        Currencies.FuselageParts => 2,
        Currencies.EngineParts => 3,
        Currencies.AvionicsParts => 4,
        Currencies.CannonParts => 5,
        Currencies.MissileParts => 6,
        Currencies.RocketParts => 7,
        Currencies.AdvancedParts => 8,
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
}
