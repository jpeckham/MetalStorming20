using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MetalStorming20.Core;

class Program
{
    // Verified plane upgrade requirements (per step) 1->2 .. 19->20
    static readonly int[] PartsPerUpgrade = {
        50, 95, 170, 325, 465, 610, 750, 900, 1050, 1185, 1325, 1460, 1600, 1725, 1850, 2000, 2150, 2300, 2450
    };
    static readonly int[] SilverPerUpgrade = {
        150, 300, 600, 1200, 2000, 2600, 3275, 4000, 4500, 5200, 5875, 6500, 7200, 7800, 8500, 9100, 9800, 10400, 11000
    };

    // Mastery rewards (Non-Gold and Gold bonuses), corrected per your notes
    // Non-Gold rewards
    static readonly Dictionary<int, (int parts, int silver)> MasteryNonGold = new()
    {
        { 1,  (100, 0) },
        { 3,  (150, 0) },
        { 5,  (0, 700) },
        { 7,  (0, 900) },
        { 11, (250, 0) },
        { 13, (300, 0) },
        { 15, (350, 0) },
        { 17, (0, 1200) },
        { 19, (0, 1600) },
        { 21, (400, 0) },
        { 23, (450, 0) },
    };

    // Gold-only bonuses
    static readonly Dictionary<int, (int parts, int silver)> MasteryGoldBonus = new()
    {
        { 2,  (0, 1500) },
        { 6,  (1000, 0) },
        { 10, (0, 2500) },
        { 14, (0, 3500) },
        { 18, (2000, 0) },
        { 22, (0, 5000) },
    };

    static void Main()
    {
        Console.WriteLine("=== Metalstorm Planner: Remaining to Plane Level 20 ===");
        int currentPlaneLevel = ReadInt("Current Plane Level (1-20): ", min: 1, max: 20);
        int currentMasteryLevel = ReadInt("Current Mastery Level (1-23): ", min: 1, max: 23);
        int currentParts = ReadInt("Current Universal Parts: ", min: 0);
        int currentSilver = ReadInt("Current Silver: ", min: 0);
        Console.Write("Target Mastery Level (default 23): ");
        var targetStr = Console.ReadLine();
        int targetMasteryLevel = string.IsNullOrWhiteSpace(targetStr) ? 23 : Clamp(ParseIntOrDefault(targetStr, 23), 1, 23);

        // 1) Compute total need from current plane level -> 20
        var (needParts, needSilver) = Planner.NeedToLevel20(currentPlaneLevel);

        // 2) Subtract banked resources
        int shortPartsAfterBank = Math.Max(0, needParts - currentParts);
        int shortSilverAfterBank = Math.Max(0, needSilver - currentSilver);

        // 3) Project future mastery rewards from (currentMasteryLevel+1) to target
        var (nonGoldParts, nonGoldSilver) = Planner.FutureMasteryRewards(currentMasteryLevel, targetMasteryLevel, includeGold: false);
        var (goldParts, goldSilver) = Planner.FutureMasteryRewards(currentMasteryLevel, targetMasteryLevel, includeGold: true);

        // 4) Shortfall after applying future mastery (No-Gold / With-Gold)
        var shortNoGoldParts = Math.Max(0, shortPartsAfterBank - nonGoldParts);
        var shortNoGoldSilver = Math.Max(0, shortSilverAfterBank - nonGoldSilver);

        var shortGoldParts = Math.Max(0, shortPartsAfterBank - goldParts);
        var shortGoldSilver = Math.Max(0, shortSilverAfterBank - goldSilver);

        // Output
        Console.WriteLine();
        Console.WriteLine("--- Requirements from current -> 20 ---");
        Console.WriteLine($"Raw Need:       Parts {needParts:N0}, Silver {needSilver:N0}");
        Console.WriteLine($"After Bank:     Parts {shortPartsAfterBank:N0}, Silver {shortSilverAfterBank:N0}");
        Console.WriteLine();

        Console.WriteLine($"--- Future Mastery Rewards (from M{currentMasteryLevel + 1} -> M{targetMasteryLevel}) ---");
        Console.WriteLine($"No-Gold:        Parts +{nonGoldParts:N0}, Silver +{nonGoldSilver:N0}");
        Console.WriteLine($"With Gold:      Parts +{goldParts:N0}, Silver +{goldSilver:N0}");
        Console.WriteLine();

        Console.WriteLine("--- Remaining Needed from NON-MASTERY sources ---");
        Console.WriteLine($"No-Gold path:   Parts {shortNoGoldParts:N0}, Silver {shortNoGoldSilver:N0}");
        Console.WriteLine($"With Gold path: Parts {shortGoldParts:N0}, Silver {shortGoldSilver:N0}");
        Console.WriteLine();

        // Nice per-upgrade breakdown (optional)
        Console.Write("Show per-upgrade breakdown (y/N)? ");
        var ans = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
        if (ans == "y" || ans == "yes")
        {
            PrintPerUpgrade(currentPlaneLevel);
        }
    }

    /// <summary>
    /// Calculates the total universal parts and silver required to upgrade
    /// from the provided plane level up to level 20 (exclusive of current level).
    /// Returns a tuple of (parts, silver).
    /// </summary>
    /// <param name="currentPlaneLevel">The current plane level (1-20).</param>
    /// <returns>Tuple where Item1 is required parts and Item2 is required silver.</returns>
    

    /// <summary>
    /// Computes the cumulative mastery rewards (parts and silver) obtained by
    /// progressing from the current mastery level up to the target mastery level.
    /// Gold-only bonuses are optionally included when <paramref name="includeGold"/> is true.
    /// </summary>
    /// <param name="currentMastery">Current mastery level (1-23).</param>
    /// <param name="targetMastery">Target mastery level (1-23).</param>
    /// <param name="includeGold">Whether to include gold-only mastery bonuses.</param>
    /// <returns>Tuple of (parts, silver) earned from mastery progression.</returns>
    

    /// <summary>
    /// Prints a per-upgrade breakdown of required parts and silver for each step
    /// from the current level up to level 20.
    /// </summary>
    /// <param name="currentPlaneLevel">The current plane level.</param>
    static void PrintPerUpgrade(int currentPlaneLevel)
    {
        Console.WriteLine();
        Console.WriteLine("Upgrade | Parts  | Silver");
        Console.WriteLine("--------------------------");
        for (int lvl = currentPlaneLevel; lvl < 20; lvl++)
        {
            int idx = lvl - 1;
            Console.WriteLine(
                $"{lvl,2} -> {lvl + 1,2} | {PartsPerUpgrade[idx],5:N0} | {SilverPerUpgrade[idx],6:N0}");
        }
    }

    // Helpers
    /// <summary>
    /// Reads an integer from the console with validation and bounds checking.
    /// Re-prompts until a valid integer within [min, max] is provided.
    /// </summary>
    static int ReadInt(string prompt, int min = int.MinValue, int max = int.MaxValue)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = Console.ReadLine();
            if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) && v >= min && v <= max)
                return v;
            Console.WriteLine($"Please enter a valid integer between {min} and {max}.");
        }
    }

    /// <summary>
    /// Parses an integer using invariant culture; returns <paramref name="def"/> if parsing fails.
    /// </summary>
    static int ParseIntOrDefault(string? s, int def)
    {
        return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : def;
    }

    /// <summary>
    /// Clamps integer <paramref name="v"/> into the inclusive range [min, max].
    /// </summary>
    internal static int Clamp(int v, int min, int max) => Math.Min(Math.Max(v, min), max);
}
