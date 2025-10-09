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
            if (stream.Writing) {
                stream.WriteUShort((ushort) EnabledChanges);
            } else {
                EnabledChanges = (Rules) stream.ReadUShort();
            }

            stream.Serialize(ref Stage);
            stream.Serialize(ref Gamemode);
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
            bool gamemodeChanged = false;
            bool levelChanged = false;

            if (rulesChanges.HasFlag(Rules.Gamemode)) {
                gamemodeChanged = rules.Gamemode != Gamemode;

                GameRules tempRules = default;
                f.FindAsset(Gamemode).DefaultRules.Materialize(f, ref tempRules);
                tempRules.Stage = rules.Stage;

                rules = tempRules;
            }
            if (rulesChanges.HasFlag(Rules.Stage)) {
                levelChanged = rules.Stage != Stage;
                rules.Stage = Stage;
            }
            if (rulesChanges.HasFlag(Rules.StarsToWin)) {
                rules.StarsToWin = StarsToWin;
            }
            if (rulesChanges.HasFlag(Rules.CoinsForPowerup)) {
                rules.CoinsForPowerup = CoinsForPowerup;
            }
            if (rulesChanges.HasFlag(Rules.Lives)) {
                rules.Lives = Lives;
            }
            if (rulesChanges.HasFlag(Rules.TimerMinutes)) {
                rules.TimerMinutes = TimerMinutes;
            }
            if (rulesChanges.HasFlag(Rules.TeamsEnabled)) {
                rules.TeamsEnabled = TeamsEnabled;
            }
            if (rulesChanges.HasFlag(Rules.CustomPowerupsEnabled)) {
                rules.CustomPowerupsEnabled = CustomPowerupsEnabled;
            }
            if (rulesChanges.HasFlag(Rules.DrawOnTimeUp)) {
                rules.DrawOnTimeUp = DrawOnTimeUp;
            }

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
            TimerMinutes = 1 << 5,
            TeamsEnabled = 1 << 6,
            CustomPowerupsEnabled = 1 << 7,
            DrawOnTimeUp = 1 << 8,
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