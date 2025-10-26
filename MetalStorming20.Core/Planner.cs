namespace MetalStorming20.Core;

public static class Planner
{
    private static readonly int[] PartsPerUpgrade = {
        50, 95, 170, 325, 465, 610, 750, 900, 1050, 1185, 1325, 1460, 1600, 1725, 1850, 2000, 2150, 2300, 2450
    };
    private static readonly int[] SilverPerUpgrade = {
        150, 300, 600, 1200, 2000, 2600, 3275, 4000, 4500, 5200, 5875, 6500, 7200, 7800, 8500, 9100, 9800, 10400, 11000
    };

    private static readonly Dictionary<int, (int parts, int silver)> MasteryNonGold = new()
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

    private static readonly Dictionary<int, (int parts, int silver)> MasteryGoldBonus = new()
    {
        { 2,  (0, 1500) },
        { 6,  (1000, 0) },
        { 10, (0, 2500) },
        { 14, (0, 3500) },
        { 18, (2000, 0) },
        { 22, (0, 5000) },
    };

    public static (int needParts, int needSilver) NeedToLevel20(int currentPlaneLevel)
    {
        if (currentPlaneLevel >= 20) return (0, 0);
        int startIndex = Math.Max(0, currentPlaneLevel - 1);
        int parts = 0, silver = 0;
        for (int i = startIndex; i < PartsPerUpgrade.Length; i++)
        {
            parts += PartsPerUpgrade[i];
            silver += SilverPerUpgrade[i];
        }
        return (parts, silver);
    }

    public static (int parts, int silver) FutureMasteryRewards(int currentMastery, int targetMastery, bool includeGold)
    {
        if (targetMastery <= currentMastery) return (0, 0);
        int parts = 0, silver = 0;
        for (int m = currentMastery + 1; m <= targetMastery; m++)
        {
            if (MasteryNonGold.TryGetValue(m, out var baseReward))
            {
                parts += baseReward.parts;
                silver += baseReward.silver;
            }
            if (includeGold && MasteryGoldBonus.TryGetValue(m, out var gold))
            {
                parts += gold.parts;
                silver += gold.silver;
            }
        }
        return (parts, silver);
    }

    public static int Clamp(int v, int min, int max) => Math.Min(Math.Max(v, min), max);

    public static IReadOnlyList<(int fromLevel, int toLevel, int parts, int silver)> GetUpgradeSteps()
    {
        var steps = new List<(int fromLevel, int toLevel, int parts, int silver)>(PartsPerUpgrade.Length);
        for (int i = 0; i < PartsPerUpgrade.Length; i++)
        {
            steps.Add((i + 1, i + 2, PartsPerUpgrade[i], SilverPerUpgrade[i]));
        }
        return steps;
    }
}
