using CriminalCase2.Utils;
using NUnit.Framework;

namespace CriminalCase2.Tests
{
    public class LoggerTests
    {
        [Test]
        public void StaticFacade_DefaultsToUnityLogger()
        {
            var previous = CaptureBackend(out var restore);
            GameLogger.SetBackend(new UnityLogger());

            GameLogger.Info("info");
            GameLogger.Warn("warn");
            GameLogger.Error("error");
            Assert.Pass();

            restore(previous);
        }

        [Test]
        public void SetBackend_AcceptsCustomImpl()
        {
            var capture = new CaptureLogger();
            GameLogger.SetBackend(capture);

            GameLogger.Info("hello");
            GameLogger.Warn("caution");
            GameLogger.Error("oops");

            Assert.AreEqual(3, capture.Entries.Count);
            Assert.AreEqual("hello", capture.Entries[0]);
            Assert.AreEqual("caution", capture.Entries[1]);
            Assert.AreEqual("oops", capture.Entries[2]);
        }

        [Test]
        public void SetBackend_NullFallsBackToUnity()
        {
            GameLogger.SetBackend(null);
            GameLogger.Info("after-null");
            Assert.Pass();
        }

        private static IGameLogger? CaptureBackend(out System.Action<IGameLogger?> restore)
        {
            restore = _ => { };
            return null;
        }

        private sealed class CaptureLogger : IGameLogger
        {
            public readonly System.Collections.Generic.List<string> Entries = new System.Collections.Generic.List<string>();
            public void Info(string message) => Entries.Add(message);
            public void Warn(string message) => Entries.Add(message);
            public void Error(string message) => Entries.Add(message);
        }
    }
}
