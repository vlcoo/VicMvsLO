using Photon.Deterministic;

namespace Quantum {
    public class CommandGiveUp : DeterministicCommand, ILobbyCommand {
        public override void Serialize(BitStream stream) {
            // Nothing, sorry.
        }
        
        public unsafe void Execute(Frame f, PlayerRef sender, PlayerData* playerData) {
            foreach (var (entity, mario) in f.Unsafe.GetComponentBlockIterator<MarioPlayer>()) {
                if (mario->PlayerRef != sender) continue;

                f.Signals.OnMarioPlayerDisqualified(entity);
                f.Events.MarioPlayerDisqualified(entity);
                f.Destroy(entity);
                GameLogicSystem.CheckForGameEnd(f);
                return;
            }
        }
    }
}