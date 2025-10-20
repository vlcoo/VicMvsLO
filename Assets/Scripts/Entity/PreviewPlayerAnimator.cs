using System.Collections.Generic;
using UnityEngine;

public class PreviewPlayerAnimator : MonoBehaviour {
    private static readonly int AnimGoalJump = Animator.StringToHash("goalJump");
    private static readonly int AnimOnGround = Animator.StringToHash("onGround");
    private static readonly int ParamOverallsColor = Shader.PropertyToID("OverallsColor");
    private static readonly int ParamShirtColor = Shader.PropertyToID("ShirtColor");
    private static readonly int ParamHatUsesOverallsColor = Shader.PropertyToID("HatUsesOverallsColor");
    
    public Animator animator;
    public AudioSource sfx;
    public GameObject modelParent;
    private bool _isVisible = false;
    private readonly List<Renderer> _renderers = new();
    
    void Start()
    {
        _renderers.AddRange(GetComponentsInChildren<MeshRenderer>(true));
        _renderers.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>(true));
        SetVisible(false);
    }

    public void SetVisible(bool how) {
        modelParent.SetActive(how);
        
        if (how) {
            animator.SetBool(AnimOnGround, true);
        } 

        _isVisible = how;
    }

    public void SetSelected() {
        if (_isVisible) return;
        SetVisible(true);
        animator.SetTrigger(AnimGoalJump);
        sfx.Play();
    }
    
    public void SetPalette(CharacterPalette palette, CharacterAsset character) {
        var materialBlock = new MaterialPropertyBlock();
        if (palette != null) {
            materialBlock.SetVector(ParamOverallsColor, palette.OverallsColor.AsColor.linear);
            materialBlock.SetVector(ParamShirtColor, palette.ShirtColor.AsColor.linear);
            materialBlock.SetFloat(ParamHatUsesOverallsColor, palette.HatUsesOverallsColor ? 1 : 0);
        }
        foreach (Renderer r in _renderers) {
            r.SetPropertyBlock(materialBlock);
        }
    }
}
