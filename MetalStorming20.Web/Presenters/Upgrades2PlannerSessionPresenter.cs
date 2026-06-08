using MetalStorming20.Core;

namespace MetalStorming20.Web.Presenters;

public sealed record Upgrades2LevelNodeViewModel(
    int Level,
    string Label,
    string CssClass,
    string State,
    bool IsPressed);

public sealed record Upgrades2SystemNodeViewModel(
    int Level,
    string? BranchCode,
    string Label,
    string CssClass,
    string State,
    bool IsPressed);

public sealed record Upgrades2SystemPlanRowViewModel(
    string SystemSlotId,
    string DisplayName,
    string Slug,
    IReadOnlyList<Upgrades2SystemNodeViewModel> TrunkNodes,
    IReadOnlyList<IReadOnlyList<Upgrades2SystemNodeViewModel>> BranchNodeStacks);

public sealed record Upgrades2SavedBuildViewModel(string Id, string Name);

public sealed record Upgrades2PlannerSessionViewModel(
    IReadOnlyList<Upgrades2LevelNodeViewModel> AircraftLevels,
    IReadOnlyList<Upgrades2LevelNodeViewModel> MasteryLevels,
    string GoldCssClass,
    string GoldState,
    bool IsGoldPressed,
    IReadOnlyList<Upgrades2SystemPlanRowViewModel> SystemPlans,
    string? SelectedBuildId,
    string SelectedBuildName,
    IReadOnlyList<Upgrades2SavedBuildViewModel> Builds)
{
    public static Upgrades2PlannerSessionViewModel Empty { get; } =
        new([], [], "node-toggle gold-toggle", "off", false, [], null, "unnamed", []);
}

public sealed class Upgrades2PlannerSessionPresenter : IUpgrades2PlannerSessionPresenter
{
    public Upgrades2PlannerSessionViewModel ViewModel { get; private set; } = Upgrades2PlannerSessionViewModel.Empty;

    public void Present(Upgrades2PlannerSessionResponse response)
    {
        ViewModel = new Upgrades2PlannerSessionViewModel(
            response.AircraftLevels.Select(ToLevelViewModel).ToArray(),
            response.MasteryLevels.Select(ToLevelViewModel).ToArray(),
            GoldCssClass(response.GoldMasteryStatus),
            NodeState(response.GoldMasteryStatus),
            response.GoldMasteryStatus != GoldMasteryStatus.Off,
            response.Systems.Select(ToSystemPlanViewModel).ToArray(),
            response.SelectedBuildId,
            response.SelectedBuildName,
            response.Builds.Select(build => new Upgrades2SavedBuildViewModel(build.Id, build.Name)).ToArray());
    }

    private static Upgrades2LevelNodeViewModel ToLevelViewModel(Upgrades2LevelSelectionResponse level) =>
        new(
            level.Level,
            level.Level.ToString(),
            NodeCssClass(level.State),
            NodeState(level.State),
            IsPressed(level.State));

    private static Upgrades2SystemPlanRowViewModel ToSystemPlanViewModel(Upgrades2SystemPlanResponse system)
    {
        var nodes = system.Nodes
            .Select(node => new Upgrades2SystemNodeViewModel(
                node.Level,
                node.BranchCode,
                NodeLabel(node.Level, node.BranchCode),
                NodeCssClass(node.State),
                NodeState(node.State),
                IsPressed(node.State)))
            .ToArray();

        var trunkNodes = nodes.Where(node => node.BranchCode is null).ToArray();
        var branchStacks = nodes
            .Where(node => node.BranchCode is not null)
            .GroupBy(node => node.Level)
            .OrderBy(group => group.Key)
            .Select(group => (IReadOnlyList<Upgrades2SystemNodeViewModel>)group
                .OrderBy(node => node.BranchCode, StringComparer.OrdinalIgnoreCase)
                .ToArray())
            .ToArray();

        return new Upgrades2SystemPlanRowViewModel(
            system.SystemSlotId,
            system.DisplayName,
            Slug(system.DisplayName),
            trunkNodes,
            branchStacks);
    }

    private static string NodeCssClass(Upgrades2NodeSelectionState state) =>
        state switch
        {
            Upgrades2NodeSelectionState.Desired => "node-toggle desired",
            Upgrades2NodeSelectionState.Owned => "node-toggle has",
            _ => "node-toggle"
        };

    private static string NodeState(Upgrades2NodeSelectionState state) =>
        state switch
        {
            Upgrades2NodeSelectionState.Desired => "desired",
            Upgrades2NodeSelectionState.Owned => "has",
            _ => "off"
        };

    private static string GoldCssClass(GoldMasteryStatus status) =>
        status switch
        {
            GoldMasteryStatus.Planned => "node-toggle gold-toggle desired",
            GoldMasteryStatus.Owned => "node-toggle gold-toggle has",
            _ => "node-toggle gold-toggle"
        };

    private static string NodeState(GoldMasteryStatus status) =>
        status switch
        {
            GoldMasteryStatus.Planned => "desired",
            GoldMasteryStatus.Owned => "has",
            _ => "off"
        };

    private static bool IsPressed(Upgrades2NodeSelectionState state) =>
        state is Upgrades2NodeSelectionState.Desired or Upgrades2NodeSelectionState.Owned;

    private static string NodeLabel(int level, string? branchCode) =>
        branchCode is null ? level.ToString() : $"{level}{branchCode}";

    private static string Slug(string value) =>
        value.ToLowerInvariant()
            .Replace("/", "-", StringComparison.Ordinal)
            .Replace(" ", "-", StringComparison.Ordinal);
}
