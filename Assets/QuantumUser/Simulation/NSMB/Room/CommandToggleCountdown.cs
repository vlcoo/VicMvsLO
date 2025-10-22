using Photon.Deterministic;

namespace Quantum {
    public class CommandToggleCountdown : DeterministicCommand, ILobbyCommand {
        public override void Serialize(BitStream stream) {
            // Sorry, nothing.
        }

        public unsafe void Execute(Frame f, PlayerRef sender, PlayerData* playerData) {
            if (f.Global->GameState != GameState.PreGameRoom && !playerData->IsRoomHost) {
                // Only the host can start the countdown.
                return;
            }
            
            if (f.IsVerified) {
                f.MapAssetRef = f.Global->Rules.Stage;
            }
            f.Global->PlayerLoadFrames = (ushort) (20 * f.UpdateRate);
            f.Global->GameState = GameState.WaitingForPlayers;

            f.Events.GameStateChanged(GameState.WaitingForPlayers);

            // bool gameStarting = f.Global->GameStartFrames == 0;
            // if (gameStarting && !QuantumUtils.IsGameStartable(f)) {
            //     return;
            // }
            // f.Global->GameStartFrames = (ushort) (gameStarting ? 3 * f.UpdateRate : 0);
            // f.Events.StartingCountdownChanged(gameStarting);
        }
    }
}