MetalStorming20
================

Calculates how much you need to reach plane level 20 in Metalstorm.

Prerequisites
-------------
- .NET 8 SDK

Build and Run
-------------
1. Build:
   ```bash
   dotnet build
   ```
2. Run:
   ```bash
   dotnet run --project MetalStorming20.csproj
   ```

Usage
-----
Follow the prompts:
- Current Plane Level (1-20)
- Current Mastery Level (1-23)
- Current Universal Parts
- Current Silver
- Target Mastery Level (press Enter for 23)

Optionally, choose to print per-upgrade breakdown.

Testing
-------
Run tests:
```bash
dotnet test
```

CI
--
This repo includes a GitHub Actions workflow that builds and runs tests on pushes and PRs to `main`.

License
-------
MIT


