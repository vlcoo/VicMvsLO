using System.Collections;
using NSMB.Utils;
using UnityEngine;
using UnityEngine.UI;

public class FadeOutManager : MonoBehaviour {

    public float fadeTimer, fadeTime, blackTime, totalTime;

    private Image image;
    private Coroutine fadeCoroutine;
    private Animator anim;

    public void Start() {
        image = GetComponent<Image>();
        anim = GetComponent<Animator>();
    }

    public void FadeOutAndIn(float fadeTime, float blackTime) {
        anim.SetTrigger("FadeInAndOut");
    }

    public void FadeOut()
    {
        anim.SetTrigger("FadeOut");
    }
}