# Multi-Build Saved Planner Design

## Goal

Let users keep any number of named Upgrades 2.0 planner builds, switch between them, create new builds, rename builds, and delete the current build with confirmation.

## Current State

The app stores one `Upgrades2PlannerState` in localStorage under `metalstorming20.upgrades2.state`. Every planner interaction rebuilds the current state and overwrites that single value. The planner session model already captures one build cleanly, so the multi-build feature should wrap the existing state rather than change the calculation model.

## Recommended Approach

Add a saved-build collection around the existing `Upgrades2PlannerState`.

Each saved build contains:

- Stable `Id`
- Editable `Name`
- Existing `Upgrades2PlannerState`

The collection contains:

- `SelectedBuildId`
- `Builds`

The new localStorage key should store this collection. On first load, if no collection exists but the old single-state key exists, migrate that old state into the collection as one selected build named `unnamed`.

## UI Behavior

The page toolbar should expose:

- A dropdown for saved builds
- A text input for the selected build name
- `New Build`
- `Delete Build`

`New Build` replaces the old `Start New Build` behavior. It creates a blank separate build, selects it, and future edits save into that selected build.

`Delete Build` asks for confirmation before deleting the selected build. After deletion, the app selects the next build if available, otherwise the previous build. If there are no builds left, it creates a blank selected build named `unnamed`, so the planner always has an active build.

## Data Flow

Initialization should still load catalog data and render an empty planner session first. After localStorage is available, the controller loads or creates the saved-build collection, selects the active build, loads that build's state into the existing `Upgrades2PlannerSession`, and presents the planner/session view models.

Every planner interaction should update only the active build's `State` inside the saved-build collection, then persist the collection.

Rename, select, new, and delete operations should also save the collection and present an updated build list view model for the toolbar.

## Error Handling

Invalid or missing selected build ids should fall back to the first available build. Empty collections should create a blank `unnamed` build. Blank rename input should normalize back to `unnamed`.

Malformed or incompatible stored JSON can be treated as no saved collection, matching the current lightweight localStorage behavior.

## Testing

Core tests should cover:

- Old single-state migration into a selected `unnamed` build
- Creating a new blank build and selecting it
- Saving planner changes into only the selected build
- Switching selected builds loads that build's state
- Renaming the selected build
- Deleting the selected build and moving to the next available build
- Deleting the last build creates a blank `unnamed` build

Web tests should cover:

- LocalStorage gateway reads the new collection key
- LocalStorage gateway migrates the old single-state key
- UI delete confirmation path where practical
