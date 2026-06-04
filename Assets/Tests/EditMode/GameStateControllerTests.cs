using System;
using System.Reflection;
using CriminalCase2.Data;
using CriminalCase2.Domain;
using CriminalCase2.Managers;
using CriminalCase2.Services;
using NUnit.Framework;
using UnityEngine;

namespace CriminalCase2.Tests
{
    public class GameStateControllerTests
    {
        private GameObject _testGo;
        private GameStateController _controller;
        private FakeGameStateProvider _provider;

        [SetUp]
        public void SetUp()
        {
            _provider = new FakeGameStateProvider();
            GameServices.ResetForTesting();
            GameServices.Register(_provider);

            _testGo = new GameObject("test-state-controller");
            _controller = _testGo.AddComponent<GameStateController>();

            // EditMode tests do not invoke MonoBehaviour lifecycle methods.
            // Call OnEnable directly to simulate the runtime subscription path.
            InvokeLifecycle(_controller, "OnEnable");
        }

        [TearDown]
        public void TearDown()
        {
            if (_testGo != null)
            {
                InvokeLifecycle(_controller, "OnDisable");
                UnityEngine.Object.DestroyImmediate(_testGo);
                _testGo = null;
                _controller = null;
            }
            GameServices.ResetForTesting();
        }

        [Test]
        public void OnEnable_SubscribesToStateChanged()
        {
            Assert.AreEqual(1, _provider.SubscriberCount, "Controller should subscribe to StateChanged on enable.");
        }

        [Test]
        public void OnDisable_UnsubscribesFromStateChanged()
        {
            InvokeLifecycle(_controller, "OnDisable");
            Assert.AreEqual(0, _provider.SubscriberCount, "Controller should unsubscribe from StateChanged on disable.");
        }

        [Test]
        public void StateChanged_Tutorial_AdvancesToInvestigation()
        {
            _provider.FireStateChanged(GameState.Tutorial);
            Assert.AreEqual(GameState.Investigation, _provider.CurrentState,
                "Tutorial should dispatch SetState(Investigation).");
        }

        [Test]
        public void StateChanged_AllStates_AreHandledWithoutThrowing()
        {
            Assert.DoesNotThrow(() => _provider.FireStateChanged(GameState.IntroVideo));
            Assert.DoesNotThrow(() => _provider.FireStateChanged(GameState.Tutorial));
            Assert.DoesNotThrow(() => _provider.FireStateChanged(GameState.Investigation));
            Assert.DoesNotThrow(() => _provider.FireStateChanged(GameState.Verdict));
            Assert.DoesNotThrow(() => _provider.FireStateChanged(GameState.Results));
        }

        private static void InvokeLifecycle(MonoBehaviour target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.IsNotNull(method, $"Lifecycle method {methodName} not found on {target.GetType().Name}");
            method!.Invoke(target, null);
        }
    }

    /// <summary>
    /// Minimal IGameStateProvider for tests. Records SetState invocations and exposes
    /// a way to fire StateChanged directly.
    /// </summary>
    internal sealed class FakeGameStateProvider : IGameStateProvider
    {
        public GameState CurrentState { get; private set; } = GameState.IntroVideo;
        public LevelConfig? CurrentLevel => null;
        public event Action<GameState>? StateChanged;

        public int SubscriberCount => StateChanged?.GetInvocationList().Length ?? 0;

        public void FireStateChanged(GameState newState)
        {
            CurrentState = newState;
            StateChanged?.Invoke(newState);
        }

        public void SetState(GameState newState)
        {
            CurrentState = newState;
            StateChanged?.Invoke(newState);
        }

        public void AdvanceToNextLevel(Action? onComplete = null) { }
    }
}
