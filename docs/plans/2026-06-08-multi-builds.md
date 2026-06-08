# Multi-Build Saved Planner Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Replace the single saved Upgrades 2.0 planner build with unlimited named builds that can be created, selected, renamed, and deleted.

**Architecture:** Keep `Upgrades2PlannerState` as the state for one build. Add a collection layer with selected build metadata, extend the localStorage gateway to read and write the collection, and have the session interactor update only the selected build after planner changes.

**Tech Stack:** .NET, C#, xUnit, Blazor, JS interop localStorage, Playwright tests where appropriate.

---

### Task 1: Core Saved-Build Models And Gateway Contract

**Files:**
- Modify: `MetalStorming20.Core/Upgrades2PlannerStateUseCases.cs`
- Test: `MetalStorming20.Tests/Upgrades2PlannerStateUseCaseTests.cs`

**Step 1: Write failing tests**

Add tests for saving and loading a build collection through a gateway:

```csharp
[Fact]
public async Task SaveBuildCollectionAsync_WritesCollectionThroughGateway()
{
    var gateway = new RecordingPlannerStateGateway();
    var useCase = new SaveUpgrades2SavedBuildCollectionUseCase(gateway);
    var collection = ExampleBuildCollection();

    await useCase.HandleAsync(collection);

    Assert.Same(collection, gateway.SavedBuildCollections.Single());
}
```

Also add a corresponding load/present test.

**Step 2: Run test to verify it fails**

Run: `dotnet test MetalStorming20.Tests/MetalStorming20.Tests.csproj --filter Upgrades2PlannerStateUseCaseTests`

Expected: FAIL because saved-build collection types and methods do not exist.

**Step 3: Implement minimal contract**

Add records:

```csharp
public sealed record Upgrades2SavedBuild(string Id, string Name, Upgrades2PlannerState State);
public sealed record Upgrades2SavedBuildCollection(string? SelectedBuildId, IReadOnlyList<Upgrades2SavedBuild>? Builds);
public sealed record Upgrades2SavedBuildCollectionResponse(Upgrades2SavedBuildCollection? Collection);
```

Extend `IUpgrades2PlannerStateGateway` with `LoadBuildCollectionAsync` and `SaveBuildCollectionAsync`.

Add load/save use cases and presenter interface for the collection.

**Step 4: Run test to verify it passes**

Run: `dotnet test MetalStorming20.Tests/MetalStorming20.Tests.csproj --filter Upgrades2PlannerStateUseCaseTests`

Expected: PASS.

### Task 2: LocalStorage Collection Persistence And Migration

**Files:**
- Modify: `MetalStorming20.Web/Services/Upgrades2LocalStoragePlannerStateGateway.cs`
- Test: existing web tests if available, otherwise add focused Core tests for migration helper extracted into Core.

**Step 1: Write failing test**

Add a test that proves an old single `Upgrades2PlannerState` becomes a collection with one selected build named `unnamed`.

**Step 2: Run test to verify it fails**

Run relevant test command.

Expected: FAIL because migration is not implemented.

**Step 3: Implement minimal migration**

Use a new storage key `metalstorming20.upgrades2.builds`. When loading the collection:

1. Read new key.
2. If present, deserialize collection.
3. If absent, call existing `LoadAsync`.
4. If old state exists, return a collection with generated id, `Name = "unnamed"`, selected id matching the generated id.

Keep `LoadAsync` and `SaveAsync` for compatibility until all call sites are moved.

**Step 4: Run test to verify it passes**

Run relevant test command.

Expected: PASS.

### Task 3: Session Interactor Build Collection Operations

**Files:**
- Modify: `MetalStorming20.Core/Upgrades2PlannerSessionInteractor.cs`
- Modify: `MetalStorming20.Core/Upgrades2PlannerSession.cs`
- Test: `MetalStorming20.Tests/Upgrades2PlannerSessionInteractorTests.cs`
- Test: `MetalStorming20.Tests/Upgrades2PlannerSessionTests.cs`

**Step 1: Write failing tests**

Cover:

- Loading builds selects active build and loads its state.
- Cycling a planner node saves changes into only the selected build.
- Creating new build adds blank selected build.
- Renaming selected build normalizes blank names to `unnamed`.
- Deleting selected build picks the next build.
- Deleting the final build creates a blank `unnamed` build.

**Step 2: Run test to verify it fails**

Run: `dotnet test MetalStorming20.Tests/MetalStorming20.Tests.csproj --filter Upgrades2PlannerSessionInteractorTests`

Expected: FAIL because operations do not exist.

**Step 3: Implement minimal interactor behavior**

Add collection state to `Upgrades2PlannerSession`:

- `SavedBuilds`
- `SelectedBuildId`
- helper methods for loading collection, selecting, renaming, creating, deleting, and saving current state into selected build.

Change interactor persistence from saving a single state to saving the build collection.

**Step 4: Run test to verify it passes**

Run: `dotnet test MetalStorming20.Tests/MetalStorming20.Tests.csproj --filter Upgrades2PlannerSessionInteractorTests`

Expected: PASS.

### Task 4: Web Controller And Toolbar UI

**Files:**
- Modify: `MetalStorming20.Web/Controllers/Upgrades2Controller.cs`
- Modify: `MetalStorming20.Web/Components/Pages/Upgrades2.razor`
- Modify: `MetalStorming20.Web/Presenters/Upgrades2PlannerSessionPresenter.cs`
- Test: `MetalStorming20.Tests/Upgrades2PlannerSessionViewUseCaseTests.cs`
- Test: `MetalStorming20.PlaywrightTests/PlannerPageTests.cs`

**Step 1: Write failing tests**

Add view-model assertions for build dropdown options and selected build name. Add Playwright coverage for visible build controls and delete confirmation if practical.

**Step 2: Run test to verify it fails**

Run: `dotnet test MetalStorming20.Tests/MetalStorming20.Tests.csproj --filter Upgrades2PlannerSessionViewUseCaseTests`

Expected: FAIL because view model fields do not exist.

**Step 3: Implement UI wiring**

Add controller methods:

- `SelectBuildAsync`
- `RenameSelectedBuildAsync`
- `CreateNewBuildAsync`
- `DeleteSelectedBuildAsync`

Update the page toolbar:

- `select.form-select` bound to selected build id
- text input for name
- `New Build` button
- `Delete Build` button guarded by `window.confirm`

**Step 4: Run test to verify it passes**

Run targeted tests.

Expected: PASS.

### Task 5: Full Verification

**Files:**
- All touched files

**Step 1: Run full unit suite**

Run: `dotnet test MetalStorming20.Tests/MetalStorming20.Tests.csproj`

Expected: PASS.

**Step 2: Run Playwright suite**

Run: `dotnet test MetalStorming20.PlaywrightTests/MetalStorming20.PlaywrightTests.csproj`

Expected: PASS or document environment blocker.

**Step 3: Inspect git diff**

Run: `git diff --check` and `git status --short`

Expected: no whitespace errors; only intended files changed.
