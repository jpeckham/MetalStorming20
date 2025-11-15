using MetalStorming20.Core;

namespace MetalStorming20.Tests
{
    public class PlannerTests
    {
        #region NeedToLevel20 Tests

        [Fact]
        public void NeedToLevel20_FromLevel1_ReturnsFullTotal()
        {
            // Arrange & Act
            var (parts, silver) = Planner.NeedToLevel20(1);

            // Assert - Sum of all 19 upgrades (1->2 through 19->20)
            // The function starts from currentPlaneLevel-1, so level 1 means index 0 (level 1->2)
            // Total of all 19 array elements
            Assert.Equal(22460, parts);  // Sum of all parts: 50+95+...+2450 = 22460
            Assert.Equal(100000, silver); // Sum of all silver: 150+300+...+11000 = 100000
        }

        [Fact]
        public void NeedToLevel20_FromLevel19_ReturnsLastUpgrade()
        {
            // Arrange & Act
            var (parts, silver) = Planner.NeedToLevel20(19);

            // Assert - Only 19->20 upgrade
            Assert.Equal(2450, parts);
            Assert.Equal(11000, silver);
        }

        [Fact]
        public void NeedToLevel20_FromLevel20_ReturnsZero()
        {
            // Arrange & Act
            var (parts, silver) = Planner.NeedToLevel20(20);

            // Assert
            Assert.Equal(0, parts);
            Assert.Equal(0, silver);
        }

        [Fact]
        public void NeedToLevel20_FromLevel10_ReturnsCorrectPartialSum()
        {
            // Arrange & Act
            var (parts, silver) = Planner.NeedToLevel20(10);

            // Assert - Sum from level 10->11 through 19->20 (10 upgrades)
            // Parts: 1185+1325+1460+1600+1725+1850+2000+2150+2300+2450 = 18045
            // Silver: 5200+5875+6500+7200+7800+8500+9100+9800+10400+11000 = 81375
            Assert.Equal(18045, parts);
            Assert.Equal(81375, silver);
        }

        [Fact]
        public void NeedToLevel20_FromAbove20_ReturnsZero()
        {
            // Arrange & Act
            var (parts, silver) = Planner.NeedToLevel20(25);

            // Assert
            Assert.Equal(0, parts);
            Assert.Equal(0, silver);
        }

        #endregion

        #region FutureMasteryRewards Tests

        [Fact]
        public void FutureMasteryRewards_NoGold_Level1To5()
        {
            // Arrange & Act
            var (parts, silver) = Planner.FutureMasteryRewards(1, 5, includeGold: false);

            // Assert
            // M2: none, M3: (150,0), M4: none, M5: (0,700)
            Assert.Equal(150, parts);
            Assert.Equal(700, silver);
        }

        [Fact]
        public void FutureMasteryRewards_WithGold_Level1To5()
        {
            // Arrange & Act
            var (parts, silver) = Planner.FutureMasteryRewards(1, 5, includeGold: true);

            // Assert
            // M2: (0,1500) gold, M3: (150,0) base, M4: none, M5: (0,700) base
            Assert.Equal(150, parts);
            Assert.Equal(2200, silver); // 1500 + 700
        }

        [Fact]
        public void FutureMasteryRewards_NoGold_Level1To23()
        {
            // Arrange & Act
            var (parts, silver) = Planner.FutureMasteryRewards(1, 23, includeGold: false);

            // Assert
            // From currentMastery+1 (M2) to targetMastery (M23)
            // M3: (150,0), M5: (0,700), M7: (0,900), M11: (250,0), M13: (300,0),
            // M15: (350,0), M17: (0,1200), M19: (0,1600), M21: (400,0), M23: (450,0)
            // Parts: 150+250+300+350+400+450 = 1900
            // Silver: 700+900+1200+1600 = 4400
            Assert.Equal(1900, parts);
            Assert.Equal(4400, silver);
        }

        [Fact]
        public void FutureMasteryRewards_WithGold_Level1To23()
        {
            // Arrange & Act
            var (parts, silver) = Planner.FutureMasteryRewards(1, 23, includeGold: true);

            // Assert
            // Non-Gold: 1900 parts, 4400 silver (M3,M5,M7,M11,M13,M15,M17,M19,M21,M23)
            // Gold bonuses: M2: (0,1500), M6: (1000,0), M10: (0,2500), M14: (0,3500), M18: (2000,0), M22: (0,5000)
            // Gold parts: 1000+2000 = 3000
            // Gold silver: 1500+2500+3500+5000 = 12500
            Assert.Equal(4900, parts);   // 1900 + 3000
            Assert.Equal(16900, silver); // 4400 + 12500
        }

        [Fact]
        public void FutureMasteryRewards_SameLevel_ReturnsZero()
        {
            // Arrange & Act
            var (parts, silver) = Planner.FutureMasteryRewards(10, 10, includeGold: true);

            // Assert
            Assert.Equal(0, parts);
            Assert.Equal(0, silver);
        }

        [Fact]
        public void FutureMasteryRewards_CurrentHigherThanTarget_ReturnsZero()
        {
            // Arrange & Act
            var (parts, silver) = Planner.FutureMasteryRewards(15, 10, includeGold: true);

            // Assert
            Assert.Equal(0, parts);
            Assert.Equal(0, silver);
        }

        [Fact]
        public void FutureMasteryRewards_NoGold_OnlyGoldLevels_ReturnsZero()
        {
            // Arrange & Act - Only M2, M6 exist which are gold-only
            var (parts, silver) = Planner.FutureMasteryRewards(1, 6, includeGold: false);

            // Assert - Should only count M3 and M5 (non-gold)
            Assert.Equal(150, parts);  // M3
            Assert.Equal(700, silver); // M5
        }

        #endregion

        #region Clamp Tests

        [Fact]
        public void Clamp_ValueWithinRange_ReturnsValue()
        {
            // Arrange & Act
            var result = Planner.Clamp(5, 1, 10);

            // Assert
            Assert.Equal(5, result);
        }

        [Fact]
        public void Clamp_ValueBelowMin_ReturnsMin()
        {
            // Arrange & Act
            var result = Planner.Clamp(-5, 1, 10);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void Clamp_ValueAboveMax_ReturnsMax()
        {
            // Arrange & Act
            var result = Planner.Clamp(15, 1, 10);

            // Assert
            Assert.Equal(10, result);
        }

        [Fact]
        public void Clamp_ValueEqualsMin_ReturnsMin()
        {
            // Arrange & Act
            var result = Planner.Clamp(1, 1, 10);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void Clamp_ValueEqualsMax_ReturnsMax()
        {
            // Arrange & Act
            var result = Planner.Clamp(10, 1, 10);

            // Assert
            Assert.Equal(10, result);
        }

        #endregion

        #region GetUpgradeSteps Tests

        [Fact]
        public void GetUpgradeSteps_ReturnsCorrectNumberOfSteps()
        {
            // Arrange & Act
            var steps = Planner.GetUpgradeSteps();

            // Assert - Should have 19 steps (1->2 through 19->20)
            Assert.Equal(19, steps.Count);
        }

        [Fact]
        public void GetUpgradeSteps_FirstStep_IsCorrect()
        {
            // Arrange & Act
            var steps = Planner.GetUpgradeSteps();

            // Assert - First upgrade: 1->2 costs 50 parts, 150 silver
            Assert.Equal(1, steps[0].fromLevel);
            Assert.Equal(2, steps[0].toLevel);
            Assert.Equal(50, steps[0].parts);
            Assert.Equal(150, steps[0].silver);
        }

        [Fact]
        public void GetUpgradeSteps_LastStep_IsCorrect()
        {
            // Arrange & Act
            var steps = Planner.GetUpgradeSteps();

            // Assert - Last upgrade: 19->20 costs 2450 parts, 11000 silver
            var lastStep = steps[^1];
            Assert.Equal(19, lastStep.fromLevel);
            Assert.Equal(20, lastStep.toLevel);
            Assert.Equal(2450, lastStep.parts);
            Assert.Equal(11000, lastStep.silver);
        }

        [Fact]
        public void GetUpgradeSteps_MiddleStep_IsCorrect()
        {
            // Arrange & Act
            var steps = Planner.GetUpgradeSteps();

            // Assert - 10th upgrade: 10->11 costs 1185 parts, 5200 silver
            var step10 = steps[9]; // 0-indexed
            Assert.Equal(10, step10.fromLevel);
            Assert.Equal(11, step10.toLevel);
            Assert.Equal(1185, step10.parts);
            Assert.Equal(5200, step10.silver);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void IntegrationTest_Level10Mastery5ToLevel20Mastery23_NoGold()
        {
            // Arrange
            int currentLevel = 10;
            int currentMastery = 5;
            int targetMastery = 23;
            int currentParts = 1000;
            int currentSilver = 5000;

            // Act
            var (needParts, needSilver) = Planner.NeedToLevel20(currentLevel);
            var shortPartsAfterBank = Math.Max(0, needParts - currentParts);
            var shortSilverAfterBank = Math.Max(0, needSilver - currentSilver);
            var (masteryParts, masterySilver) = Planner.FutureMasteryRewards(currentMastery, targetMastery, includeGold: false);
            var finalShortParts = Math.Max(0, shortPartsAfterBank - masteryParts);
            var finalShortSilver = Math.Max(0, shortSilverAfterBank - masterySilver);

            // Assert
            Assert.Equal(18045, needParts);      // From level 10 to 20
            Assert.Equal(81375, needSilver);
            Assert.Equal(17045, shortPartsAfterBank);  // 18045 - 1000
            Assert.Equal(76375, shortSilverAfterBank); // 81375 - 5000
            // Mastery rewards M6-M23 without gold: M7:(0,900), M11:(250,0), M13:(300,0), M15:(350,0), M17:(0,1200), M19:(0,1600), M21:(400,0), M23:(450,0)
            // Parts: 250+300+350+400+450 = 1750, Silver: 900+1200+1600 = 3700
            Assert.Equal(1750, masteryParts);
            Assert.Equal(3700, masterySilver);
            Assert.Equal(15295, finalShortParts);  // 17045 - 1750
            Assert.Equal(72675, finalShortSilver); // 76375 - 3700
        }

        [Fact]
        public void IntegrationTest_AlreadyAtLevel20_ReturnsZero()
        {
            // Arrange & Act
            var (parts, silver) = Planner.NeedToLevel20(20);

            // Assert
            Assert.Equal(0, parts);
            Assert.Equal(0, silver);
        }

        #endregion

        #region Edge Case Tests

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void NeedToLevel20_BelowLevel1_HandlesGracefully(int level)
        {
            // Arrange & Act
            var (parts, silver) = Planner.NeedToLevel20(level);

            // Assert - Math.Max(0, currentPlaneLevel - 1) ensures it starts from index 0
            // When level <= 0, startIndex = Math.Max(0, -1 or less) = 0, which sums all arrays
            Assert.Equal(22460, parts);
            Assert.Equal(100000, silver);
        }

        [Fact]
        public void FutureMasteryRewards_Level0To1_IncludesMastery1()
        {
            // Arrange & Act
            var (parts, silver) = Planner.FutureMasteryRewards(0, 1, includeGold: false);

            // Assert - M1 gives (100,0)
            Assert.Equal(100, parts);
            Assert.Equal(0, silver);
        }

        [Fact]
        public void GetUpgradeSteps_IsReadOnly()
        {
            // Arrange & Act
            var steps = Planner.GetUpgradeSteps();

            // Assert - Verify it's a read-only list
            Assert.IsAssignableFrom<IReadOnlyList<(int, int, int, int)>>(steps);
        }

        #endregion
    }
}