using Photon.Deterministic;

namespace Quantum {
    public class CommandTaunt : DeterministicCommand {
        public byte EmoteId;
        
        public override void Serialize(BitStream stream) {
            stream.Serialize(ref EmoteId);
        }
    }
}