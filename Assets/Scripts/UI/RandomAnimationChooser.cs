using UnityEngine;

public class RandomAnimationChooser : MonoBehaviour
{
    public Animator animator;
    public string propertyName;
    public int animationCount;

    private void Start()
    {
        animator ??= GetComponent<Animator>();
        animator.SetInteger(propertyName, Perso.GetIntRange(0, animationCount));
    }
}