using Photon.Deterministic;

namespace Quantum {
    public class CommandUpdatePing : DeterministicCommand, ILobbyCommand {

        public int PingMs;
        public byte Device;

        public override void Serialize(BitStream stream) {
            stream.Serialize(ref PingMs);
            stream.Serialize(ref Device);
        }
        public unsafe void Execute(Frame f, PlayerRef sender, PlayerData* playerData) {
            playerData->Ping = PingMs;
            playerData->Device = Device;
            f.Events.PlayerDataChanged(sender);
        }
    }
}