# MetalStorm Planner

MetalStorm Planner is a Blazor WebAssembly app that helps MetalStorm players forecast the parts and silver needed to finish upgrading a plane, and the mastery rewards that offset those costs. The UI is static and GitHub Pages–friendly, so it can be published directly from the generated `wwwroot` artifacts without a backend server.

## Projects
- **MetalStorming20.Core** – Shared library with the planner math (upgrade steps, mastery reward logic, clamping helpers).
- **MetalStorming20.Web** – Blazor WebAssembly frontend that hosts the planner UI and static assets.
- **MetalStorming20.Tests** – Existing xUnit suite that exercises the planner calculations.
- **MetalStorming20.PlaywrightTests** – New MSTest + Playwright project for UI smoke tests against the published site.

## Running the app locally
1. Install the .NET 8 SDK.
2. From the repo root, run `dotnet restore`.
3. Start the WebAssembly app locally:
   ```bash
   dotnet run --project MetalStorming20.Web/MetalStorming20.Web.csproj
   ```
4. Visit the app at the printed local URL (by default `http://localhost:5000/`).

## Testing
### Unit tests (planner math)
Run the xUnit suite from the repository root:
```bash
dotnet test MetalStorming20.Tests/MetalStorming20.Tests.csproj
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

The GitHub Pages workflow publishes the site and then runs these Playwright tests against the deployed URL to ensure navigation and calculations work after each deployment.

## Deployment
GitHub Pages is configured via `.github/workflows/pages.yml` to:
1. Publish the Blazor WebAssembly app with an explicit `BaseHref` and `StaticWebAssetBasePath` so assets resolve under the repository name.
2. Upload the generated `wwwroot` artifacts and deploy them to Pages.
3. Run the Playwright smoke test job against the freshly deployed Pages URL to validate deep-link routing and the calculator flow.

