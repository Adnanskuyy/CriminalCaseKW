#nullable enable
using System;
using System.Collections.Generic;
using CriminalCase2.Data;
using CriminalCase2.ViewModels;
using NUnit.Framework;
using UnityEngine;

namespace CriminalCase2.Tests
{
    [TestFixture]
    public class ResultViewModelTests
    {
        private ResultViewModel _vm = null!;

        [SetUp]
        public void SetUp()
        {
            _vm = new ResultViewModel();
        }

        [TearDown]
        public void TearDown()
        {
            _vm.Dispose();
        }

        [Test]
        public void Ctor_HasEmptyEntries()
        {
            Assert.That(_vm.Entries, Is.Empty);
        }

        [Test]
        public void SetRecords_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _vm.SetRecords(null!));
        }

        [Test]
        public void SetRecords_WithRecords_PopulatesEntriesWith1BasedIndex()
        {
            var records = new List<VerdictRecord>
            {
                new VerdictRecord(CreateSuspect("Alice", SuspectRole.User), SuspectRole.User),
                new VerdictRecord(CreateSuspect("Bob", SuspectRole.Normal), SuspectRole.Dealer),
            };

            _vm.SetRecords(records);

            Assert.That(_vm.Entries.Count, Is.EqualTo(2));
            Assert.That(_vm.Entries[0].Index, Is.EqualTo(1));
            Assert.That(_vm.Entries[0].SuspectName, Is.EqualTo("Alice"));
            Assert.That(_vm.Entries[0].PlayerChoiceDisplay, Is.EqualTo("Pecandu"));
            Assert.That(_vm.Entries[0].CorrectAnswerDisplay, Is.EqualTo("Pecandu"));
            Assert.That(_vm.Entries[1].Index, Is.EqualTo(2));
            Assert.That(_vm.Entries[1].SuspectName, Is.EqualTo("Bob"));
            Assert.That(_vm.Entries[1].PlayerChoiceDisplay, Is.EqualTo("Bandar Narkoba"));
            Assert.That(_vm.Entries[1].CorrectAnswerDisplay, Is.EqualTo("Warga Biasa"));
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
        public void RequestNextLevel_RaisesNextLevelRequested()
        {
            var raised = false;
            _vm.NextLevelRequested += () => raised = true;

            _vm.RequestNextLevel();

            Assert.That(raised, Is.True);
        }

        [Test]
        public void Dispose_Idempotent()
        {
            _vm.Dispose();
            Assert.DoesNotThrow(() => _vm.Dispose());
        }

        [Test]
        public void RequestNextLevel_AfterDispose_Throws()
        {
            _vm.Dispose();
            Assert.Throws<ObjectDisposedException>(() => _vm.RequestNextLevel());
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
    }
}
