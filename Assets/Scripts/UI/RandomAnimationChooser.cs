using UnityEngine;

public class RandomAnimationChooser : MonoBehaviour {
    public Animator animator;
    public string propertyName;
    public int animationCount;

    private void OnEnable()
    {
        animator ??= GetComponent<Animator>();
        animator.SetInteger(propertyName, Perso.GetIntRange(0, animationCount, propertyName));
    }
}