using Photon.Deterministic;
using System;

namespace Quantum {
    public class CommandChangeRules : DeterministicCommand, ILobbyCommand {

        public Rules EnabledChanges;

        public AssetRef<Map> Stage;
        public AssetRef<GamemodeAsset> Gamemode;
        public int StarsToWin;
        public int CoinsForPowerup;
        public int Lives;
        public int TimerSeconds;
        public bool TeamsEnabled;
        public bool CustomPowerupsEnabled;
        public bool DrawOnTimeUp;
        public int Laps;
        
        public bool SNoReserve;
        public bool SNoDroppedStars;
        public bool SInstantDeath;
        public bool SNoDefrost;
        public bool SNoCollisions;
        public bool SNoIframes;
        public bool SHideSeek;
        public bool SNoEnemies;
        public bool SNoBahs;
        public bool SPitWrap;
        public bool SAllBricks;
        public bool SNoLooping;
        public bool SNoCoins;
        public bool SNoPowerups;
        public bool SShowCoinCount;

        public override void Serialize(BitStream stream) {
            if (stream.Writing) {
                stream.WriteUInt((uint) EnabledChanges);
            } else {
                EnabledChanges = (Rules) stream.ReadUInt();
            }

            stream.Serialize(ref Stage);
            stream.Serialize(ref Gamemode);
            stream.Serialize(ref StarsToWin);
            stream.Serialize(ref CoinsForPowerup);
            stream.Serialize(ref Lives);
            stream.Serialize(ref TimerSeconds);
            stream.Serialize(ref TeamsEnabled);
            stream.Serialize(ref CustomPowerupsEnabled);
            stream.Serialize(ref DrawOnTimeUp);
            stream.Serialize(ref Laps);
            
            stream.Serialize(ref SNoReserve);
            stream.Serialize(ref SNoDroppedStars);
            stream.Serialize(ref SInstantDeath);
            stream.Serialize(ref SNoDefrost);
            stream.Serialize(ref SNoCollisions);
            stream.Serialize(ref SNoIframes);
            stream.Serialize(ref SHideSeek);
            stream.Serialize(ref SNoEnemies);
            stream.Serialize(ref SNoBahs);
            stream.Serialize(ref SPitWrap);
            stream.Serialize(ref SAllBricks);
            stream.Serialize(ref SNoLooping);
            stream.Serialize(ref SNoCoins);
            stream.Serialize(ref SNoPowerups);
            stream.Serialize(ref SShowCoinCount);
        }

        public unsafe void Execute(Frame f, PlayerRef sender, PlayerData* playerData) {
            if (f.Global->GameState != GameState.PreGameRoom || !playerData->IsRoomHost) {
                // Only the host can change rules.
                return;
            }

            Rules rulesChanges = EnabledChanges;
            var rules = f.Global->Rules;
            bool gamemodeChanged = false;
            bool levelChanged = false;

            if (rulesChanges.HasFlag(Rules.Gamemode)) {
                gamemodeChanged = rules.Gamemode != Gamemode;
                rules.Gamemode = Gamemode;
            }
            if (rulesChanges.HasFlag(Rules.Stage)) {
                levelChanged = rules.Stage != Stage;
                rules.Stage = Stage;
            }
            if (rulesChanges.HasFlag(Rules.StarsToWin)) rules.StarsToWin = StarsToWin;
            if (rulesChanges.HasFlag(Rules.CoinsForPowerup)) rules.CoinsForPowerup = CoinsForPowerup;
            if (rulesChanges.HasFlag(Rules.Lives)) rules.Lives = Lives;
            if (rulesChanges.HasFlag(Rules.Laps)) rules.Laps = Laps;
            if (rulesChanges.HasFlag(Rules.TimerSeconds)) rules.TimerSeconds = TimerSeconds;
            if (rulesChanges.HasFlag(Rules.TeamsEnabled)) rules.TeamsEnabled = TeamsEnabled;
            if (rulesChanges.HasFlag(Rules.CustomPowerupsEnabled)) rules.CustomPowerupsEnabled = CustomPowerupsEnabled;
            if (rulesChanges.HasFlag(Rules.DrawOnTimeUp)) rules.DrawOnTimeUp = DrawOnTimeUp;
            if (rulesChanges.HasFlag(Rules.SNoReserve)) rules.SNoReserve = SNoReserve;
            if (rulesChanges.HasFlag(Rules.SNoDroppedStars)) rules.SNoDroppedStars = SNoDroppedStars;
            if (rulesChanges.HasFlag(Rules.SInstantDeath)) rules.SInstantDeath = SInstantDeath;
            if (rulesChanges.HasFlag(Rules.SNoDefrost)) rules.SNoDefrost = SNoDefrost;
            if (rulesChanges.HasFlag(Rules.SNoCollisions)) rules.SNoCollisions = SNoCollisions;
            if (rulesChanges.HasFlag(Rules.SNoIframes)) rules.SNoIframes = SNoIframes;
            if (rulesChanges.HasFlag(Rules.SHideSeek)) rules.SHideSeek = SHideSeek;
            if (rulesChanges.HasFlag(Rules.SNoEnemies)) rules.SNoEnemies = SNoEnemies;
            if (rulesChanges.HasFlag(Rules.SNoBahs)) rules.SNoBahs = SNoBahs;
            if (rulesChanges.HasFlag(Rules.SPitWrap)) rules.SPitWrap = SPitWrap;
            if (rulesChanges.HasFlag(Rules.SAllBricks)) rules.SAllBricks = SAllBricks;
            if (rulesChanges.HasFlag(Rules.SNoLooping)) rules.SNoLooping = SNoLooping;
            if (rulesChanges.HasFlag(Rules.SNoCoins)) rules.SNoCoins = SNoCoins;
            if (rulesChanges.HasFlag(Rules.SNoPowerups)) rules.SNoPowerups = SNoPowerups;
            if (rulesChanges.HasFlag(Rules.SShowCoinCount)) rules.SShowCoinCount = SShowCoinCount;

            f.Global->Rules = rules;
            f.Events.RulesChanged(gamemodeChanged, levelChanged);

            if (f.Global->GameStartFrames > 0 && !QuantumUtils.IsGameStartable(f)) {
                GameLogicSystem.StopCountdown(f);
            }
        }

        [Flags]
        public enum Rules : uint {
            None = 0,
            Stage = 1 << 0,
            Gamemode = 1 << 1,
            StarsToWin = 1 << 2,
            CoinsForPowerup = 1 << 3,
            Lives = 1 << 4,
            TimerSeconds = 1 << 5,
            TeamsEnabled = 1 << 6,
            CustomPowerupsEnabled = 1 << 7,
            DrawOnTimeUp = 1 << 8,
            Laps = 1 << 9,
            SNoReserve = 1 << 11,
            SNoDroppedStars = 1 << 12,
            SInstantDeath = 1 << 13,
            SNoDefrost = 1 << 14,
            SNoCollisions = 1 << 15,
            SNoIframes = 1 << 16,
            SHideSeek = 1 << 17,
            SNoEnemies = 1 << 18,
            SNoBahs = 1 << 19,
            SPitWrap = 1 << 20,
            SAllBricks = 1 << 21,
            SNoLooping = 1 << 22,
            SNoCoins = 1 << 23,
            SNoPowerups = 1 << 24,
            SShowCoinCount = 1 << 25,
        }
    }
}