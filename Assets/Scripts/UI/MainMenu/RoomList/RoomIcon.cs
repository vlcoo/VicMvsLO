using NSMB.UI.Elements;
using NSMB.UI.Translation;
using NSMB.Utilities;
using NSMB.Utilities.Extensions;
using Photon.Realtime;
using Quantum;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static NSMB.Utilities.NetworkUtils;

namespace NSMB.UI.MainMenu.Submenus.RoomList {
    public class RoomIcon : MonoBehaviour {

        //---Properties
        public bool HasGameStarted {
            get {
                GetCustomProperty(room.CustomProperties, Enums.NetRoomProperties.BoolProperties, out int boolPropertiesPacked);
                BooleanProperties boolProperties = boolPropertiesPacked;
                return boolProperties.GameStarted;
            }
        }

        //---Public Variables
        public RoomInfo room;

        //---Serialized Variables
        [SerializeField] private TMP_Text nameText, symbolsText, detailText;
        [SerializeField] private Image icon;
        [SerializeField] private Tooltip descriptionTooltip;

        public void OnValidate() {
            this.SetIfNull(ref icon);
        }

        public void UpdateUI(RoomInfo newRoomInfo) {
            room = newRoomInfo;

            TranslationManager tm = GlobalController.Instance.translationManager;

            GetCustomProperty(room.CustomProperties, Enums.NetRoomProperties.HostName, out string hostname);
            nameText.text = tm.GetTranslationWithReplacements("ui.rooms.listing.name", "playername", hostname.ToValidNickname(null, PlayerRef.None, false));

            GetCustomProperty(room.CustomProperties, Enums.NetRoomProperties.GamemodeGuid, out string gamemodeAssetGuid);
            GetCustomProperty(room.CustomProperties, Enums.NetRoomProperties.StageGuid, out string stageAssetGuid);
            GetCustomProperty(room.CustomProperties, Enums.NetRoomProperties.IntProperties, out int intPropertiesPacked);
            GetCustomProperty(room.CustomProperties, Enums.NetRoomProperties.BoolProperties, out int boolPropertiesPacked);
            GetCustomProperty(room.CustomProperties, Enums.NetRoomProperties.Description, out string description);

            descriptionTooltip.gameObject.SetActive(description == "");
            descriptionTooltip.message = description.WrapText(50);

            IntegerProperties intProperties = intPropertiesPacked;
            BooleanProperties boolProperties = boolPropertiesPacked;
            
            detailText.text = $"{room.PlayerCount}/{room.MaxPlayers}" + " - " +
                              (boolProperties.GameStarted
                                  ? tm.GetTranslation("ui.rooms.listing.status.started")
                                  : tm.GetTranslation("ui.rooms.listing.status.notstarted"));

            StringBuilder symbols = new();

            if (intProperties.TriggerCount > 0) {
                symbols.Append("<sprite name=room_rules>").Append(Utils.GetSymbolString(intProperties.TriggerCount.ToString(), Utils.smallSymbols));
            }

            if (boolProperties.CustomPowerups) {
                symbols.Append("<sprite name=room_powerups>");
            }

            if (boolProperties.Teams) {
                symbols.Append("<sprite name=room_teams>");
            }

            if (intProperties.Timer > 0) {
                symbols.Append("<sprite name=room_timer>").Append(Utils.GetSymbolString(intProperties.Timer.ToString(), Utils.smallSymbols));
            }

            if (intProperties.Lives > 0) {
                symbols.Append("<sprite name=room_lives>").Append(Utils.GetSymbolString(intProperties.Lives.ToString(), Utils.smallSymbols));
            }

            if (intProperties.StarRequirement > 0) {
                symbols.Append("<sprite name=room_stars>").Append(Utils.GetSymbolString(intProperties.StarRequirement.ToString(), Utils.smallSymbols));
            }

            if (intProperties.CoinRequirement > 0) {
                symbols.Append("<sprite name=room_coins>").Append(Utils.GetSymbolString(intProperties.CoinRequirement.ToString(), Utils.smallSymbols));
            }
            
            symbolsText.text = symbols.ToString();
        }
    }
}
