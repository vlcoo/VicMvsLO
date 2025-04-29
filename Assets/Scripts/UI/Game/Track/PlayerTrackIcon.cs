using Quantum;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NSMB.UI.Game.Track {
    public unsafe class PlayerTrackIcon : TrackIcon {

        //---Static Variables
        public static bool HideAllPlayerIcons = false;
        private static readonly Vector3 TwoThirds = Vector3.one * (2f / 3f);
        private static readonly Vector3 FlipY = new(1f, -1f, 1f);
        private static readonly WaitForSeconds FlashWait = new(0.1f);

        //---Serialized Variables
        [SerializeField] private GameObject allImageParent;
        [SerializeField] private Image teamIcon;
        [SerializeField] private Image hostIcon;
        [SerializeField] private Image iceCubeIcon;
        [SerializeField] private Image targetIcon;

        //---Private Variables
        private Coroutine flashRoutine;
        private bool iceCubeIconEnabled;

        public override void OnActivate(Frame f) {
            image.enabled = true;

            var mario = f.Unsafe.GetPointer<MarioPlayer>(targetEntity);
            image.color = Utils.Utils.GetPlayerColor(f, mario->PlayerRef);
            if (f.Global->Rules.TeamsEnabled) {
                teamIcon.sprite = f.SimulationConfig.Teams[mario->GetTeam(f)].spriteColorblind;
            }

            stage.HidePlayersOnMinimap = !f.Global->Rules.HPlayers;
            hostIcon.enabled = f.Global->Rules.HHost && QuantumUtils.GetPlayerData(f, mario->PlayerRef)->IsRoomHost;
            targetIcon.enabled = f.Global->Rules.TeamsEnabled && f.Global->Rules.HTeamTarget == mario->GetTeam(f);
            iceCubeIconEnabled = f.Global->Rules.HIceCubes;
        }

        public override void OnDeactivate() {
            if (flashRoutine != null) {
                StopCoroutine(flashRoutine);
                flashRoutine = null;
            }
        }

        public void Start() {
            QuantumCallback.Subscribe<CallbackGameResynced>(this, OnGameResynced);
            QuantumEvent.Subscribe<EventMarioPlayerDied>(this, OnMarioPlayerDied);
            QuantumEvent.Subscribe<EventMarioPlayerRespawned>(this, OnMarioPlayerRespawned);
            QuantumEvent.Subscribe<EventEntityFrozen>(this, OnEntityFrozen);
            QuantumEvent.Subscribe<EventEntityThawed>(this, OnEntityThawed);
        }

        public override void OnUpdateView() {
            bool controllingCamera = playerElements.CameraAnimator.Target == targetEntity;
            image.transform.localScale = controllingCamera ? FlipY : TwoThirds;

            Frame f = PredictedFrame;
            if (flashRoutine == null) {
                image.enabled = controllingCamera || !stage.HidePlayersOnMinimap;
            }
            teamIcon.gameObject.SetActive(image.enabled && Settings.Instance.GraphicsColorblind && f.Global->Rules.TeamsEnabled && !controllingCamera);
            
        }

        private void OnGameResynced(CallbackGameResynced e) {
            // TODO: do proper if statements to start the flashing if needed?
            // eh. probably not needed.
            if (flashRoutine != null) {
                StopCoroutine(flashRoutine);
                flashRoutine = null;
            }
        }

        private IEnumerator Flash() {
            while (true) {
                image.enabled = !image.enabled;
                yield return FlashWait;
            }
        }

        public void OnMarioPlayerDied(EventMarioPlayerDied e) {
            if (e.Entity != targetEntity) {
                return;
            }

            if (flashRoutine == null) {
                flashRoutine = StartCoroutine(Flash());
            }
        }

        public void OnMarioPlayerRespawned(EventMarioPlayerRespawned e) {
            if (e.Entity != targetEntity) {
                return;
            }

            image.enabled = true;
            if (flashRoutine != null) {
                StopCoroutine(flashRoutine);
            }
            flashRoutine = null;
        }

        public void OnEntityFrozen(EventEntityFrozen e) {
            if (!iceCubeIconEnabled || !e.Entity.Equals(targetEntity)) return;
            iceCubeIcon.enabled = true;
        }
        
        public void OnEntityThawed(EventEntityThawed e) {
            if (!iceCubeIconEnabled || !e.Entity.Equals(targetEntity)) return;
            iceCubeIcon.enabled = false;
        }
    }
}
