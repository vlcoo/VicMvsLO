using System.Collections;
using NSMB.Utils;
using UnityEngine;
using UnityEngine.UI;

public class FadeOutManager : MonoBehaviour {

    public float fadeTimer, fadeTime, blackTime, totalTime;

    private Image image;
    private Coroutine fadeCoroutine;
    public Animator anim;
    public bool alreadyInitiallyFadedOut = false;

    public void SetInvisible(bool how)
    {
        transform.localScale = how ? Vector3.zero : new Vector3(1f, 1f, 1f);
    }

    public void Start() {
        image = GetComponent<Image>();
        anim = GetComponent<Animator>();
        SetInvisible(GlobalController.Instance.settings.reduceUIAnims);
    }

    public void FadeOutAndIn(float fadeTime, float blackTime)
    {
        anim.SetTrigger("FadeInAndOut");
    }

    public void FadeOut()
    {
        if (alreadyInitiallyFadedOut) return;
        anim.SetTrigger("FadeOut");
        alreadyInitiallyFadedOut = true;
    }
}