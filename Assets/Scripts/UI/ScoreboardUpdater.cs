using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScoreboardUpdater : MonoBehaviour
{
    public static ScoreboardUpdater instance;
    private static IComparer<ScoreboardEntry> entryComparer;

    [SerializeField] private GameObject entryTemplate, rulesListBox;

    private readonly List<ScoreboardEntry> entries = new();
    private Animator animator;
    private bool manuallyToggled, autoToggled;

    public void Awake()
    {
        instance = this;
        animator = GetComponent<Animator>();
        if (entryComparer == null)
            entryComparer = new ScoreboardEntry.EntryComparer();
    }

    public void OnEnable()
    {
        InputSystem.controls.UI.Scoreboard.performed += OnToggle;
    }

    public void OnDisable()
    {
        InputSystem.controls.UI.Scoreboard.performed -= OnToggle;
        //rulesListBox.transform.SetSiblingIndex(transform.childCount - 1);
    }

    private void OnToggle(InputAction.CallbackContext context)
    {
        ManualToggle();
    }

    public void SetEnabled()
    {
        manuallyToggled = true;
        animator.SetFloat("speed", 1);
        animator.Play("toggle", 0, 0.99f);
    }

    public void ManualToggle()
    {
        if (autoToggled && !manuallyToggled)
        {
            //exception, already open. close.
            manuallyToggled = false;
            autoToggled = false;
        }
        else
        {
            manuallyToggled = !manuallyToggled;
        }

        PlayAnimation(manuallyToggled);
    }

    private void PlayAnimation(bool enabled)
    {
        animator.SetFloat("speed", enabled ? 1.5f : -1.5f);
        animator.Play("toggle", 0, Mathf.Clamp01(animator.GetCurrentAnimatorStateInfo(0).normalizedTime));
    }

    public void OnDeathToggle()
    {
        if (!manuallyToggled)
        {
            PlayAnimation(true);
            autoToggled = true;
        }
    }

    public void OnRespawnToggle()
    {
        if (!manuallyToggled)
        {
            PlayAnimation(false);
            autoToggled = false;
        }
    }

    public void Reposition()
    {
        entries.Sort(entryComparer);
        entries.ForEach(se => se.transform.SetAsLastSibling());

        rulesListBox.transform.SetSiblingIndex(transform.childCount - 1);
    }

    public void Populate(IEnumerable<PlayerController> players)
    {
        foreach (var player in players)
        {
            if (!player)
                continue;

            var entryObj = Instantiate(entryTemplate, transform);
            entryObj.SetActive(true);
            entryObj.name = player.photonView.Owner.NickName;
            var entry = entryObj.GetComponent<ScoreboardEntry>();
            entry.target = player;

            entries.Add(entry);
        }

        Reposition();
    }
}