using UnityEngine;

public class RandomAnimationChooser : MonoBehaviour
{
    public Animator animator;
    public string propertyName;
    public int animationCount;

    private void Start()
    {
        animator ??= GetComponent<Animator>();
        animator.SetInteger(propertyName, Random.Range(0, animationCount));
    }
}