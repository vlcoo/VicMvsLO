using System;
using System.Collections.Generic;
using NSMB.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorChooser : MonoBehaviour, KeepChildInFocus.IFocusIgnore
{
    [SerializeField] private Canvas baseCanvas;
    [SerializeField] private GameObject template, blockerTemplate, content;
    [SerializeField] private Sprite clearSprite;
    [SerializeField] private string property;
    private readonly List<Button> buttons = new();

    private readonly List<ColorButton> colorButtons = new();
    private GameObject blocker;
    private List<Navigation> navigations = new();
    [NonSerialized] public int selected;

    public void Start()
    {
        content.SetActive(false);
        /*PlayerColorSet[] colors = GlobalController.Instance.skins;

        for (int i = 0; i < colors.Length; i++) {
            PlayerColorSet color = colors[i];

            GameObject newButton = Instantiate(template, template.transform.parent);
            ColorButton cb = newButton.GetComponent<ColorButton>();
            colorButtons.Add(cb);
            cb.palette = color;

            Button b = newButton.GetComponent<Button>();
            newButton.name = color?.name ?? "Reset";
            if (color == null)
                b.image.sprite = clearSprite;

            newButton.SetActive(true);
            buttons.Add(b);

            Navigation navigation = new() { mode = Navigation.Mode.Explicit };

            if (i > 0 && i % 4 != 0) {
                Navigation n = navigations[i - 1];
                n.selectOnRight = b;
                navigations[i - 1] = n;
                navigation.selectOnLeft = buttons[i - 1];
            }
            if (i >= 4) {
                Navigation n = navigations[i - 4];
                n.selectOnDown = b;
                navigations[i - 4] = n;
                navigation.selectOnUp = buttons[i - 4];
            }

            navigations.Add(navigation);
        }

        for (int i = 0; i < buttons.Count; i++) {
            buttons[i].navigation = navigations[i];
        }*/

        ChangeCharacter(Utils.GetCharacterData());
    }

    public void ChangeCharacter(PlayerData data)
    {
        /*foreach (ColorButton b in colorButtons)
            b.Instantiate(data);*/

        colorButtons.ForEach(button => Destroy(button.gameObject));
        colorButtons.Clear();
        buttons.ForEach(button => Destroy(button.gameObject));
        buttons.Clear();

        var colors = Utils.GetColorsForPlayer(data);
        colors.Insert(0, null);

        foreach (var color in colors)
        {
            var newButton = Instantiate(template, template.transform.parent);
            var cb = newButton.GetComponent<ColorButton>();
            colorButtons.Add(cb);
            cb.Instantiate(color);
            var b = newButton.GetComponent<Button>();
            if (color == null)
            {
                cb.OnDeselect(null);
                b.image.sprite = clearSprite;
            }

            newButton.SetActive(true);
            buttons.Add(b);
        }
    }

    public void SelectColor(Button button)
    {
        selected = buttons.IndexOf(button);
        MainMenuManager.Instance.SetPlayerColor(buttons.IndexOf(button));
        Close();
    }

    public void Open()
    {
        blocker = Instantiate(blockerTemplate, baseCanvas.transform);
        blocker.SetActive(true);
        content.SetActive(true);

        EventSystem.current.SetSelectedGameObject(buttons[selected].gameObject);
    }

    public void Close()
    {
        Destroy(blocker);
        EventSystem.current.SetSelectedGameObject(gameObject);
        content.SetActive(false);
    }
}