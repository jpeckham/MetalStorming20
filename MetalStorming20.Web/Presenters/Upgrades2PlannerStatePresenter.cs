using MetalStorming20.Core;

namespace MetalStorming20.Web.Presenters;

public sealed record Upgrades2PlannerStateViewModel(Upgrades2PlannerState? State)
{
    public static Upgrades2PlannerStateViewModel Empty { get; } = new((Upgrades2PlannerState?)null);
}

public sealed class Upgrades2PlannerStatePresenter : IUpgrades2PlannerStatePresenter
{
    public Upgrades2PlannerStateViewModel ViewModel { get; private set; } = Upgrades2PlannerStateViewModel.Empty;

    public void Present(Upgrades2PlannerStateResponse response)
    {
        ViewModel = new Upgrades2PlannerStateViewModel(response.State);
    }
}
