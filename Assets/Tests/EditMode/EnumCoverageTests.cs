using CriminalCase2.Data;
using NUnit.Framework;

namespace CriminalCase2.Tests
{
    public class EnumCoverageTests
    {
        [Test]
        public void SuspectRole_HasThreeValues()
        {
            var values = System.Enum.GetValues(typeof(SuspectRole));
            Assert.AreEqual(3, values.Length, "SuspectRole must have exactly 3 values.");
        }

        [Test]
        public void GameState_ContainsAllExpectedStates()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.IntroVideo));
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.Tutorial));
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.Investigation));
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.Verdict));
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.Results));
        }

        [Test]
        public void DrugTestResult_HasNegativeAndPositive()
        {
            Assert.AreEqual(0, (int)DrugTestResult.Negative);
            Assert.AreEqual(1, (int)DrugTestResult.Positive);
        }
    }
}
