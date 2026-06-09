using MetalStorming20.Core;

namespace MetalStorming20.Web.Presenters;

public sealed record Upgrades2PlannerViewModel(
    IReadOnlyList<SystemSlotDefinitionV2> SystemSlots,
    Upgrades2PlannerResultViewModel? Result,
    IReadOnlyList<Upgrades2CurrencyFilterViewModel> CurrencyFilters)
{
    private static readonly Upgrades2CurrencyFilterViewModel[] CurrencyFilterOptions =
    [
        new("", "All currencies"),
        new(PlannerV2.Currencies.Gold, PlannerV2.Currencies.Gold),
        new(PlannerV2.Currencies.Silver, PlannerV2.Currencies.Silver),
        new(PlannerV2.Currencies.AircraftParts, PlannerV2.Currencies.AircraftParts),
        new(PlannerV2.Currencies.FuselageParts, PlannerV2.Currencies.FuselageParts),
        new(PlannerV2.Currencies.EngineParts, PlannerV2.Currencies.EngineParts),
        new(PlannerV2.Currencies.AvionicsParts, PlannerV2.Currencies.AvionicsParts),
        new(PlannerV2.Currencies.CannonParts, PlannerV2.Currencies.CannonParts),
        new(PlannerV2.Currencies.MissileParts, PlannerV2.Currencies.MissileParts),
        new(PlannerV2.Currencies.RocketParts, PlannerV2.Currencies.RocketParts),
        new(PlannerV2.Currencies.AdvancedParts, PlannerV2.Currencies.AdvancedParts)
    ];

    public static Upgrades2PlannerViewModel Empty { get; } = new([], null, CurrencyFilterOptions);
}

public sealed record Upgrades2CurrencyFilterViewModel(string Value, string Label);

public sealed record Upgrades2CurrencyAmountViewModel(string CurrencyCode, string DisplayText);

public sealed record Upgrades2PlanStepViewModel(
    int Order,
    string StepText,
    string CostsText,
    IReadOnlyList<string> CurrencyCodes);

public sealed record Upgrades2PlannerResultViewModel(
    bool HasWarnings,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<Upgrades2CurrencyAmountViewModel> TotalsRequired,
    bool HasMasteryRebate,
    IReadOnlyList<Upgrades2CurrencyAmountViewModel> MasteryRebate,
    bool HasMasteryNormalRebate,
    IReadOnlyList<Upgrades2CurrencyAmountViewModel> MasteryNormalRebate,
    bool HasMasteryGoldRebate,
    IReadOnlyList<Upgrades2CurrencyAmountViewModel> MasteryGoldRebate,
    bool HasNetGrindNeeded,
    IReadOnlyList<Upgrades2CurrencyAmountViewModel> NetGrindNeeded,
    IReadOnlyList<Upgrades2PlanStepViewModel> Steps);

public sealed class Upgrades2PlannerPresenter : IUpgrades2PlannerPresenter
{
    public Upgrades2PlannerViewModel ViewModel { get; private set; } = Upgrades2PlannerViewModel.Empty;

    public void Present(Upgrades2PlannerResponse response)
    {
        var systemDisplayNames = response.SystemSlots.ToDictionary(
            slot => slot.SystemSlotId,
            slot => slot.DisplayName,
            StringComparer.OrdinalIgnoreCase);

        ViewModel = new Upgrades2PlannerViewModel(
            response.SystemSlots,
            ToResultViewModel(response.Result, systemDisplayNames),
            Upgrades2PlannerViewModel.Empty.CurrencyFilters);
    }

    private static Upgrades2PlannerResultViewModel ToResultViewModel(
        PlannerResultV2 result,
        IReadOnlyDictionary<string, string> systemDisplayNames)
    {
        var netGrindNeeded = result.NetGrindNeeded.Select(ToCurrencyAmountViewModel).ToArray();
        var masteryNormalRebate = result.MasteryNormalRebate.Select(ToCurrencyAmountViewModel).ToArray();
        var masteryGoldRebate = result.MasteryGoldRebate.Select(ToCurrencyAmountViewModel).ToArray();
        return new Upgrades2PlannerResultViewModel(
            result.Warnings.Count > 0,
            result.Warnings,
            result.TotalsRequired.Select(ToCurrencyAmountViewModel).ToArray(),
            result.MasteryRebate.Count > 0,
            result.MasteryRebate.Select(ToCurrencyAmountViewModel).ToArray(),
            masteryNormalRebate.Length > 0,
            masteryNormalRebate,
            masteryGoldRebate.Length > 0,
            masteryGoldRebate,
            netGrindNeeded.Length > 0,
            netGrindNeeded,
            result.Steps.Select(step => ToPlanStepViewModel(step, systemDisplayNames)).ToArray());
    }

    private static Upgrades2CurrencyAmountViewModel ToCurrencyAmountViewModel(CurrencyAmountV2 amount) =>
        new(amount.CurrencyCode, $"{amount.CurrencyCode} {amount.Amount:N0}");

    private static Upgrades2PlanStepViewModel ToPlanStepViewModel(
        PlanStepV2 step,
        IReadOnlyDictionary<string, string> systemDisplayNames)
    {
        var stepText = StepText(step, systemDisplayNames);

        return new Upgrades2PlanStepViewModel(
            step.Order,
            stepText,
            string.Join(", ", step.Costs.Select(cost => $"{cost.CurrencyCode} {cost.Amount:N0}")),
            step.Costs.Select(cost => cost.CurrencyCode).Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private static string StepText(
        PlanStepV2 step,
        IReadOnlyDictionary<string, string> systemDisplayNames) =>
        step.Scope switch
        {
            PlanStepScope.Mastery => "Gold Mastery",
            PlanStepScope.Aircraft => $"Aircraft {step.FromLevel}->{step.ToLevel}",
            _ => $"{SystemStepDisplayName(step, systemDisplayNames)} {step.FromLevel}->{step.ToLevel}"
        };

    private static string SystemStepDisplayName(
        PlanStepV2 step,
        IReadOnlyDictionary<string, string> systemDisplayNames) =>
        step.SystemSlotId is not null && systemDisplayNames.TryGetValue(step.SystemSlotId, out var displayName)
            ? displayName
            : step.SystemSlotId ?? "System";
}
