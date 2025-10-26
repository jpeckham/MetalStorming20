namespace MetalStorming20.Tests;

public class CalculationTests
{
    [Fact]
    public void NeedToLevel20_AtLevel20_ReturnsZero()
    {
        var (parts, silver) = Program.NeedToLevel20(20);
        Assert.Equal(0, parts);
        Assert.Equal(0, silver);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void NeedToLevel20_MonotonicDecrease_AsLevelIncreases(int level)
    {
        var (aParts, aSilver) = Program.NeedToLevel20(level);
        var (bParts, bSilver) = Program.NeedToLevel20(Math.Min(level + 1, 20));
        Assert.True(bParts <= aParts);
        Assert.True(bSilver <= aSilver);
    }

    [Fact]
    public void FutureMasteryRewards_NoGold_MatchesNonGoldTable()
    {
        var (parts, silver) = Program.FutureMasteryRewards(0, 1, includeGold: false);
        Assert.Equal(100, parts);
        Assert.Equal(0, silver);
    }

    [Fact]
    public void FutureMasteryRewards_WithGold_IncludesGoldBonuses()
    {
        var (partsNoGold, silverNoGold) = Program.FutureMasteryRewards(1, 2, includeGold: false);
        var (partsWithGold, silverWithGold) = Program.FutureMasteryRewards(1, 2, includeGold: true);
        Assert.True(partsWithGold >= partsNoGold);
        Assert.True(silverWithGold >= silverNoGold);
    }

    [Fact]
    public void Clamp_Works()
    {
        Assert.Equal(5, Program.Clamp(5, 1, 10));
        Assert.Equal(1, Program.Clamp(-10, 1, 10));
        Assert.Equal(10, Program.Clamp(50, 1, 10));
    }
}
