using System;
using System.Collections;
using UnityEngine;

public class AnimatedFader : MonoBehaviour {
    public enum FadeStyle {
        Cut,
        Dissolve,
        Circle,
    }
    
    [SerializeField] private Animator anim;
    private FadeStyle currentInStyle, currentOutStyle;

    public void Fade(FadeStyle inStyle, FadeStyle outStyle = FadeStyle.Cut, Action onComplete = null) {
        currentInStyle = inStyle;
        currentOutStyle = outStyle;
        
        anim.SetBool("direction", true);
        switch (inStyle) {
        case FadeStyle.Circle:
            anim.SetTrigger("circle");
            break;
        case FadeStyle.Dissolve:
            anim.SetTrigger("dissolve");
            break;
        case FadeStyle.Cut:
            break;
        }
        StartCoroutine(WaitForAnimation(onComplete));
    }
    
    private IEnumerator WaitForAnimation(Action onComplete) {
        yield return null;
        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !anim.IsInTransition(0));
        yield return new WaitForSeconds(0.5f);
        onComplete?.Invoke();
        FadeOut();
    }

    private void FadeOut() {
        anim.SetBool("direction", false);
        switch (currentOutStyle) {
        case FadeStyle.Circle:
            anim.SetTrigger("circle");
            break;
        case FadeStyle.Dissolve:
            anim.SetTrigger("dissolve");
            break;
        case FadeStyle.Cut:
            break;
        }
    }
}
