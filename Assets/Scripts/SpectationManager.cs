using System.Collections.Generic;
using NSMB.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpectationManager : MonoBehaviour
{
    [SerializeField] private GameObject spectationUI;
    [SerializeField] private TMP_Text spectatingText;
    private bool _spectating;
    private PlayerController _targetPlayer;
    private int targetIndex;

    public bool Spectating
    {
        get => _spectating;
        set
        {
            _spectating = value;
            if (TargetPlayer == null)
                SpectateNextPlayer();

            UpdateSpectateUI();
            GameManager.Instance.SetSpectateMusic(value);
        }
    }

    public PlayerController TargetPlayer
    {
        get => _targetPlayer;
        set
        {
            if (_targetPlayer)
                _targetPlayer.cameraController.IsControllingCamera = false;

            _targetPlayer = value;
            if (value != null)
            {
                UpdateSpectateUI();
                value.cameraController.IsControllingCamera = true;
            }
        }
    }

    public void Update()
    {
        if (!Spectating)
            return;

        if (!TargetPlayer)
            SpectateNextPlayer();
    }

    public void OnEnable()
    {
        InputSystem.controls.UI.SpectatePlayerByIndex.performed += SpectatePlayerIndex;
    }

    public void OnDisable()
    {
        InputSystem.controls.UI.SpectatePlayerByIndex.performed -= SpectatePlayerIndex;
    }

    public void UpdateSpectateUI()
    {
        spectationUI.SetActive(Spectating);
        if (!Spectating || !UIUpdater.Instance)
            return;

        UIUpdater.Instance.player = TargetPlayer;
        if (!TargetPlayer || !TargetPlayer.photonView)
            return;

        spectatingText.text = $"Spectating: {TargetPlayer.photonView.Owner.GetUniqueNickname()}";
    }

    public void SpectateNextPlayer()
    {
        var players = GameManager.Instance.players;
        var count = players.Count;
        if (count <= 0)
            return;

        TargetPlayer = null;

        var nulls = 0;
        while (!TargetPlayer)
        {
            targetIndex = (targetIndex + 1) % count;
            TargetPlayer = players[targetIndex];
            if (nulls++ >= count)
                break;
        }
    }

    public void SpectatePreviousPlayer()
    {
        var players = GameManager.Instance.players;
        var count = players.Count;
        if (count <= 0)
            return;

        TargetPlayer = null;

        var nulls = 0;
        while (!TargetPlayer)
        {
            targetIndex = (targetIndex + count - 1) % count;
            TargetPlayer = players[targetIndex];
            if (nulls++ >= count)
                break;
        }
    }

    private void SpectatePlayerIndex(InputAction.CallbackContext context)
    {
        if (!Spectating)
            return;

        if (int.TryParse(context.control.name, out var index))
        {
            index += 9;
            index %= 10;

            List<PlayerController> sortedPlayers = new(GameManager.Instance.players);
            sortedPlayers.Sort(new PlayerComparer());

            if (index >= sortedPlayers.Count)
                return;

            var newTarget = sortedPlayers[index];

            if (!newTarget)
                return;

            TargetPlayer = newTarget;
        }
    }

    public class PlayerComparer : IComparer<PlayerController>
    {
        public int Compare(PlayerController x, PlayerController y)
        {
            if (!x ^ !y)
                return !x ? 1 : -1;

            if (x.stars == y.stars || x.lives == 0 || y.lives == 0)
            {
                if (Mathf.Max(0, x.lives) == Mathf.Max(0, y.lives))
                    return x.playerId - y.playerId;

                return y.lives - x.lives;
            }

            return y.stars - x.stars;
        }
    }
}