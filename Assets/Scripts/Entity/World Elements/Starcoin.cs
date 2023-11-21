using System;
using UnityEngine;

public class Starcoin : MonoBehaviour
{
    [Range(1, 3)] public int number = 1;
    public MeshRenderer model;
    public Material disabledMaterial;
    [NonSerialized] public Animator animationController;

    private void Start()
    {
        animationController = GetComponent<Animator>();
    }

    private void Disappear()
    {
        gameObject.SetActive(false);
    }

    public void SetDisabled()
    {
        model.materials = new[] { disabledMaterial, disabledMaterial };
    }
}