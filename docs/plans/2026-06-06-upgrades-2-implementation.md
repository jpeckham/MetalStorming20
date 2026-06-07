# Upgrades 2 Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add an additive Upgrades 2.0 planner that preserves the legacy calculator while supporting aircraft levels, system tracks, branch ownership, per-currency deficits, static catalogs, exports, and documentation.

**Architecture:** Keep all deterministic planning in `MetalStorming20.Core`, expose immutable seed data as static JSON under `MetalStorming20.Web/wwwroot/data/v2/`, and build a GitHub Pages-compatible Blazor route at `/upgrades-2`. Browser state remains local to the client and uses schema version `2`; no backend is introduced.

**Tech Stack:** .NET Blazor WebAssembly, `MetalStorming20.Core`, xUnit, MSTest Playwright, static JSON catalogs.

---

### Task 1: Core Catalog And DTO Tests

**Files:**
- Create: `MetalStorming20.Tests/PlannerV2Tests.cs`
- Create: `MetalStorming20.Core/PlannerV2.cs`

**Step 1: Write failing tests**

Add tests for:
- aircraft 4 to 5 and 5 to 6 lookup rows
- aircraft 1 to 20 sum
- system 0 to 1 and 4 to 5 lookup rows
- system chosen-only 0 to 8 sum
- system both-branches 0 to 8 derived sum

**Step 2: Run red**

Run: `dotnet test MetalStorming20.Tests/MetalStorming20.Tests.csproj --filter PlannerV2CostTests`

Expected: fail because `PlannerV2` and V2 DTOs do not exist.

**Step 3: Implement minimal core catalog**

Create V2 enums/records, seed cost rows, lookup helpers, and prefix-sum helpers.

**Step 4: Run green**

Run: `dotnet test MetalStorming20.Tests/MetalStorming20.Tests.csproj --filter PlannerV2CostTests`

Expected: pass.

### Task 2: Planner Dependency And Validation Tests

**Files:**
- Modify: `MetalStorming20.Tests/PlannerV2Tests.cs`
- Modify: `MetalStorming20.Core/PlannerV2.cs`

**Step 1: Write failing tests**

Add tests for:
- system target on aircraft level 5 inserts aircraft 5 to 6 first
- equipped-only branch change costs zero when both level-5 branches are owned
- `BOTH` mode below level 5 costs the same as chosen-only
- `BOTH` mode from level 4 to 5 doubles only the branch level cost
- duplicate missile slots are tracked separately by `systemSlotId`
- invalid branch target returns validation warnings and no steps

**Step 2: Run red**

Run: `dotnet test MetalStorming20.Tests/MetalStorming20.Tests.csproj --filter PlannerV2DependencyTests`

Expected: fail because planner execution is not implemented.

**Step 3: Implement minimal planner**

Add request/result records, validation, aircraft dependency insertion, system delta generation, branch ownership modes, totals, deficits, and ordered steps.

**Step 4: Run green**

Run: `dotnet test MetalStorming20.Tests/MetalStorming20.Tests.csproj --filter PlannerV2DependencyTests`

Expected: pass.

### Task 3: Static Catalog Seeds

**Files:**
- Create: `MetalStorming20.Web/wwwroot/data/v2/schema-version.json`
- Create: `MetalStorming20.Web/wwwroot/data/v2/currencies.json`
- Create: `MetalStorming20.Web/wwwroot/data/v2/aircraft-milestones.json`
- Create: `MetalStorming20.Web/wwwroot/data/v2/system-types.json`
- Create: `MetalStorming20.Web/wwwroot/data/v2/aircraft.json`
- Create: `MetalStorming20.Web/wwwroot/data/v2/aircraft-system-slots.json`
- Create: `MetalStorming20.Web/wwwroot/data/v2/branch-families.json`
- Create: `MetalStorming20.Web/wwwroot/data/v2/system-node-definitions.json`
- Create: `MetalStorming20.Web/wwwroot/data/v2/upgrade-costs.json`

**Step 1: Write failing manifest test**

Add a test that confirms each required JSON file exists and key seed counts match the prompt.

**Step 2: Run red**

Run: `dotnet test MetalStorming20.Tests/MetalStorming20.Tests.csproj --filter Upgrades2CatalogFiles`

Expected: fail because static catalog files do not exist.

**Step 3: Add seed files**

Add all required catalog files with prompt-provided currencies, milestones, system types, cost rows, and a minimal aircraft/slot seed for F-106 plus duplicate missile slots.

**Step 4: Run green**

Run: `dotnet test MetalStorming20.Tests/MetalStorming20.Tests.csproj --filter Upgrades2CatalogFiles`

Expected: pass.

### Task 4: Web Route And Export UI

**Files:**
- Create: `MetalStorming20.Web/Components/Pages/Upgrades2.razor`
- Modify: `MetalStorming20.Web/Components/Layout/NavMenu.razor`
- Modify: `MetalStorming20.PlaywrightTests/PlannerPageTests.cs`

**Step 1: Write failing Playwright test**

Add smoke test for `/upgrades-2` that enters current state, target, balances, runs planner, and sees totals, deficits, steps, JSON export, and markdown export. Keep existing legacy route test.

**Step 2: Run red**

Run against a local server: `dotnet test MetalStorming20.PlaywrightTests/MetalStorming20.PlaywrightTests.csproj --filter Upgrades2PageLoadsAndCalculates`

Expected: fail because route does not exist.

**Step 3: Implement route**

Build a compact planner page with current state, target build, branch controls, balances, results, exports, validation warnings, and filters. Keep state local to the component with `schemaVersion = 2` labels and no backend dependency.

**Step 4: Run green**

Run the Playwright smoke test again with `PLAYWRIGHT_BASE_URL` set to the local server.

Expected: pass.

### Task 5: Documentation And Final Verification

**Files:**
- Modify: `README.md`
- Modify: `MetalStorming20.Web/README.md`

**Step 1: Document behavior**

Update docs with the legacy route, `/upgrades-2`, static catalog URLs, export behavior, browser-state schema version, test commands, and known seed-data limitations.

**Step 2: Verify**

Run:
- `dotnet test MetalStorming20.Tests/MetalStorming20.Tests.csproj`
- `dotnet test MetalStorming20.PlaywrightTests/MetalStorming20.PlaywrightTests.csproj`
- `dotnet build MetalStorming20.sln`

Expected: all pass with exit code `0`.
