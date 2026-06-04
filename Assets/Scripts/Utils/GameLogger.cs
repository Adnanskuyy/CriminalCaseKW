using UnityEngine;

namespace CriminalCase2.Utils
{
    public static class GameLogger
    {
        private static IGameLogger _backend = new UnityLogger();

        public static void SetBackend(IGameLogger backend)
        {
            _backend = backend ?? new UnityLogger();
        }

        public static void Info(string message) => _backend.Info(message);
        public static void Warn(string message) => _backend.Warn(message);
        public static void Error(string message) => _backend.Error(message);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetBackend()
        {
            _backend = new UnityLogger();
        }
    }
}
