namespace MetalStorming20.Core;

public enum Upgrades2NodeSelectionState
{
    Off,
    Desired,
    Owned
}

public sealed record Upgrades2SystemPlanInput(
    string SystemSlotId,
    IReadOnlyDictionary<string, Upgrades2NodeSelectionState> NodeStates);

public sealed record Upgrades2PlannerInput(
    int CurrentAircraftLevel,
    int TargetAircraftLevel,
    IReadOnlyList<Upgrades2SystemPlanInput> Systems,
    MasteryPlanV2? MasteryPlan);

public sealed record Upgrades2PlannerResponse(
    IReadOnlyList<SystemSlotDefinitionV2> SystemSlots,
    PlannerResultV2 Result);

public interface IUpgrades2CatalogGateway
{
    Task<IReadOnlyList<SystemSlotDefinitionV2>> GetSystemSlotsAsync(CancellationToken cancellationToken = default);
}

public interface IUpgrades2PlannerPresenter
{
    void Present(Upgrades2PlannerResponse response);
}

public sealed class Upgrades2PlannerUseCase
{
    private readonly IUpgrades2CatalogGateway catalogGateway;

    public Upgrades2PlannerUseCase(IUpgrades2CatalogGateway catalogGateway)
    {
        this.catalogGateway = catalogGateway;
    }

    public async Task HandleAsync(
        Upgrades2PlannerInput input,
        IUpgrades2PlannerPresenter presenter,
        CancellationToken cancellationToken = default)
    {
        var systemSlots = await catalogGateway.GetSystemSlotsAsync(cancellationToken);
        var genericSystemSlots = systemSlots
            .Where(s => s.AircraftId == PlannerV2.GenericAircraftId)
            .DefaultIfEmpty()
            .Where(s => s is not null)
            .Cast<SystemSlotDefinitionV2>()
            .ToArray();

        if (genericSystemSlots.Length == 0)
        {
            genericSystemSlots = PlannerV2.GenericSystemSlots.ToArray();
        }

        var request = BuildPlannerRequest(input, genericSystemSlots);
        presenter.Present(new Upgrades2PlannerResponse(genericSystemSlots, PlannerV2.Plan(request)));
    }

    public static PlannerRequestV2 BuildPlannerRequest(
        Upgrades2PlannerInput input,
        IReadOnlyList<SystemSlotDefinitionV2> systemSlots)
    {
        var systemsBySlotId = input.Systems.ToDictionary(s => s.SystemSlotId, StringComparer.OrdinalIgnoreCase);
        var systemTargets = new List<SystemTargetV2>();
        var ownedNodes = new List<OwnedSystemNodeV2>();
        var equippedBranches = new List<EquippedSystemBranchV2>();

        foreach (var slot in systemSlots)
        {
            var system = systemsBySlotId.TryGetValue(slot.SystemSlotId, out var plannedSystem)
                ? plannedSystem
                : new Upgrades2SystemPlanInput(slot.SystemSlotId, new Dictionary<string, Upgrades2NodeSelectionState>());

            ownedNodes.AddRange(BuildOwnedNodes(system));
            equippedBranches.AddRange(BuildEquippedBranches(system));
            var systemTarget = BuildSystemTarget(system, slot);
            if (systemTarget.TargetSystemLevel > 0)
            {
                systemTargets.Add(systemTarget);
            }
        }

        var hasAircraftSelection = input.TargetAircraftLevel > 0;

        return new PlannerRequestV2(
            Aircraft: hasAircraftSelection
                ? [new AircraftStateV2(PlannerV2.GenericAircraftId, true, Math.Max(1, input.CurrentAircraftLevel))]
                : [],
            OwnedSystemNodes: ownedNodes,
            EquippedSystemBranches: equippedBranches,
            ResourceBalances: [],
            AircraftTargets: hasAircraftSelection
                ? [new AircraftTargetV2(PlannerV2.GenericAircraftId, input.TargetAircraftLevel)]
                : [],
            SystemTargets: systemTargets,
            SystemSlots: systemSlots,
            MasteryPlan: input.MasteryPlan);
    }

    private static SystemTargetV2 BuildSystemTarget(Upgrades2SystemPlanInput system, SystemSlotDefinitionV2 slot)
    {
        var targetSystemLevel = TargetSystemLevel(system);
        return new SystemTargetV2(
            system.SystemSlotId,
            targetSystemLevel,
            BranchOwnershipMode(system, slot),
            BuildBranchTargets(system, targetSystemLevel, slot),
            BuildBranchTargetsToOwn(system, targetSystemLevel, slot));
    }

    private static BranchOwnershipMode BranchOwnershipMode(Upgrades2SystemPlanInput system, SystemSlotDefinitionV2 slot) =>
        slot.UsesBranches && Enumerable.Range(5, 4).Any(level =>
            IsTarget(system, level, "A") &&
            IsTarget(system, level, "B"))
            ? MetalStorming20.Core.BranchOwnershipMode.Both
            : MetalStorming20.Core.BranchOwnershipMode.ChosenOnly;

    private static IReadOnlyDictionary<int, string>? BuildBranchTargets(
        Upgrades2SystemPlanInput system,
        int targetSystemLevel,
        SystemSlotDefinitionV2 slot)
    {
        return slot.UsesBranches && targetSystemLevel >= 5
            ? Enumerable.Range(5, targetSystemLevel - 4)
                .ToDictionary(level => level, level => SelectedBranchFor(system, level) ?? "A")
            : null;
    }

    private static IReadOnlyDictionary<int, IReadOnlyList<string>>? BuildBranchTargetsToOwn(
        Upgrades2SystemPlanInput system,
        int targetSystemLevel,
        SystemSlotDefinitionV2 slot)
    {
        if (!slot.UsesBranches || targetSystemLevel < 5)
        {
            return null;
        }

        return Enumerable.Range(5, targetSystemLevel - 4)
            .Select(level => new
            {
                Level = level,
                Branches = new[] { "A", "B" }
                    .Where(branch => IsTarget(system, level, branch))
                    .ToArray()
            })
            .Where(target => target.Branches.Length > 0)
            .ToDictionary(
                target => target.Level,
                target => (IReadOnlyList<string>)target.Branches);
    }

    private static IEnumerable<OwnedSystemNodeV2> BuildOwnedNodes(Upgrades2SystemPlanInput system) =>
        system.NodeStates
            .Where(node => node.Value == Upgrades2NodeSelectionState.Owned)
            .Select(node => new OwnedSystemNodeV2(system.SystemSlotId, ParseLevel(node.Key), ParseBranch(node.Key)));

    private static IEnumerable<EquippedSystemBranchV2> BuildEquippedBranches(Upgrades2SystemPlanInput system) =>
        system.NodeStates
            .Where(node => node.Value == Upgrades2NodeSelectionState.Owned)
            .Select(node => new { Level = ParseLevel(node.Key), Branch = ParseBranch(node.Key) })
            .Where(node => node.Level >= 5 && node.Branch is not null)
            .GroupBy(node => node.Level)
            .Select(group => group.OrderBy(node => node.Branch, StringComparer.OrdinalIgnoreCase).First())
            .Select(node => new EquippedSystemBranchV2(system.SystemSlotId, node.Level, node.Branch!));

    private static int TargetSystemLevel(Upgrades2SystemPlanInput system)
    {
        var targetLevels = system.NodeStates
            .Where(node => node.Value is Upgrades2NodeSelectionState.Desired or Upgrades2NodeSelectionState.Owned)
            .Select(node => ParseLevel(node.Key))
            .Where(level => level > 0)
            .ToArray();

        return targetLevels.Length == 0 ? 0 : targetLevels.Max();
    }

    private static string? SelectedBranchFor(Upgrades2SystemPlanInput system, int level)
    {
        if (IsTarget(system, level, "A"))
        {
            return "A";
        }

        return IsTarget(system, level, "B") ? "B" : null;
    }

    private static bool IsTarget(Upgrades2SystemPlanInput system, int level, string? branchCode) =>
        system.NodeStates.TryGetValue(NodeKey(level, branchCode), out var state) &&
        state is Upgrades2NodeSelectionState.Desired or Upgrades2NodeSelectionState.Owned;

    private static string NodeKey(int level, string? branchCode) =>
        branchCode is null ? level.ToString() : $"{level}{branchCode}";

    private static int ParseLevel(string key) =>
        int.TryParse(key[..1], out var level) ? level : 0;

    private static string? ParseBranch(string key) =>
        key.Length > 1 ? key[1..].ToUpperInvariant() : null;
}
