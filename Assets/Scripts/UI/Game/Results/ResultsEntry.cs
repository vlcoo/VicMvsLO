using NSMB.Utils;
using Quantum;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultsEntry : MonoBehaviour {

    //---Seriaized Variables
    [SerializeField] private Image leftHalf, rightHalf;
    [SerializeField] private Image characterIcon;
    [SerializeField] private TMP_Text usernameText, starCountText;
    [SerializeField] private RectTransform childTransform;
    [SerializeField] private GameObject fullSlot, emptySlot;
    [SerializeField] private Color firstPlaceColor, secondPlaceColor, thirdPlaceColor, unrankedColor;
    [SerializeField] private float offscreenStartPosition = -500, slideInTimeSeconds = 0.1f;

        //---Private Variables
        private PlayerRef player;
        private NicknameColor nicknameColor = NicknameColor.White;

    public unsafe void Initialize(Frame f, in PlayerInformation? info, int ranking, float delay, int stars = -1) {
        bool occupied = info.HasValue;
        fullSlot.SetActive(occupied);
        emptySlot.SetActive(!occupied);

        if (occupied) {
            PlayerRef player = info.Value.PlayerRef;

                usernameText.text = info.Value.Nickname.ToString().ToValidUsername(f, player);
                nicknameColor = NicknameColor.Parse(info.Value.NicknameColor.ToString());
                usernameText.color = nicknameColor.Sample();
                characterIcon.sprite = f.FindAsset(f.SimulationConfig.CharacterDatas[info.Value.Character]).ReadySprite;

            if (stars < 0) {
                starCountText.text = "<sprite name=hudnumber_x>";
                rightHalf.color = unrankedColor;
            } else {
                starCountText.text = Utils.GetSymbolString("S" + stars.ToString(), Utils.uiSymbols);
                rightHalf.color = ranking switch {
                    1 => firstPlaceColor,
                    2 => secondPlaceColor,
                    3 => thirdPlaceColor,
                    _ => unrankedColor
                };
            }

                leftHalf.color = Utils.Utils.GetPlayerColor(f, player, s: 0.7f, considerDisqualifications: false);
                
                var playerData = QuantumUtils.GetPlayerData(f, player);
                readyCheckmark.SetActive(playerData != null && playerData->VotedToContinue);
            } else {
                player = PlayerRef.None;
                nicknameColor = NicknameColor.White;
                leftHalf.color = rightHalf.color = unrankedColor;
                readyCheckmark.SetActive(false);
            }


        StartCoroutine(ResultsHandler.MoveObjectToTarget(childTransform, offscreenStartPosition, 0, slideInTimeSeconds, delay));
    }

        public void Update() {
            if (!nicknameColor.Constant) {
                usernameText.color = nicknameColor.Sample();
            }
        }

        private unsafe void OnPlayerDataChanged(EventPlayerDataChanged e) {
            if (e.Player != player) {
                return;
            }
