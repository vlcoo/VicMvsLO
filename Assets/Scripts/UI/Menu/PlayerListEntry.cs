using NSMB.Utils;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerListEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText, pingText, deviceText;
    [SerializeField] private Image colorStrip;

    [SerializeField] private RectTransform background, options;
    [SerializeField] private GameObject blockerTemplate, firstButton;

    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private LayoutElement layout;

    [SerializeField] private GameObject[] adminOnlyOptions;

    private GameObject blockerInstance;

    public Player player;

    public void Update()
    {
        nameText.colorGradientPreset = Utils.GetRainbowColor();
    }

    private void OnDestroy()
    {
        if (blockerInstance)
            Destroy(blockerInstance);
    }

    public void UpdateText()
    {
        colorStrip.color = Utils.GetPlayerColor(player);
        enabled = player.HasRainbowName();

        nameText.text = (player.IsMasterClient ? "<sprite=5> " : "") + Utils.GetCharacterData(player).uistring +
                        player.GetUniqueNickname();

        Utils.GetCustomProperty(Enums.NetPlayerProperties.Ping, out int ping, player.CustomProperties);
        var signalStrength = ping switch
        {
            < 0 => "52",
            < 80 => "49",
            < 120 => "50",
            < 180 => "51",
            _ => "52"
        };
        pingText.text = $"{ping} <sprite={signalStrength}>";

        Utils.GetCustomProperty(Enums.NetPlayerProperties.DeviceType, out Utils.DeviceType deviceType,
            player.CustomProperties);
        var deviceTypeText = deviceType switch
        {
            Utils.DeviceType.EDITOR => "26",
            Utils.DeviceType.MOBILE => "79",
            Utils.DeviceType.DESKTOP => "77",
            Utils.DeviceType.BROWSER => "78",
            Utils.DeviceType.OTHER => "",
            _ => ""
        };
        deviceText.text = $"<sprite={deviceTypeText}>";

        var parent = transform.parent;
        var childIndex = 0;
        for (var i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i) != gameObject)
                continue;

            childIndex = i;
            break;
        }

        layout.layoutPriority = transform.parent.childCount - childIndex;
    }

    public void ShowDropdown()
    {
        if (blockerInstance)
            Destroy(blockerInstance);

        var admin = PhotonNetwork.IsMasterClient && !player.IsMasterClient;
        foreach (var option in adminOnlyOptions) option.SetActive(admin);

        Canvas.ForceUpdateCanvases();

        blockerInstance = Instantiate(blockerTemplate, rootCanvas.transform);
        var blockerTransform = blockerInstance.GetComponent<RectTransform>();
        blockerTransform.offsetMax = blockerTransform.offsetMin = Vector2.zero;
        blockerInstance.SetActive(true);

        background.offsetMin = new Vector2(background.offsetMin.x, -options.rect.height);
        options.anchoredPosition = new Vector2(options.anchoredPosition.x, -options.rect.height);

        EventSystem.current.SetSelectedGameObject(firstButton);
        MainMenuManager.Instance.sfx.PlayOneShot(Enums.Sounds.UI_Cursor.GetClip());
    }

    public void HideDropdown(bool didAction)
    {
        Destroy(blockerInstance);

        background.offsetMin = new Vector2(background.offsetMin.x, 0);
        options.anchoredPosition = new Vector2(options.anchoredPosition.x, 0);

        MainMenuManager.Instance.sfx.PlayOneShot((didAction ? Enums.Sounds.UI_Decide : Enums.Sounds.UI_Back).GetClip());
    }

    public void BanPlayer()
    {
        MainMenuManager.Instance.Ban(player);
        HideDropdown(true);
    }

    public void KickPlayer()
    {
        MainMenuManager.Instance.Kick(player);
        HideDropdown(true);
    }

    public void MutePlayer()
    {
        MainMenuManager.Instance.Mute(player);
        HideDropdown(true);
    }

    public void PromotePlayer()
    {
        MainMenuManager.Instance.Promote(player);
        HideDropdown(true);
    }

    public void CopyPlayerId()
    {
        MainMenuManager.Instance.CopyPlayerID(player);
        HideDropdown(true);
    }
}