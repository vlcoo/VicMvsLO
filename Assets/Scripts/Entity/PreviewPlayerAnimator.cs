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
        if (!_isVisible) SetVisible(true);
        animator.SetTrigger(AnimGoalJump);
        sfx.Play();
    }
    
    public void SetPalette(PaletteSet palette, CharacterAsset character) {
        var materialBlock = new MaterialPropertyBlock();
        if (palette != null) {
            var skin = palette.GetPaletteForCharacter(character);
            // materialBlock.SetVector(ParamOverallsColor, skin.overallsColor.linear);
            // materialBlock.SetVector(ParamShirtColor, skin.shirtColor.linear);
            // materialBlock.SetFloat(ParamHatUsesOverallsColor, skin.hatUsesOverallsColor ? 1 : 0);
        }
        foreach (Renderer r in _renderers) {
            r.SetPropertyBlock(materialBlock);
        }
    }
}
