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
        enabled = player.HasSpecialName();

        nameText.text = (player.IsMasterClient ? "<sprite name=\"room_host\">" : "") +
                        Utils.GetCharacterData(player).uistring +
                        player.GetUniqueNickname();

        Utils.GetCustomProperty(Enums.NetPlayerProperties.Ping, out int ping, player.CustomProperties);
        var signalStrength = ping switch
        {
            < 0 => "connection_great",
            < 80 => "connection_good",
            < 120 => "connection_fair",
            < 180 => "connection_bad",
            _ => "connection_disconnected"
        };
        pingText.text = $"{ping}ms <sprite name=\"{signalStrength}\">";

        Utils.GetCustomProperty(Enums.NetPlayerProperties.DeviceType, out Utils.DeviceType deviceType,
            player.CustomProperties);
        var deviceTypeText = deviceType switch
        {
            Utils.DeviceType.EDITOR => "platform_editor",
            Utils.DeviceType.MOBILE => "platform_mobile",
            Utils.DeviceType.DESKTOP => "platform_desktop",
            Utils.DeviceType.BROWSER => "platform_browser",
            Utils.DeviceType.OTHER => "platform_unknown",
            _ => ""
        };
        deviceText.text = $"<sprite name=\"{deviceTypeText}\">";

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