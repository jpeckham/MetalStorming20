# MetalStorm Planner

<!-- badges:start -->
## Status Badges

[![Continuous Delivery](https://github.com/jpeckham/MetalStorming20/actions/workflows/continuous-delivery-dotnet-blazor-github-pages.yml/badge.svg)](https://github.com/jpeckham/MetalStorming20/actions/workflows/continuous-delivery-dotnet-blazor-github-pages.yml)
[![Coverage](.github/badges/coverage.svg)](https://jpeckham.github.io/MetalStorming20/coverage/)
<!-- badges:end -->

MetalStorm Planner is a Blazor WebAssembly app that helps MetalStorm players forecast Upgrades 2.0 costs. The app loads directly into the new planner for aircraft levels, system tracks, branch ownership, required resources, and purchase steps. The UI is static and GitHub Pages-friendly, so it can be published directly from the generated `wwwroot` artifacts without a backend server.

## Projects
- **MetalStorming20.Core** – Shared library with the `PlannerV2` Upgrades 2.0 engine.
- **MetalStorming20.Web** – Blazor WebAssembly frontend that hosts the planner and static catalog assets.
- **MetalStorming20.Tests** – xUnit suite that exercises Upgrades 2.0 costs, dependency logic, and static catalog files.
- **MetalStorming20.PlaywrightTests** – MSTest + Playwright project for UI smoke tests against the published site or a local dev server.

## Route
- `/` - Upgrades 2.0 planner for a generic aircraft, all upgrade types, square node toggles for levels `1-4` and branch nodes `5A-8B`, required resource totals, and purchase steps.

## Upgrades 2.0 static catalogs
The Upgrades 2.0 planner keeps seed data in immutable static JSON files under `MetalStorming20.Web/wwwroot/data/v2/`:

- `/data/v2/schema-version.json`
- `/data/v2/currencies.json`
- `/data/v2/aircraft.json`
- `/data/v2/aircraft-milestones.json`
- `/data/v2/system-types.json`
- `/data/v2/aircraft-system-slots.json`
- `/data/v2/branch-families.json`
- `/data/v2/system-node-definitions.json`
- `/data/v2/upgrade-costs.json`

The seed catalog includes the prompt-provided aircraft and system upgrade cost rows, official aircraft milestone shape, supported currencies, and one generic system-slot catalog. It deliberately shows every supported upgrade type without requiring an aircraft pick: Fuselage, Engines, Avionics, Cannons, Main/Radar Missile, Secondary/IR Missile, and Rockets. Each system uses node toggles laid out as `[1] [2] [3] [4] [5A] [6A] [7A] [8A]` with `[5B] [6B] [7B] [8B]` stacked below the branch columns.

Each node cycles through off, has, and desired. Has nodes define the current state, desired nodes define the target state, and the planner calculates the remaining cost from current state to desired state. Exact branch names and node bonus text remain intentionally incremental seed-data work.

## Browser state
The planner persists its generic form state to `localStorage` using schema version `2`. No server API, database, auth, or backend dependency is required.

## Running the app locally
1. Install the .NET 8 SDK.
2. From the repo root, run `dotnet restore`.
3. Start the WebAssembly app locally:
   ```bash
   dotnet run --project MetalStorming20.Web/MetalStorming20.Web.csproj
   ```
4. Visit the app at the printed local URL.

## Testing
### Unit tests (planner math)
Run the xUnit suite from the repository root:
```bash
dotnet test MetalStorming20.Tests/MetalStorming20.Tests.csproj
```

Focused Upgrades 2.0 tests:
```bash
dotnet test MetalStorming20.Tests/MetalStorming20.Tests.csproj --filter "PlannerV2CostTests|PlannerV2DependencyTests|Upgrades2CatalogFiles"
```

### UI smoke tests (Playwright)
The Playwright tests can point at either a local development server or the GitHub Pages deployment. They default to `http://localhost:5000/` but accept a `PLAYWRIGHT_BASE_URL` environment variable to override the target.

1. Build the Playwright test project:
   ```bash
   dotnet build MetalStorming20.PlaywrightTests/MetalStorming20.PlaywrightTests.csproj -c Release
   ```
2. Install the Playwright browsers (once per environment):
   ```bash
   pwsh MetalStorming20.PlaywrightTests/bin/Release/net8.0/playwright.ps1 install --with-deps
   ```
3. Run the tests (set `PLAYWRIGHT_BASE_URL` to your deployed Pages URL when validating GitHub Pages):
   ```bash
   PLAYWRIGHT_BASE_URL="https://<user>.github.io/<repo>/" dotnet test MetalStorming20.PlaywrightTests/MetalStorming20.PlaywrightTests.csproj -c Release
   ```

The GitHub Pages workflow publishes the site and then runs these Playwright tests against the deployed URL to ensure routing and calculations work after each deployment.

## Deployment
GitHub Pages and release automation are configured via `.github/workflows/continuous-delivery-dotnet-blazor-github-pages.yml` to:
1. Publish the Blazor WebAssembly app with an explicit `BaseHref` and `StaticWebAssetBasePath` so assets resolve under the repository name.
2. Upload the generated `wwwroot` artifacts and deploy them to Pages.
3. Run the Playwright smoke test job against the freshly deployed Pages URL to validate deep-link routing and the calculator flow.
