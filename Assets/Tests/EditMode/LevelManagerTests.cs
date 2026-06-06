#nullable enable
using System;
using System.Collections.Generic;
using CriminalCase2.Data;
using CriminalCase2.Domain;
using CriminalCase2.Managers;
using CriminalCase2.Services;
using NUnit.Framework;
using UnityEngine;

namespace CriminalCase2.Tests
{
    [TestFixture]
    public class LevelManagerTests
    {
        private GameObject _go = null!;
        private LevelManager _level = null!;
        private FakeVerdictRecorder _verdicts = null!;

        [SetUp]
        public void SetUp()
        {
            _verdicts = new FakeVerdictRecorder();
            GameServices.ResetForTesting();
            GameServices.Register(_verdicts);

            _go = new GameObject("LevelManager_Test");
            _level = _go.AddComponent<LevelManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) UnityEngine.Object.DestroyImmediate(_go);
            GameServices.ResetForTesting();
        }

        [Test]
        public void GetSuspectVerdict_UnjudgedSuspect_ReturnsNull()
        {
            var suspect = CreateSuspect("Alice", SuspectRole.User);

            var verdict = _level.GetSuspectVerdict(suspect);

            Assert.That(verdict, Is.Null);
        }

        [Test]
        public void GetSuspectVerdict_AfterRecordJudgedSuspect_ReturnsRecordedRole()
        {
            var suspect = CreateSuspect("Bob", SuspectRole.Dealer);

            _level.RecordJudgedSuspect(suspect, SuspectRole.Dealer);

            var verdict = _level.GetSuspectVerdict(suspect);

            Assert.That(verdict, Is.EqualTo(SuspectRole.Dealer));
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

        private sealed class FakeVerdictRecorder : IVerdictRecorder
        {
            public IReadOnlyList<VerdictRecord> Records { get; private set; } = Array.Empty<VerdictRecord>();
            public event Action<VerdictRecord>? VerdictRecorded;

            public void Record(SuspectData suspect, SuspectRole role)
            {
                var list = new List<VerdictRecord>(Records) { new VerdictRecord(suspect, role) };
                Records = list;
                VerdictRecorded?.Invoke(list[list.Count - 1]);
            }
        }
    }
}
