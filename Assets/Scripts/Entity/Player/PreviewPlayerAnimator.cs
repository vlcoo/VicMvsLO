using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewPlayerAnimator : MonoBehaviour {
    private static readonly int AnimGoalJump = Animator.StringToHash("goalJump");
    private static readonly int AnimOnGround = Animator.StringToHash("onGround");
    public Animator animator;
    public AudioSource sfx;
    public GameObject modelParent;
    private bool _isVisible = false;
    
    // Start is called before the first frame update
    void Start()
    {
        SetVisible(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetVisible(bool how) {
        modelParent.SetActive(how);
        
        if (how) {
            animator.SetBool(AnimOnGround, true);
        } else {
            
        }

        _isVisible = how;
    }

    public void SetSelected() {
        if (!_isVisible) SetVisible(true);
        animator.SetTrigger(AnimGoalJump);
        sfx.Play();
    }
}
