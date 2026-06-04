using System.Reflection;
using CriminalCase2.Data;
using CriminalCase2.Services;
using CriminalCase2.UI;
using NUnit.Framework;
using UnityEngine;

namespace CriminalCase2.Tests
{
    public class VideoPlayerUITests
    {
        private GameObject _go = null!;
        private VideoPlayerUI _videoPlayerUI = null!;
        private FakeGameStateProvider _fakeState = null!;

        [SetUp]
        public void SetUp()
        {
            GameServices.ResetForTesting();
            _fakeState = new FakeGameStateProvider();
            GameServices.Register(_fakeState);

            _go = new GameObject("test-video-player-ui");
            _videoPlayerUI = _go.AddComponent<VideoPlayerUI>();

            // EditMode tests do not run MonoBehaviour lifecycle. We intentionally
            // do NOT call OnEnable here: in tests the VideoPlayer is not wired,
            // so SetupVideoPlayer would log an error. The state-transition methods
            // we exercise (OnPlayClicked, OnSkipClicked) don't depend on OnEnable.
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
            {
                Object.DestroyImmediate(_go);
                _go = null!;
                _videoPlayerUI = null!;
            }
            GameServices.ResetForTesting();
        }

        [Test]
        public void OnPlayClicked_NoVideoPlayer_AdvancesToInvestigation()
        {
            // _videoPlayer is null because no Inspector wiring in tests.
            InvokePrivate(_videoPlayerUI, "OnPlayClicked");
            Assert.AreEqual(GameState.Investigation, _fakeState.CurrentState);
        }

        [Test]
        public void OnSkipClicked_AdvancesToInvestigation()
        {
            InvokePrivate(_videoPlayerUI, "OnSkipClicked");
            Assert.AreEqual(GameState.Investigation, _fakeState.CurrentState);
        }

        [Test]
        public void OnPlayClicked_WithNullGameState_DoesNotThrow()
        {
            // Edge case: if GameServices is not registered, the chain
            // GameServices.GameState?.SetState(...) must swallow the null.
            GameServices.ResetForTesting();
            Assert.DoesNotThrow(() => InvokePrivate(_videoPlayerUI, "OnPlayClicked"));
        }

        private static void InvokePrivate(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Method {methodName} not found on {target.GetType().Name}");
            method!.Invoke(target, null);
        }
    }
}
