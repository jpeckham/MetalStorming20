# Mastery Rebate Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add mastery rewards back to the Upgrades 2.0 planner as a calculated rebate against the grind still needed.

**Architecture:** `PlannerV2` owns the reward table and calculates mastery rebates from current/planned mastery plus one global gold mastery status. Results keep raw totals and add rebate-adjusted net grind so the full cost remains visible. The Blazor page exposes one 1-24 mastery level track plus one three-state Gold toggle, persists the selections, and includes the rebate in exports.

**Tech Stack:** C#/.NET 8, Blazor WebAssembly, xUnit, MSTest Playwright.

---

### Task 1: Core Rebate Model

**Files:**
- Modify: `MetalStorming20.Tests/PlannerV2Tests.cs`
- Modify: `MetalStorming20.Core/PlannerV2.cs`

**Steps:**
1. Add failing tests proving non-gold mastery from 1 to 23 rebates `1,900` aircraft parts and `4,400` silver.
2. Add a failing test proving the global Gold status adds gold bonuses on top of non-gold rewards.
3. Run `dotnet test MetalStorming20.Tests/MetalStorming20.Tests.csproj --filter Mastery` and verify the tests fail because the model is missing.
4. Add `MasteryPlanV2`, `GoldMasteryStatus`, `MasteryRebate`, and `NetGrindNeeded` to the planner request/result.
5. Implement the old reward tables in `PlannerV2` and subtract rebates from deficits without changing raw totals.
6. Run the same filtered tests and verify they pass.

### Task 2: UI Controls and State

**Files:**
- Modify: `MetalStorming20.PlaywrightTests/PlannerPageTests.cs`
- Modify: `MetalStorming20.Web/Components/Pages/Upgrades2.razor`

**Steps:**
1. Add a failing Playwright test that verifies the Mastery section renders one level track with levels 1-24 and one Gold toggle.
2. Add a failing Playwright test that selects planned mastery and planned Gold status and expects Mastery Rebate and Net Grind Needed to update.
3. Implement mastery level controls: current/planned levels use the existing has/desired color language, and the Gold toggle cycles off -> has -> desired.
4. Pass mastery state into `PlannerV2.Plan`.
5. Persist mastery state in local storage without breaking old schema reads.
6. Run the targeted Playwright tests and verify they pass.

### Task 3: Exports and Full Verification

**Files:**
- Modify: `MetalStorming20.Web/Components/Pages/Upgrades2.razor`

**Steps:**
1. Include mastery selections, rebate, and net grind in markdown and JSON exports.
2. Run `dotnet test`.
3. If Playwright needs a running app, start the dev server and run `dotnet test MetalStorming20.PlaywrightTests/MetalStorming20.PlaywrightTests.csproj`.
