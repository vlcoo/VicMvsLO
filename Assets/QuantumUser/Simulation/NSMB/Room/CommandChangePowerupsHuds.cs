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
            
            f.Global->Rules = rules;
            f.Events.PowerupChancesChanged(f);

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
        }
    }
}