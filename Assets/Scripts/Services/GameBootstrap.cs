using CriminalCase2.Managers;
using UnityEngine;

namespace CriminalCase2.Services
{
    internal static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (GameServices.IsRegistered) return;

            var root = new GameObject("[GameServicesRoot]");
            Object.DontDestroyOnLoad(root);

            root.AddComponent<GameManager>();
            root.AddComponent<LevelManager>();
        }
    }
}
