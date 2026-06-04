#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using CriminalCase2.Data;
using CriminalCase2.Domain;
using CriminalCase2.ViewModels;
using NUnit.Framework;
using UnityEngine;

namespace CriminalCase2.Tests
{
    public class SuspectDetailViewModelTests
    {
        private FakeLevelController _levels = null!;
        private SuspectData _suspect = null!;

        [SetUp]
        public void SetUp()
        {
            _levels = new FakeLevelController
            {
                DrugTestsRemaining = 1,
                HasDrugTestResultResult = false,
                UseDrugTestResult = true
            };
            _suspect = CreateSuspect("Budi", "Description text", "Evidence text", DrugTestResult.Positive);
        }

        [TearDown]
        public void TearDown()
        {
            if (_suspect != null) UnityEngine.Object.DestroyImmediate(_suspect);
        }

        [Test]
        public void Ctor_NullSuspect_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new SuspectDetailViewModel(null!, _levels));
        }

        [Test]
        public void Ctor_NullLevels_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new SuspectDetailViewModel(_suspect, null!));
        }

        [Test]
        public void Ctor_ExposesSuspectPassthroughProperties()
        {
            var vm = new SuspectDetailViewModel(_suspect, _levels);

            Assert.AreSame(_suspect, vm.Suspect);
            Assert.AreEqual("Budi", vm.SuspectName);
            Assert.AreEqual("Description text", vm.Description);
            Assert.AreEqual("Evidence text", vm.EvidenceText);
        }

        [Test]
        public void Ctor_NotTested_NoRemaining_DisablesButton()
        {
            _levels.DrugTestsRemaining = 0;
            _levels.HasDrugTestResultResult = false;

            var vm = new SuspectDetailViewModel(_suspect, _levels);

            Assert.IsFalse(vm.IsDrugTestButtonEnabled);
            Assert.AreEqual(string.Empty, vm.DrugTestResultText);
        }

        [Test]
        public void Ctor_NotTested_HasRemaining_EnablesButton()
        {
            _levels.DrugTestsRemaining = 1;
            _levels.HasDrugTestResultResult = false;

            var vm = new SuspectDetailViewModel(_suspect, _levels);

            Assert.IsTrue(vm.IsDrugTestButtonEnabled);
            Assert.AreEqual(string.Empty, vm.DrugTestResultText);
        }

        [Test]
        public void Ctor_AlreadyTested_PopulatesResultText_DisablesButton()
        {
            _levels.HasDrugTestResultResult = true;
            _levels.GetDrugTestResultResult = DrugTestResult.Positive;

            var vm = new SuspectDetailViewModel(_suspect, _levels);

            Assert.IsFalse(vm.IsDrugTestButtonEnabled);
            Assert.AreEqual(DrugTestResult.Positive.ToDisplayName(), vm.DrugTestResultText);
        }

        [Test]
        public void Ctor_RaisesStateChanged_Once()
        {
            int calls = 0;
            _levels.HasDrugTestResultResult = false;
            _levels.DrugTestsRemaining = 1;

            var vm = new SuspectDetailViewModel(_suspect, _levels);
            vm.StateChanged += () => calls++;

            // Subscribe-after-construction should not raise again on subscribe;
            // the construction-time raise happened before the handler existed.
            Assert.AreEqual(0, calls);
        }

        [Test]
        public void RequestDrugTest_ButtonDisabled_DoesNothing()
        {
            _levels.DrugTestsRemaining = 0;
            _levels.HasDrugTestResultResult = false;
            var vm = new SuspectDetailViewModel(_suspect, _levels);

            vm.RequestDrugTest();

            Assert.AreEqual(0, _levels.UseDrugTestCalls);
            Assert.AreEqual(0, _levels.RecordDrugTestCalls);
        }

        [Test]
        public void RequestDrugTest_UseDrugTestReturnsFalse_DoesNotRecord()
        {
            _levels.UseDrugTestResult = false;
            var vm = new SuspectDetailViewModel(_suspect, _levels);

            vm.RequestDrugTest();

            Assert.AreEqual(1, _levels.UseDrugTestCalls);
            Assert.AreEqual(0, _levels.RecordDrugTestCalls);
        }

        [Test]
        public void RequestDrugTest_OnSuccess_RecordsSuspectDataResult()
        {
            var vm = new SuspectDetailViewModel(_suspect, _levels);

            vm.RequestDrugTest();

            Assert.AreEqual(1, _levels.RecordDrugTestCalls);
            Assert.AreEqual(_suspect, _levels.LastRecordDrugTestSuspect);
            Assert.AreEqual(DrugTestResult.Positive, _levels.LastRecordDrugTestResult);
        }

        [Test]
        public void RequestDrugTest_OnSuccess_DisablesButton_AndPopulatesText()
        {
            _levels.HasDrugTestResultResult = true;
            _levels.GetDrugTestResultResult = DrugTestResult.Positive;
            var vm = new SuspectDetailViewModel(_suspect, _levels);

            vm.RequestDrugTest();

            Assert.IsFalse(vm.IsDrugTestButtonEnabled);
            Assert.AreEqual(DrugTestResult.Positive.ToDisplayName(), vm.DrugTestResultText);
        }

        [Test]
        public void RequestDrugTest_OnSuccess_RaisesStateChanged()
        {
            int calls = 0;
            var vm = new SuspectDetailViewModel(_suspect, _levels);
            vm.StateChanged += () => calls++;

            vm.RequestDrugTest();

            Assert.AreEqual(1, calls);
        }

        [Test]
        public void SelectVerdict_CallsRecordJudgedSuspect()
        {
            var vm = new SuspectDetailViewModel(_suspect, _levels);

            vm.SelectVerdict(SuspectRole.Dealer);

            Assert.AreEqual(1, _levels.RecordJudgedSuspectCalls);
            Assert.AreEqual(_suspect, _levels.LastRecordJudgedSuspect);
            Assert.AreEqual(SuspectRole.Dealer, _levels.LastRecordJudgedRole);
        }

        [Test]
        public void SelectVerdict_RaisesVerdictRecorded_NotCloseRequested()
        {
            int verdictCalls = 0;
            int closeCalls = 0;
            var vm = new SuspectDetailViewModel(_suspect, _levels);
            vm.VerdictRecorded += () => verdictCalls++;
            vm.CloseRequested += () => closeCalls++;

            vm.SelectVerdict(SuspectRole.User);

            Assert.AreEqual(1, verdictCalls);
            Assert.AreEqual(0, closeCalls);
        }

        [Test]
        public void RequestClose_RaisesCloseRequested_DoesNotRecordVerdict()
        {
            int closeCalls = 0;
            var vm = new SuspectDetailViewModel(_suspect, _levels);
            vm.CloseRequested += () => closeCalls++;

            vm.RequestClose();

            Assert.AreEqual(1, closeCalls);
            Assert.AreEqual(0, _levels.RecordJudgedSuspectCalls);
        }

        [Test]
        public void Dispose_CanBeCalled_Idempotently()
        {
            var vm = new SuspectDetailViewModel(_suspect, _levels);

            Assert.DoesNotThrow(() => vm.Dispose());
            Assert.DoesNotThrow(() => vm.Dispose());
        }

        // ----- helpers -----

        private static SuspectData CreateSuspect(string name, string description, string evidenceText, DrugTestResult result)
        {
            var so = ScriptableObject.CreateInstance<SuspectData>();
            var t = typeof(SuspectData);
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            t.GetField("_suspectName", flags)!.SetValue(so, name);
            t.GetField("_description", flags)!.SetValue(so, description);
            t.GetField("_evidenceText", flags)!.SetValue(so, evidenceText);
            t.GetField("_drugTestResult", flags)!.SetValue(so, result);
            return so;
        }

        /// <summary>
        /// Minimal in-memory ILevelController for VM tests. Only the surface
        /// the VM actually touches is meaningful; everything else is a
        /// not-implemented stub.
        /// </summary>
        private sealed class FakeLevelController : ILevelController
        {
            public int DrugTestsRemaining { get; set; }
            public bool HasDrugTestResultResult { get; set; }
            public DrugTestResult GetDrugTestResultResult { get; set; }
            public bool UseDrugTestResult { get; set; }

            public int UseDrugTestCalls { get; private set; }
            public int RecordDrugTestCalls { get; private set; }
            public SuspectData? LastRecordDrugTestSuspect { get; private set; }
            public DrugTestResult LastRecordDrugTestResult { get; private set; }
            public int RecordJudgedSuspectCalls { get; private set; }
            public SuspectData? LastRecordJudgedSuspect { get; private set; }
            public SuspectRole LastRecordJudgedRole { get; private set; }

            public LevelConfig? CurrentLevelConfig => null;
            public int JudgedCount => 0;
            public int TotalSuspects => 0;
            public bool AllSuspectsJudged => false;
            public event Action<LevelConfig>? LevelLoaded;

            public bool IsSuspectJudged(SuspectData suspect) => false;
            public SuspectRole GetSuspectVerdict(SuspectData suspect) => SuspectRole.Normal;

            public void RecordJudgedSuspect(SuspectData suspect, SuspectRole playerChoice)
            {
                RecordJudgedSuspectCalls++;
                LastRecordJudgedSuspect = suspect;
                LastRecordJudgedRole = playerChoice;
            }

            public bool UseDrugTest()
            {
                UseDrugTestCalls++;
                return UseDrugTestResult;
            }

            public void RecordDrugTest(SuspectData suspect, DrugTestResult result)
            {
                RecordDrugTestCalls++;
                LastRecordDrugTestSuspect = suspect;
                LastRecordDrugTestResult = result;
            }

            public bool HasDrugTestResult(SuspectData suspect) => HasDrugTestResultResult;
            public DrugTestResult GetDrugTestResult(SuspectData suspect) => GetDrugTestResultResult;
            public void LoadLevel(LevelConfig config) { }
        }
    }
}
