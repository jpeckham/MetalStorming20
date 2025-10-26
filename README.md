MetalStorming20
================

Calculates how much you need to reach plane level 20 in Metalstorm.

Prerequisites
-------------
- .NET 8 SDK

Build and Run (Console)
-----------------------
1. Build:
   ```bash
   dotnet build
   ```
2. Run:
   ```bash
   dotnet run --project MetalStorming20.csproj
   ```

Web App (Blazor WebAssembly)
----------------------------
- Start the dev server:
  ```bash
  dotnet run --project MetalStorming20.Web/MetalStorming20.Web.csproj --urls http://localhost:5173
  ```
- Open `http://localhost:5173` in your browser.

Usage
-----
Enter:
- Current Plane Level (1-20)
- Current Mastery Level (1-23)
- Current Universal Parts
- Current Silver
- Target Mastery Level (default 23)

Optionally, expand the per-upgrade breakdown.

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


