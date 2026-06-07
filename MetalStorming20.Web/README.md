MetalStorming20
================

Calculates Metalstorm Upgrades 2.0 needs. The app loads directly into the planner at `/`.

Prerequisites
-------------
- .NET 8 SDK

Web App (Blazor WebAssembly)
----------------------------
- Start the dev server:
  ```bash
  dotnet run --project MetalStorming20.Web/MetalStorming20.Web.csproj --urls http://localhost:5173
  ```
- Open `http://localhost:5173` in your browser.

Usage
-----
Planner at `/`:
- Enter current and target aircraft levels.
- Click square node toggles for every generic upgrade type.
- Use the built-in rows for Fuselage, Engines, Avionics, Cannons, Main/Radar Missile, Secondary/IR Missile, and Rockets.
- Levels `1-4` are single buttons; branch levels are stacked as `5A/5B`, `6A/6B`, `7A/7B`, and `8A/8B`.
- Each node cycles through off, desired, and has. Has is your current state; desired is the target state. Costs are calculated for the gap between current and target.
- Enter global balances for silver, aircraft parts, system parts, and advanced parts.
- Run the planner to see totals, deficits, purchase steps, warnings, filters, JSON export, markdown export, and a copyable share summary.

Static Catalogs
---------------
Upgrades 2.0 seed data is served as static JSON under `wwwroot/data/v2/`. This keeps GitHub Pages deployment compatible and avoids a mandatory backend. The current seed set includes currencies, aircraft milestones, system types, generic aircraft/system slots, generic branch labels, generic system node definitions, and all prompt-provided aircraft/system upgrade costs.

Local State
-----------
The Upgrades 2.0 generic planner form persists to browser `localStorage` with schema version `2`. This is client-only persistence; no profile data is sent to a server.

Testing
-------
Run tests:
```bash
dotnet test
```

CI
--
GitHub Actions workflow builds and tests on pushes/PRs to `main`.

License
-------
MIT


