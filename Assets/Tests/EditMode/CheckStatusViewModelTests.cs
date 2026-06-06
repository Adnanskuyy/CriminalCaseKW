#nullable enable
using System;
using System.Collections.Generic;
using CriminalCase2.Data;
using CriminalCase2.Domain;
using CriminalCase2.ViewModels;
using NUnit.Framework;
using UnityEngine;

namespace CriminalCase2.Tests
{
    [TestFixture]
    public class CheckStatusViewModelTests
    {
        private FakeLevelController _levels = null!;
        private CheckStatusViewModel _vm = null!;

        [SetUp]
        public void SetUp()
        {
            _levels = new FakeLevelController();
            _vm = new CheckStatusViewModel(_levels);
        }

        [TearDown]
        public void TearDown()
        {
            _vm.Dispose();
        }

        [Test]
        public void Ctor_NullLevels_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new CheckStatusViewModel(null!));
        }

        [Test]
        public void Ctor_InitialState_HasEmptyEntriesAndIsEmptyTrue()
        {
            Assert.That(_vm.Entries, Is.Empty);
            Assert.That(_vm.IsEmpty, Is.True);
        }

        [Test]
        public void SetRecords_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _vm.SetRecords(null!));
        }

        [Test]
        public void SetRecords_WithRecords_PopulatesEntriesWithNames()
        {
            var records = new List<VerdictRecord>
            {
                new VerdictRecord(CreateSuspect("Alice", SuspectRole.User), SuspectRole.User),
                new VerdictRecord(CreateSuspect("Bob", SuspectRole.Dealer), SuspectRole.Dealer),
            };

            _vm.SetRecords(records);

            Assert.That(_vm.Entries.Count, Is.EqualTo(2));
            Assert.That(_vm.Entries[0].SuspectName, Is.EqualTo("Alice"));
            Assert.That(_vm.Entries[0].PlayerVerdictDisplay, Is.EqualTo("Pecandu"));
            Assert.That(_vm.Entries[1].SuspectName, Is.EqualTo("Bob"));
            Assert.That(_vm.Entries[1].PlayerVerdictDisplay, Is.EqualTo("Bandar Narkoba"));
        }

        [Test]
        public void SetRecords_RaisesStateChanged()
        {
            var raised = 0;
            _vm.StateChanged += () => raised++;
            _vm.SetRecords(new List<VerdictRecord>());

            Assert.That(raised, Is.EqualTo(1));
        }

        [Test]
        public void IsEmpty_FalseWhenHasRecords()
        {
            _vm.SetRecords(new List<VerdictRecord>
            {
                new VerdictRecord(CreateSuspect("X", SuspectRole.Normal), SuspectRole.Normal),
            });

            Assert.That(_vm.IsEmpty, Is.False);
        }

        [Test]
        public void CanSubmit_TrueWhenAllSuspectsJudged()
        {
            _levels.JudgedCount = 3;
            _levels.TotalSuspects = 3;
            _levels.AllSuspectsJudgedValue = true;

            Assert.That(_vm.CanSubmit, Is.True);
        }

        [Test]
        public void CanSubmit_FalseWhenNotAllJudged()
        {
            _levels.JudgedCount = 1;
            _levels.TotalSuspects = 3;
            _levels.AllSuspectsJudgedValue = false;

            Assert.That(_vm.CanSubmit, Is.False);
        }

        [Test]
        public void SubmitButtonText_AllJudged_ShowsFinalText()
        {
            _levels.JudgedCount = 3;
            _levels.TotalSuspects = 3;
            _levels.AllSuspectsJudgedValue = true;

            Assert.That(_vm.SubmitButtonText, Is.EqualTo("Kirim Vonis Akhir"));
        }

        [Test]
        public void SubmitButtonText_NotAllJudged_ShowsRemainingText()
        {
            _levels.JudgedCount = 1;
            _levels.TotalSuspects = 3;
            _levels.AllSuspectsJudgedValue = false;

            Assert.That(_vm.SubmitButtonText, Is.EqualTo("Kirim Vonis Akhir (2 tersisa)"));
        }

        [Test]
        public void RequestClose_RaisesCloseRequested()
        {
            var raised = false;
            _vm.CloseRequested += () => raised = true;

            _vm.RequestClose();

            Assert.That(raised, Is.True);
        }

        [Test]
        public void RequestSubmit_WhenCanSubmit_RaisesSubmitRequested()
        {
            _levels.AllSuspectsJudgedValue = true;
            var raised = false;
            _vm.SubmitRequested += () => raised = true;

            _vm.RequestSubmit();

            Assert.That(raised, Is.True);
        }

        [Test]
        public void RequestSubmit_WhenCannotSubmit_DoesNotRaise()
        {
            _levels.AllSuspectsJudgedValue = false;
            var raised = false;
            _vm.SubmitRequested += () => raised = true;

            _vm.RequestSubmit();

            Assert.That(raised, Is.False);
        }

        [Test]
        public void LevelLoaded_RaisesStateChanged()
        {
            var raised = 0;
            _vm.StateChanged += () => raised++;

            _levels.RaiseLevelLoaded(null!);

            Assert.That(raised, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_Idempotent()
        {
            _vm.Dispose();
            Assert.DoesNotThrow(() => _vm.Dispose());
        }

        [Test]
        public void RequestClose_AfterDispose_Throws()
        {
            _vm.Dispose();
            Assert.Throws<ObjectDisposedException>(() => _vm.RequestClose());
        }

        [Test]
        public void RequestSubmit_AfterDispose_Throws()
        {
            _vm.Dispose();
            Assert.Throws<ObjectDisposedException>(() => _vm.RequestSubmit());
        }

        private static SuspectData CreateSuspect(string name, SuspectRole correctRole)
        {
            var so = ScriptableObject.CreateInstance<SuspectData>();
            var t = typeof(SuspectData);
            const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            t.GetField("_suspectName", flags)!.SetValue(so, name);
            t.GetField("_correctRole", flags)!.SetValue(so, correctRole);
            return so;
        }

        private sealed class FakeLevelController : ILevelController
        {
            public int JudgedCount { get; set; }
            public int TotalSuspects { get; set; }
            public bool AllSuspectsJudgedValue { get; set; }

            public LevelConfig? CurrentLevelConfig => null;
            public int DrugTestsRemaining => 0;
            public bool AllSuspectsJudged => AllSuspectsJudgedValue;

            public event Action<LevelConfig>? LevelLoaded;
            public void RaiseLevelLoaded(LevelConfig cfg) => LevelLoaded?.Invoke(cfg);

            public bool IsSuspectJudged(SuspectData suspect) => false;
            public SuspectRole GetSuspectVerdict(SuspectData suspect) => SuspectRole.Normal;
            public void RecordJudgedSuspect(SuspectData suspect, SuspectRole playerChoice) { }
            public bool UseDrugTest() => false;
            public void RecordDrugTest(SuspectData suspect, DrugTestResult result) { }
            public bool HasDrugTestResult(SuspectData suspect) => false;
            public DrugTestResult GetDrugTestResult(SuspectData suspect) => DrugTestResult.Negative;
            public void LoadLevel(LevelConfig config) { }
        }
    }
}
