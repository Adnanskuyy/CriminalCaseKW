using System;
using CriminalCase2.Data;

namespace CriminalCase2.Domain
{
    public interface IGameStateProvider
    {
        GameState CurrentState { get; }
        LevelConfig? CurrentLevel { get; }
        event Action<GameState>? StateChanged;
    }
}
