using System;

namespace Quantum {
    [Serializable]
    public unsafe partial struct GameRules {

        public readonly bool IsLivesEnabled => Lives > 0;
        public readonly bool IsLapsEnabled => Laps > 1;
        public readonly bool IsStarsEnabled => StarsToWin > 0;
        public readonly bool IsCoinsEnabled => CoinsForPowerup > 0;
        public readonly bool IsTimerEnabled => TimerSeconds > 0;

    }
}