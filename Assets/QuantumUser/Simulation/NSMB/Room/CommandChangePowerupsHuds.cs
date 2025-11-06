using Photon.Deterministic;

namespace Quantum {
    public class CommandChangePowerupsHuds : DeterministicCommand, ILobbyCommand {
        
        public PowerupsHuds EnabledChanges;
        
        public bool ChanceMushroom;
        public bool ChanceFireFlower;
        public bool ChanceIceFlower;
        public bool ChancePropellerMushroom;
        public bool ChanceBlueShell;
        public bool ChanceHammerSuit;
        public bool ChanceMiniMushroom;
        public bool ChanceMegaMushroom;
        public bool ChanceStarman;
        
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
            if (stream.Writing) {
                stream.WriteUInt((uint) EnabledChanges);
            } else {
                EnabledChanges = (PowerupsHuds) stream.ReadUInt();
            }
            
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

            PowerupsHuds rulesChanges = EnabledChanges;
            var rules = f.Global->Rules;

            if (rulesChanges.HasFlag(PowerupsHuds.Mushroom)) rules.ChanceMushroom = ChanceMushroom;
            if (rulesChanges.HasFlag(PowerupsHuds.FireFlower)) rules.ChanceFireFlower = ChanceFireFlower;
            if (rulesChanges.HasFlag(PowerupsHuds.IceFlower)) rules.ChanceIceFlower = ChanceIceFlower;
            if (rulesChanges.HasFlag(PowerupsHuds.PropellerMushroom)) rules.ChancePropellerMushroom = ChancePropellerMushroom;
            if (rulesChanges.HasFlag(PowerupsHuds.BlueShell)) rules.ChanceBlueShell = ChanceBlueShell;
            if (rulesChanges.HasFlag(PowerupsHuds.HammerSuit)) rules.ChanceHammerSuit = ChanceHammerSuit;
            if (rulesChanges.HasFlag(PowerupsHuds.MiniMushroom)) rules.ChanceMiniMushroom = ChanceMiniMushroom;
            if (rulesChanges.HasFlag(PowerupsHuds.MegaMushroom)) rules.ChanceMegaMushroom = ChanceMegaMushroom;
            if (rulesChanges.HasFlag(PowerupsHuds.Starman)) rules.ChanceStarman = ChanceStarman;
            if (rulesChanges.HasFlag(PowerupsHuds.HStars)) rules.HStars = HStars;
            if (rulesChanges.HasFlag(PowerupsHuds.HPlayers)) rules.HPlayers = HPlayers;
            if (rulesChanges.HasFlag(PowerupsHuds.HHost)) rules.HHost = HHost;
            if (rulesChanges.HasFlag(PowerupsHuds.HIceCubes)) rules.HIceCubes = HIceCubes;
            if (rulesChanges.HasFlag(PowerupsHuds.HTeamTarget)) rules.HTeamTarget = HTeamTarget;
            if (rulesChanges.HasFlag(PowerupsHuds.HStarCount)) rules.HStarCount = HStarCount;
            if (rulesChanges.HasFlag(PowerupsHuds.HLifeCount)) rules.HLifeCount = HLifeCount;
            if (rulesChanges.HasFlag(PowerupsHuds.HLapCount)) rules.HLapCount = HLapCount;
            if (rulesChanges.HasFlag(PowerupsHuds.HCoinCount)) rules.HCoinCount = HCoinCount;
            if (rulesChanges.HasFlag(PowerupsHuds.HNicknames)) rules.HNicknames = HNicknames;
            
            f.Global->Rules = rules;
            f.Events.RulesChanged(false, false);
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