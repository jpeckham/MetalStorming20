using MetalStorming20.Core;
using MetalStorming20.Web.Presenters;

namespace MetalStorming20.Web.Controllers;

public sealed class Upgrades2Controller
{
    private readonly Upgrades2PlannerSessionInteractor sessionInteractor;
    private readonly Upgrades2PlannerPresenter plannerPresenter;
    private readonly Upgrades2PlannerSessionPresenter sessionPresenter;

    public Upgrades2Controller(
        Upgrades2PlannerSessionInteractor sessionInteractor,
        Upgrades2PlannerPresenter plannerPresenter,
        Upgrades2PlannerSessionPresenter sessionPresenter)
    {
        this.sessionInteractor = sessionInteractor;
        this.plannerPresenter = plannerPresenter;
        this.sessionPresenter = sessionPresenter;
    }

    public Upgrades2PlannerViewModel ViewModel => plannerPresenter.ViewModel;

    public Upgrades2PlannerSessionViewModel SessionViewModel => sessionPresenter.ViewModel;

    public Upgrades2PlannerSession Session { get; } = new();

    public Task InitializeAsync(CancellationToken cancellationToken = default) =>
        sessionInteractor.InitializeAsync(Session, plannerPresenter, sessionPresenter, cancellationToken);

    public Task LoadSavedStateAsync(CancellationToken cancellationToken = default) =>
        sessionInteractor.LoadSavedStateAsync(Session, plannerPresenter, sessionPresenter, cancellationToken);

    public Task StartNewBuildAsync(CancellationToken cancellationToken = default) =>
        sessionInteractor.StartNewBuildAsync(Session, plannerPresenter, sessionPresenter, cancellationToken);

    public Task CycleAircraftLevelAsync(int level, CancellationToken cancellationToken = default) =>
        sessionInteractor.CycleAircraftLevelAsync(Session, level, plannerPresenter, sessionPresenter, cancellationToken);

    public Task CycleMasteryLevelAsync(int level, CancellationToken cancellationToken = default) =>
        sessionInteractor.CycleMasteryLevelAsync(Session, level, plannerPresenter, sessionPresenter, cancellationToken);

    public Task CycleGoldMasteryStatusAsync(CancellationToken cancellationToken = default) =>
        sessionInteractor.CycleGoldMasteryStatusAsync(Session, plannerPresenter, sessionPresenter, cancellationToken);

    public Task CycleNodeAsync(
        string systemSlotId,
        int level,
        string? branchCode,
        CancellationToken cancellationToken = default) =>
        sessionInteractor.CycleNodeAsync(Session, systemSlotId, level, branchCode, plannerPresenter, sessionPresenter, cancellationToken);
}
