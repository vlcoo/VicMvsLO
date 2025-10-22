using NSMB.Utilities.Extensions;
using Quantum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static NSMB.Utilities.QuantumViewUtils;

namespace NSMB.UI.Loading {
    public unsafe class LoadingCanvas : MonoBehaviour {

        public static event Action<bool> OnLoadingEnded;

        //---Public Variables
        public bool dontHideOnGameDestroy;

        //---Serialized Variables
        // [SerializeField] private AudioSource audioSource;
        // [SerializeField] private MarioLoader mario;
        [SerializeField] private Songinator musicPlayer;
        [SerializeField] private Animator animator;
        [SerializeField] private CanvasGroup loadingGroup, readyGroup;
        [SerializeField] private Image readyBackground, readyImage, groundImage;
        [SerializeField] private GameObject marioScene, bowserScene;

        [SerializeField] private CharacterAsset defaultCharacterAsset;

        //---Private Variables
        private Coroutine endCoroutine;
        private bool running;
        private MarioLoader mario;

        public void OnValidate() {
            // this.SetIfNull(ref mario, UnityExtensions.GetComponentType.Children);
        }

        public void Startup() {
            QuantumCallback.Subscribe<CallbackUnitySceneLoadBegin>(this, OnUnitySceneLoadBegin);
            QuantumCallback.Subscribe<CallbackUnitySceneLoadDone>(this, OnUnitySceneLoadDone);
            QuantumCallback.Subscribe<CallbackGameStarted>(this, OnGameStarted);
            QuantumCallback.Subscribe<CallbackGameDestroyed>(this, OnGameDestroyed);
            QuantumEvent.Subscribe<EventGameStateChanged>(this, OnGameStateChanged);
        }

        public void Initialize(QuantumGame game) {
            if (running) {
                return;
            }

            int characterIndex = 0;
            CharacterAsset character = defaultCharacterAsset;
            if (game != null) {
                Frame f = game.Frames.Predicted;
                
                f.TryFindAsset(f.Map.UserAsset, out VersusStageData stage);
                groundImage.sprite = stage.GroundSprite;
                
                List<PlayerRef> localPlayers = game.GetLocalPlayers();
                if (localPlayers.Count > 0) {
                    PlayerRef player = localPlayers[0];
                    var playerData = QuantumUtils.GetPlayerData(f, player);

                    if (playerData != null) {
                        characterIndex = playerData->Character;
                    } else {
                        characterIndex = Settings.Instance.generalCharacter;
                    }
                }

                var characters = f.SimulationConfig.CharacterDatas;
                character = f.FindAsset(characters[characterIndex % characters.Length]);
            }
            
            var characterScene = character.IsMinion ? bowserScene : marioScene;
            characterScene.SetActive(true);
            mario = characterScene.GetComponentInChildren<MarioLoader>();
            mario.Initialize(character);
            readyImage.sprite = character.ReadySprite;

            readyGroup.gameObject.SetActive(false);
            gameObject.SetActive(true);

            loadingGroup.alpha = 1;
            readyGroup.alpha = 0;
            readyBackground.color = Color.clear;

            animator.Play("waiting");

            running = true;
        }

        private void OnUnitySceneLoadBegin(CallbackUnitySceneLoadBegin e) {
            if (e.SceneName != null) {
                // Loading a map.
                Initialize(e.Game);
            }
        }

        private void OnUnitySceneLoadDone(CallbackUnitySceneLoadDone e) {
            if (IsReplay || e.Game.Frames.Predicted.Global->GameState is GameState.Starting or GameState.Playing) {
                EndLoading(e.Game);
            }
        }

        private void OnGameStarted(CallbackGameStarted e) {
            if (!IsReplay) {
                EndLoading(e.Game);
            }
        }

        private void OnGameStateChanged(EventGameStateChanged e) {
            if (e.NewState is GameState.Starting or GameState.Playing) {
                EndLoading(e.Game);
            }
        }

        public void EndLoading(QuantumGame game) {
            if (running && endCoroutine == null) {
                endCoroutine = StartCoroutine(EndLoadingRoutine(game, game.Frames.Predicted.Global->GameState));
            }
        }

        private void OnGameDestroyed(CallbackGameDestroyed e) {
            if (dontHideOnGameDestroy) {
                dontHideOnGameDestroy = false;
                return;
            }
            gameObject.SetActive(false);
        }

        public IEnumerator EndLoadingRoutine(QuantumGame game, GameState state) {
            if (!IsReplay) {
                yield return new WaitForSeconds(1);
            }

            Frame f = game.Frames.Predicted;

            bool longIntro = !IsReplay && (state <= GameState.Starting || game.GetLocalPlayers().Any(p => !(QuantumUtils.GetPlayerDataSafe(f, p)?.IsSpectator ?? true)));
            
            readyGroup.gameObject.SetActive(true);
            animator.SetTrigger(longIntro  ? "loaded" : "spectating");

            musicPlayer.SetPlaybackState(Songinator.PlaybackState.STOPPED, 2f);

            OnLoadingEnded?.Invoke(longIntro);
            running = false;
            endCoroutine = null;
        }

        public void EndAnimation() {
            marioScene.SetActive(false);
            bowserScene.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}
