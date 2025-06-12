using Photon.Deterministic;

namespace Quantum {
    public class CommandChangeRules : DeterministicCommand, ILobbyCommand {

        public Rules EnabledChanges;

        public AssetRef<Map> Stage;
        public int StarsToWin;
        public int CoinsForPowerup;
        public int Lives;
        public int TimerMinutes;
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

        public bool HStars;
        public bool HPlayers;
        public bool HHost;
        public bool HIceCubes;
        public int HTeamTarget;
        // public bool HStarCount;
        // public bool HLifeCount;
        // public bool HLapCount;
        // public bool HCoinCount;
        // public bool HNicknames;

        public override void Serialize(BitStream stream) {
            uint changes = (uint) EnabledChanges;
            stream.Serialize(ref changes);
            EnabledChanges = (Rules) changes;

            stream.Serialize(ref Stage);
            stream.Serialize(ref StarsToWin);
            stream.Serialize(ref CoinsForPowerup);
            stream.Serialize(ref Lives);
            stream.Serialize(ref TimerMinutes);
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
            
            stream.Serialize(ref HStars);
            stream.Serialize(ref HPlayers);
            stream.Serialize(ref HHost);
            stream.Serialize(ref HIceCubes);
            stream.Serialize(ref HTeamTarget);
            // stream.Serialize(ref HStarCount);
            // stream.Serialize(ref HLifeCount);
            // stream.Serialize(ref HLapCount);
            // stream.Serialize(ref HCoinCount);
            // stream.Serialize(ref HNicknames);
        }

        public unsafe void Execute(Frame f, PlayerRef sender, PlayerData* playerData) {
            if (f.Global->GameState != GameState.PreGameRoom || !playerData->IsRoomHost) {
                // Only the host can change rules.
                return;
            }

            Rules rulesChanges = EnabledChanges;
            var rules = f.Global->Rules;
            bool levelChanged = false;

            if (rulesChanges.HasFlag(Rules.Stage)) {
                levelChanged = rules.Stage != Stage;
                rules.Stage = Stage;
            }
            
            if (rulesChanges.HasFlag(Rules.StarsToWin)) rules.StarsToWin = StarsToWin;
            if (rulesChanges.HasFlag(Rules.CoinsForPowerup)) rules.CoinsForPowerup = CoinsForPowerup;
            if (rulesChanges.HasFlag(Rules.Lives)) rules.Lives = Lives;
            if (rulesChanges.HasFlag(Rules.TimerSeconds)) rules.TimerSeconds = TimerMinutes;
            if (rulesChanges.HasFlag(Rules.TeamsEnabled)) rules.TeamsEnabled = TeamsEnabled;
            if (rulesChanges.HasFlag(Rules.CustomPowerupsEnabled)) rules.CustomPowerupsEnabled = CustomPowerupsEnabled;
            if (rulesChanges.HasFlag(Rules.DrawOnTimeUp)) rules.DrawOnTimeUp = DrawOnTimeUp;
            if (rulesChanges.HasFlag(Rules.Laps)) rules.Laps = Laps;
            
            if (rulesChanges.HasFlag(Rules.SNoReserve)) { rules.SNoReserve = SNoReserve; }
            if (rulesChanges.HasFlag(Rules.SNoDroppedStars)) { rules.SNoDroppedStars = SNoDroppedStars; }
            if (rulesChanges.HasFlag(Rules.SInstantDeath)) { rules.SInstantDeath = SInstantDeath; }
            if (rulesChanges.HasFlag(Rules.SNoDefrost)) { rules.SNoDefrost = SNoDefrost; }
            if (rulesChanges.HasFlag(Rules.SNoCollisions)) { rules.SNoCollisions = SNoCollisions; }
            if (rulesChanges.HasFlag(Rules.SNoIframes)) { rules.SNoIframes = SNoIframes; }
            if (rulesChanges.HasFlag(Rules.SHideSeek)) { rules.SHideSeek = SHideSeek; }
            if (rulesChanges.HasFlag(Rules.SNoEnemies)) { rules.SNoEnemies = SNoEnemies; }
            if (rulesChanges.HasFlag(Rules.SNoBahs)) { rules.SNoBahs = SNoBahs; }
            if (rulesChanges.HasFlag(Rules.SPitWrap)) { rules.SPitWrap = SPitWrap; }
            if (rulesChanges.HasFlag(Rules.SAllBricks)) { rules.SAllBricks = SAllBricks; }
            if (rulesChanges.HasFlag(Rules.SNoLooping)) { rules.SNoLooping = SNoLooping; }
            if (rulesChanges.HasFlag(Rules.SNoCoins)) { rules.SNoCoins = SNoCoins; }
            if (rulesChanges.HasFlag(Rules.SNoPowerups)) { rules.SNoPowerups = SNoPowerups; }
            if (rulesChanges.HasFlag(Rules.SShowCoinCount)) { rules.SShowCoinCount = SShowCoinCount; }
            
            if (rulesChanges.HasFlag(Rules.HStars)) { rules.HStars = HStars; }
            if (rulesChanges.HasFlag(Rules.HPlayers)) { rules.HPlayers = HPlayers; }
            if (rulesChanges.HasFlag(Rules.HHost)) { rules.HHost = HHost; }
            if (rulesChanges.HasFlag(Rules.HIceCubes)) { rules.HIceCubes = HIceCubes; }
            if (rulesChanges.HasFlag(Rules.HTeamTarget)) { rules.HTeamTarget = HTeamTarget; }
            // if (rulesChanges.HasFlag(Rules.HStarCount)) { rules.HStarCount = HStarCount; }
            // if (rulesChanges.HasFlag(Rules.HLifeCount)) { rules.HLifeCount = HLifeCount; }
            // if (rulesChanges.HasFlag(Rules.HLapCount)) { rules.HLapCount = HLapCount; }
            // if (rulesChanges.HasFlag(Rules.HCoinCount)) { rules.HCoinCount = HCoinCount; }
            // if (rulesChanges.HasFlag(Rules.HNicknames)) { rules.HNicknames = HNicknames; }
            
            f.Global->Rules = rules;
            f.Events.RulesChanged(levelChanged);

            if (f.Global->GameStartFrames > 0 && !QuantumUtils.IsGameStartable(f)) {
                GameLogicSystem.StopCountdown(f);
            }
        }

        public enum Rules : uint {
            None = 0,
            Stage = 1 << 0,
            StarsToWin = 1 << 1,
            CoinsForPowerup = 1 << 2,
            Lives = 1 << 3,
            TimerSeconds = 1 << 4,
            TeamsEnabled = 1 << 5,
            CustomPowerupsEnabled = 1 << 6,
            DrawOnTimeUp = 1 << 7,
            Laps = 1 << 8,
            PowerupChances = 1 << 9,
            SNoReserve = 1 << 10,
            SNoDroppedStars = 1 << 11,
            SInstantDeath = 1 << 12,
            SNoDefrost = 1 << 13,
            SNoCollisions = 1 << 14,
            SNoIframes = 1 << 15,
            SHideSeek = 1 << 16,
            SNoEnemies = 1 << 17,
            SNoBahs = 1 << 18,
            SPitWrap = 1 << 19,
            SAllBricks = 1 << 20,
            SNoLooping = 1 << 21,
            SNoCoins = 1 << 22,
            SNoPowerups = 1 << 23,
            SShowCoinCount = 1 << 24,
            HStars = 1 << 25,
            HPlayers = 1 << 26,
            HHost = 1 << 27,
            HIceCubes = 1 << 28,
            HTeamTarget = 1 << 29,
            // HStarCount = 1 << 30,
            // HLifeCount = 1L << 31,
            // HLapCount = 1L << 32,
            // HCoinCount = 1L << 33,
            // HNicknames = 1L << 34,
        }
    }
}