using UnityEngine;

namespace CriminalCase2.Utils
{
    public sealed class UnityLogger : IGameLogger
    {
        public void Info(string message) => Debug.Log(message);
        public void Warn(string message) => Debug.LogWarning(message);
        public void Error(string message) => Debug.LogError(message);
    }
}
