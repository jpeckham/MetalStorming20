# Remove Export Share Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Remove all user-facing export and share functionality from the Upgrades 2.0 planner.

**Architecture:** The feature is local to the Blazor page. Remove the export/share buttons, output textarea, helper methods, and clipboard dependency from `Upgrades2.razor`, then update browser tests and docs so no public surface advertises the removed feature.

**Tech Stack:** .NET 8, Blazor WebAssembly, MSTest Playwright tests.

---

### Task 1: Playwright Expectations

**Files:**
- Modify: `MetalStorming20.PlaywrightTests/PlannerPageTests.cs`

**Step 1: Write the failing test**

Update the existing planner smoke tests so they assert:
- `Export JSON` is absent.
- `Export Markdown` is absent.
- `Copy Share Summary` is absent.
- `Export output` is absent.

**Step 2: Run test to verify it fails**

Run:
```bash
dotnet test MetalStorming20.PlaywrightTests/MetalStorming20.PlaywrightTests.csproj --filter "Upgrades2PageLoadsAndCalculates|Upgrades2GenericSystemRowsShowAllUpgradeTypesAndCopyShareSummary"
```

Expected: failure because the buttons still exist.

### Task 2: Blazor Removal

**Files:**
- Modify: `MetalStorming20.Web/Components/Pages/Upgrades2.razor`

**Step 1: Remove implementation**

Remove:
- `@using System.Text.Json` only if no longer needed.
- `@inject IJSRuntime JS` only if no longer needed.
- Lead text phrase that mentions shareable exports.
- Export/share button group.
- Export output textarea.
- `exportText`.
- Export JSON, markdown, share, and markdown summary methods.
- Resetting `exportText` during recalculation.

Keep localStorage behavior intact.

**Step 2: Run test to verify it passes**

Run the same Playwright filtered test command from Task 1.

### Task 3: Documentation And Full Verification

**Files:**
- Modify: `README.md`
- Modify: `MetalStorming20.Web/README.md`

**Step 1: Remove docs references**

Remove claims that the planner supports exports, markdown, JSON exports, or copyable share summaries.

**Step 2: Run verification**

Run:
```bash
dotnet test
```

Expected: all tests pass.
