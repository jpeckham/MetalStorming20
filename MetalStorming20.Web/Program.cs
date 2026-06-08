using MetalStorming20.Web.Components;
using MetalStorming20.Core;
using MetalStorming20.Web.Controllers;
using MetalStorming20.Web.Presenters;
using MetalStorming20.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<Upgrades2CatalogService>();
builder.Services.AddScoped<IUpgrades2CatalogGateway>(sp => sp.GetRequiredService<Upgrades2CatalogService>());
builder.Services.AddScoped<IUpgrades2PlannerStateGateway, Upgrades2LocalStoragePlannerStateGateway>();
builder.Services.AddScoped<Upgrades2PlannerUseCase>();
builder.Services.AddScoped<LoadUpgrades2PlannerStateUseCase>();
builder.Services.AddScoped<SaveUpgrades2PlannerStateUseCase>();
builder.Services.AddScoped<PresentUpgrades2PlannerSessionUseCase>();
builder.Services.AddScoped<Upgrades2PlannerSessionInteractor>();
builder.Services.AddScoped<Upgrades2PlannerPresenter>();
builder.Services.AddScoped<Upgrades2PlannerStatePresenter>();
builder.Services.AddScoped<Upgrades2PlannerSessionPresenter>();
builder.Services.AddScoped<Upgrades2Controller>();

await builder.Build().RunAsync();
