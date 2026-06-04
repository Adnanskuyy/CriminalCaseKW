using CriminalCase2.Domain;
using CriminalCase2.UI;
using UnityEngine;

namespace CriminalCase2.Services
{
    public static class GameServices
    {
        public static IGameStateProvider? GameState { get; private set; }
        public static ILevelController? Levels { get; private set; }
        public static IVerdictRecorder? Verdicts { get; private set; }
        public static IVideoService? Video { get; private set; }
        public static UIManager? UI { get; private set; }

        public static bool IsRegistered => GameState != null && Levels != null;

        public static void Register(object service)
        {
            if (service is IGameStateProvider gs) GameState = gs;
            if (service is ILevelController lv) Levels = lv;
            if (service is IVerdictRecorder vr) Verdicts = vr;
            if (service is IVideoService vs) Video = vs;
            if (service is UIManager ui) UI = ui;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLoad()
        {
            GameState = null;
            Levels = null;
            Verdicts = null;
            Video = null;
            UI = null;
        }

        /// <summary>
        /// Test-only: clear all service slots. Do not call from runtime code.
        /// </summary>
        public static void ResetForTesting()
        {
            ResetOnLoad();
        }
    }
}
