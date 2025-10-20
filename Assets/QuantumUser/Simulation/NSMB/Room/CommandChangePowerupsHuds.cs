using Photon.Deterministic;

namespace Quantum {
    public class CommandChangePowerupsHuds : DeterministicCommand, ILobbyCommand {
        public int ChanceMushroom;
        public int ChanceFireFlower;
        public int ChanceIceFlower;
        public int ChancePropellerMushroom;
        public int ChanceBlueShell;
        public int ChanceHammerSuit;
        public int ChanceMiniMushroom;
        public int ChanceMegaMushroom;
        public int ChanceStarman;
        
        public bool HStars;
        public bool HPlayers;
        public bool HHost;
        public bool HIceCubes;
        public int HTeamTarget;
        public bool HStarCount;
        public bool HLifeCount;
        public bool HLapCount;
        public bool HCoinCount;
        public bool HNicknames;

        public override void Serialize(BitStream stream) {
            stream.Serialize(ref ChanceMushroom);
            stream.Serialize(ref ChanceFireFlower);
            stream.Serialize(ref ChanceIceFlower);
            stream.Serialize(ref ChancePropellerMushroom);
            stream.Serialize(ref ChanceBlueShell);
            stream.Serialize(ref ChanceHammerSuit);
            stream.Serialize(ref ChanceMiniMushroom);
            stream.Serialize(ref ChanceMegaMushroom);
            stream.Serialize(ref ChanceStarman);
            
            stream.Serialize(ref HStars);
            stream.Serialize(ref HPlayers);
            stream.Serialize(ref HHost);
            stream.Serialize(ref HIceCubes);
            stream.Serialize(ref HTeamTarget);
            stream.Serialize(ref HStarCount);
            stream.Serialize(ref HLifeCount);
            stream.Serialize(ref HLapCount);
            stream.Serialize(ref HCoinCount);
            stream.Serialize(ref HNicknames);
        }

        public unsafe void Execute(Frame f, PlayerRef sender, PlayerData* playerData) {
            if (f.Global->GameState != GameState.PreGameRoom || !playerData->IsRoomHost) {
                // Only the host can change rules.
                return;
            }

            var rules = f.Global->Rules;

            rules.ChanceMushroom = ChanceMushroom;
            rules.ChanceFireFlower = ChanceFireFlower;
            rules.ChanceIceFlower = ChanceIceFlower;
            rules.ChancePropellerMushroom = ChancePropellerMushroom;
            rules.ChanceBlueShell = ChanceBlueShell;
            rules.ChanceHammerSuit = ChanceHammerSuit;
            rules.ChanceMiniMushroom = ChanceMiniMushroom;
            rules.ChanceMegaMushroom = ChanceMegaMushroom;
            rules.ChanceStarman = ChanceStarman;
            rules.HStars = HStars;
            rules.HPlayers = HPlayers;
            rules.HHost = HHost;
            rules.HIceCubes = HIceCubes;
            rules.HTeamTarget = HTeamTarget;
            rules.HStarCount = HStarCount;
            rules.HLifeCount = HLifeCount;
            rules.HLapCount = HLapCount;
            rules.HCoinCount = HCoinCount;
            rules.HNicknames = HNicknames;
            
            f.Global->Rules = rules;
            f.Events.RulesChanged(false, false);

            if (f.Global->GameStartFrames > 0 && !QuantumUtils.IsGameStartable(f)) {
                GameLogicSystem.StopCountdown(f);
            }
        }

        public enum PowerupsHuds : uint {
            Mushroom = 1 << 0,
            FireFlower = 1 << 1,
            IceFlower = 1 << 2,
            PropellerMushroom = 1 << 3,
            BlueShell = 1 << 4,
            HammerSuit = 1 << 5,
            MiniMushroom = 1 << 6,
            MegaMushroom = 1 << 7,
            Starman = 1 << 8,
            HStars = 1 << 9,
            HPlayers = 1 << 10,
            HHost = 1 << 11,
            HIceCubes = 1 << 12,
            HTeamTarget = 1 << 13,
            HStarCount = 1 << 14,
            HLifeCount = 1 << 15,
            HLapCount = 1 << 16,
            HCoinCount = 1 << 17,
            HNicknames = 1 << 18,
        }
    }
}