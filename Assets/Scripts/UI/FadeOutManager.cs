using UnityEngine;
using UnityEngine.UI;

public class FadeOutManager : MonoBehaviour
{
    public float fadeTimer, fadeTime, blackTime, totalTime, nextFadeSpeedMultiplier = 1f;
    public Animator anim;
    public bool alreadyInitiallyFadedOut;
    private Coroutine fadeCoroutine;

    private Image image;

    public void Start()
    {
        image = GetComponent<Image>();
        anim = GetComponent<Animator>();
        SetInvisible(GlobalController.Instance.settings.reduceUIAnims);
    }

    public void SetInvisible(bool how)
    {
        transform.localScale = how ? Vector3.zero : new Vector3(1f, 1f, 1f);
    }

    public void FadeOutAndIn(bool positive = false)
    {
        anim.speed = (positive ? 1.5f : 1) * nextFadeSpeedMultiplier;
        anim.SetTrigger(positive ? "FadeDoor" : "FadeInAndOut");
        nextFadeSpeedMultiplier = 1f;
    }

    public void FadeOut()
    {
        anim.speed = 1;
        if (alreadyInitiallyFadedOut) return;
        anim.SetTrigger("FadeOut");
        alreadyInitiallyFadedOut = true;
    }

    public void FadePipe(bool direction)
    {
        anim.speed = 1;
        anim.SetBool("PipeDirection", direction);
        anim.SetTrigger("FadePipe");
    }
}