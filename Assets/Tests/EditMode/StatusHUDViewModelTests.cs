#nullable enable
using System;
using CriminalCase2.Data;
using CriminalCase2.Domain;
using CriminalCase2.ViewModels;
using NUnit.Framework;

namespace CriminalCase2.Tests
{
    public class StatusHUDViewModelTests
    {
        private FakeLevelController _levels = null!;

        [SetUp]
        public void SetUp()
        {
            _levels = new FakeLevelController { JudgedCount = 0, TotalSuspects = 3 };
        }

        [Test]
        public void Ctor_NullLevels_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new StatusHUDViewModel(null!));
        }

        [Test]
        public void Ctor_ZeroTotal_ButtonTextIsEmpty()
        {
            _levels.TotalSuspects = 0;
            _levels.JudgedCount = 0;

            var vm = new StatusHUDViewModel(_levels);

            Assert.AreEqual(string.Empty, vm.ButtonText);
        }

        [Test]
        public void Ctor_NoJudgesYet_ShowsZeroFormat()
        {
            _levels.JudgedCount = 0;
            _levels.TotalSuspects = 3;

            var vm = new StatusHUDViewModel(_levels);

            Assert.AreEqual("Cek Status (0/3)", vm.ButtonText);
        }

        [Test]
        public void Ctor_PartiallyJudged_ShowsCekStatusFormat()
        {
            _levels.JudgedCount = 2;
            _levels.TotalSuspects = 5;

            var vm = new StatusHUDViewModel(_levels);

            Assert.AreEqual("Cek Status (2/5)", vm.ButtonText);
        }

        [Test]
        public void Ctor_AllJudged_ShowsLihatHasilFormat()
        {
            _levels.JudgedCount = 5;
            _levels.TotalSuspects = 5;

            var vm = new StatusHUDViewModel(_levels);

            Assert.AreEqual("Lihat Hasil (5/5)", vm.ButtonText);
        }

        [Test]
        public void Refresh_RecomputesButtonText_RaisesStateChanged()
        {
            _levels.JudgedCount = 0;
            _levels.TotalSuspects = 4;
            var vm = new StatusHUDViewModel(_levels);

            _levels.JudgedCount = 2;
            int calls = 0;
            vm.StateChanged += () => calls++;

            vm.Refresh();

            Assert.AreEqual("Cek Status (2/4)", vm.ButtonText);
            Assert.AreEqual(1, calls);
        }

        [Test]
        public void Refresh_TextUnchanged_DoesNotRaiseStateChanged()
        {
            _levels.JudgedCount = 1;
            _levels.TotalSuspects = 4;
            var vm = new StatusHUDViewModel(_levels);

            int calls = 0;
            vm.StateChanged += () => calls++;

            vm.Refresh();

            Assert.AreEqual(0, calls);
        }

        [Test]
        public void LevelLoaded_UpdatesButtonText()
        {
            _levels.JudgedCount = 0;
            _levels.TotalSuspects = 4;
            var vm = new StatusHUDViewModel(_levels);

            _levels.JudgedCount = 3;
            int calls = 0;
            vm.StateChanged += () => calls++;

            _levels.RaiseLevelLoaded(null!);

            Assert.AreEqual("Cek Status (3/4)", vm.ButtonText);
            Assert.AreEqual(1, calls);
        }

        [Test]
        public void RequestOpenCheckStatus_RaisesOpenCheckStatusRequested()
        {
            var vm = new StatusHUDViewModel(_levels);
            int calls = 0;
            vm.OpenCheckStatusRequested += () => calls++;

            vm.RequestOpenCheckStatus();

            Assert.AreEqual(1, calls);
        }

        [Test]
        public void Dispose_UnsubscribesFromLevelLoaded()
        {
            var vm = new StatusHUDViewModel(_levels);
            Assert.AreEqual(1, _levels.LevelLoadedSubscriberCount);

            vm.Dispose();

            Assert.AreEqual(0, _levels.LevelLoadedSubscriberCount);
        }

        [Test]
        public void Dispose_IsIdempotent()
        {
            var vm = new StatusHUDViewModel(_levels);

            Assert.DoesNotThrow(() => vm.Dispose());
            Assert.DoesNotThrow(() => vm.Dispose());
        }

        [Test]
        public void Refresh_AfterDispose_ThrowsObjectDisposedException()
        {
            var vm = new StatusHUDViewModel(_levels);
            vm.Dispose();

            Assert.Throws<ObjectDisposedException>(() => vm.Refresh());
        }

        [Test]
        public void RequestOpenCheckStatus_AfterDispose_ThrowsObjectDisposedException()
        {
            var vm = new StatusHUDViewModel(_levels);
            vm.Dispose();

            Assert.Throws<ObjectDisposedException>(() => vm.RequestOpenCheckStatus());
        }

        // ----- helpers -----

        private sealed class FakeLevelController : ILevelController
        {
            public int JudgedCount { get; set; }
            public int TotalSuspects { get; set; }

            public event Action<LevelConfig>? LevelLoaded;

            public int LevelLoadedSubscriberCount =>
                LevelLoaded?.GetInvocationList().Length ?? 0;

            public void RaiseLevelLoaded(LevelConfig config)
            {
                LevelLoaded?.Invoke(config);
            }

            public LevelConfig? CurrentLevelConfig => null;
            public int DrugTestsRemaining => 0;
            public bool AllSuspectsJudged => false;
            public bool IsSuspectJudged(SuspectData suspect) => false;
            public SuspectRole? GetSuspectVerdict(SuspectData suspect) => SuspectRole.Normal;
            public void RecordJudgedSuspect(SuspectData suspect, SuspectRole playerChoice) { }
            public bool UseDrugTest() => false;
            public void RecordDrugTest(SuspectData suspect, DrugTestResult result) { }
            public bool HasDrugTestResult(SuspectData suspect) => false;
            public DrugTestResult GetDrugTestResult(SuspectData suspect) => DrugTestResult.Negative;
            public void LoadLevel(LevelConfig config) { }
        }
    }
}
