using Photon.Deterministic;

namespace Quantum {
    public class CommandChangePowerupsHuds : DeterministicCommand, ILobbyCommand {
        
        public PowerupsHuds EnabledChanges;
        
        public bool PMushroom;
        public bool PFireFlower;
        public bool PIceFlower;
        public bool PPropellerMushroom;
        public bool PBlueShell;
        public bool PHammerSuit;
        public bool PMiniMushroom;
        public bool PMegaMushroom;
        public bool PStarman;
        
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
            
            stream.Serialize(ref PMushroom);
            stream.Serialize(ref PFireFlower);
            stream.Serialize(ref PIceFlower);
            stream.Serialize(ref PPropellerMushroom);
            stream.Serialize(ref PBlueShell);
            stream.Serialize(ref PHammerSuit);
            stream.Serialize(ref PMiniMushroom);
            stream.Serialize(ref PMegaMushroom);
            stream.Serialize(ref PStarman);
            
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

            if (rulesChanges.HasFlag(PowerupsHuds.PMushroom)) rules.PMushroom = PMushroom;
            if (rulesChanges.HasFlag(PowerupsHuds.PFireFlower)) rules.PFireFlower = PFireFlower;
            if (rulesChanges.HasFlag(PowerupsHuds.PIceFlower)) rules.PIceFlower = PIceFlower;
            if (rulesChanges.HasFlag(PowerupsHuds.PPropellerMushroom)) rules.PPropellerMushroom = PPropellerMushroom;
            if (rulesChanges.HasFlag(PowerupsHuds.PBlueShell)) rules.PBlueShell = PBlueShell;
            if (rulesChanges.HasFlag(PowerupsHuds.PHammerSuit)) rules.PHammerSuit = PHammerSuit;
            if (rulesChanges.HasFlag(PowerupsHuds.PMiniMushroom)) rules.PMiniMushroom = PMiniMushroom;
            if (rulesChanges.HasFlag(PowerupsHuds.PMegaMushroom)) rules.PMegaMushroom = PMegaMushroom;
            if (rulesChanges.HasFlag(PowerupsHuds.PStarman)) rules.PStarman = PStarman;
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
            PMushroom = 1 << 0,
            PFireFlower = 1 << 1,
            PIceFlower = 1 << 2,
            PPropellerMushroom = 1 << 3,
            PBlueShell = 1 << 4,
            PHammerSuit = 1 << 5,
            PMiniMushroom = 1 << 6,
            PMegaMushroom = 1 << 7,
            PStarman = 1 << 8,
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