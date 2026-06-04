using System;
using CriminalCase2.Data;

namespace CriminalCase2.Domain
{
    public interface IGameStateProvider
    {
        GameState CurrentState { get; }
        event Action<GameState>? StateChanged;
    }
}
