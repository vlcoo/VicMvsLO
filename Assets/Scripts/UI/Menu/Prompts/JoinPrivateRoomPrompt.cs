using UnityEngine;
using TMPro;

using NSMB.UI.MainMenu;

namespace NSMB.UI.Prompts {
    public class JoinPrivateRoomPrompt : UIPrompt {

        //---Serialized Variables
        [SerializeField] private TMP_InputField roomIdInput;

        protected override void SetDefaults() {
            roomIdInput.text = "";
        }

        public void JoinPrivateRoom() {
            string id = roomIdInput.text.ToUpper();
            if (id.Length != NetworkHandler.RoomIdLength) {
                MainMenuManager.Instance.OpenErrorBox("ui.rooms.joinprivate.invalid");
                return;
            }

            gameObject.SetActive(false);
            _ = NetworkHandler.JoinRoom(id);
        }
    }
}
