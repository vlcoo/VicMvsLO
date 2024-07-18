using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using Fusion;
using NSMB.Utils;

namespace NSMB.UI.MainMenu {
    public class RoomListManager : MonoBehaviour {

        //---Properties
        private RoomIcon _selectedRoom;
        public RoomIcon SelectedRoom {
            get => _selectedRoom;
            private set {
                if (_selectedRoom != null) {
                    _selectedRoom.Unselect();
                }

                _selectedRoom = value;

                if (_selectedRoom != null) {
                    _selectedRoom.Select();
                }
            }
        }

        //---Serialized Variables
        [SerializeField] private RoomIcon roomIconPrefab, privateRoomIcon;
        [SerializeField] private GameObject roomListScrollRect, privateRoomIdPrompt;
        [SerializeField] private Button joinRoomButton;

        //---Private Variables
        private readonly Dictionary<string, RoomIcon> rooms = new();
        private float lastSelectTime;

        public void Awake() {
            NetworkHandler.OnSessionListUpdated += OnSessionListUpdated;
            NetworkHandler.OnShutdown +=           OnShutdown;
        }
        public void OnDestroy() {
            NetworkHandler.OnSessionListUpdated -= OnSessionListUpdated;
            NetworkHandler.OnShutdown -=           OnShutdown;
        }

        public void SelectRoom(RoomIcon room) {
            if (SelectedRoom == room) {
                if (Time.time - lastSelectTime < 0.3f) {
                    JoinSelectedRoom();
                    return;
                }
            }

            SelectedRoom = room;
            joinRoomButton.interactable = SelectedRoom && Settings.Instance.generalNickname.IsValidUsername(false);
            lastSelectTime = Time.time;
        }

        public async void JoinSelectedRoom() {
            if (!SelectedRoom) {
                return;
            }

            await NetworkHandler.JoinRoom(SelectedRoom.session.Name);
        }

        public void OpenPrivateRoomPrompt() {
            privateRoomIdPrompt.SetActive(true);
        }

        public void RefreshRooms() {
            foreach (RoomIcon room in rooms.Values) {
                room.UpdateUI(room.session);
            }
        }

        public void RemoveRoom(RoomIcon icon) {
            Destroy(icon.gameObject);
            rooms.Remove(icon.session.Name);

            if (SelectedRoom == icon) {
                SelectedRoom = null;
            }
        }

        public void ClearRooms() {
            foreach (RoomIcon room in rooms.Values) {
                Destroy(room.gameObject);
            }

            rooms.Clear();
            SelectedRoom = null;
        }

        //---Callbacks
        private void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
            ClearRooms();
        }

        private void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {

            List<string> invalidRooms = rooms.Keys.ToList();

            foreach (SessionInfo session in sessionList) {

                // Is this check really necessary anymore?

                NetworkUtils.GetSessionProperty(session, Enums.NetRoomProperties.HostName, out string host);
                NetworkUtils.GetSessionProperty(session, Enums.NetRoomProperties.IntProperties, out int packedIntProperties);
                NetworkUtils.IntegerProperties intProperties = (NetworkUtils.IntegerProperties) packedIntProperties;

                bool valid = true;
                valid &= session.IsVisible && session.IsOpen;
                valid &= session.MaxPlayers is > 0 and <= 10;
                valid &= intProperties.maxPlayers is > 0 and <= 10;
                valid &= intProperties.lives <= 99;
                valid &= intProperties.starRequirement is >= -1 and <= 99;
                valid &= intProperties.coinRequirement is >= -1 and <= 99;
                valid &= intProperties.lapRequirement is >= -1 and <= 99;
                valid &= host.IsValidUsername();

                if (valid) {
                    invalidRooms.Remove(session.Name);
                } else {
                    continue;
                }

                RoomIcon roomIcon;
                if (rooms.TryGetValue(session.Name, out RoomIcon room)) {
                    roomIcon = room;
                } else {
                    roomIcon = Instantiate(roomIconPrefab, Vector3.zero, Quaternion.identity);
                    roomIcon.name = session.Name;
                    roomIcon.gameObject.SetActive(true);
                    roomIcon.transform.SetParent(roomListScrollRect.transform, false);
                    roomIcon.session = session;

                    rooms[session.Name] = roomIcon;
                }

                roomIcon.UpdateUI(session);
            }

            foreach (string key in invalidRooms) {
                if (!rooms.TryGetValue(key, out RoomIcon room)) {
                    continue;
                }

                Destroy(room.gameObject);
                rooms.Remove(key);
            }

            //if (askedToJoin && selectedRoom != null) {
            //    JoinSelectedRoom();
            //    askedToJoin = false;
            //    selectedRoom = null;
            //}

            //privateRoomIcon.transform.SetAsFirstSibling();
        }
    }
}
