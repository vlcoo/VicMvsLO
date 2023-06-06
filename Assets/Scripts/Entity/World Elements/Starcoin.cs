using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Starcoin : MonoBehaviour
{
    [Range(1, 3)] public int number = 1;
    [NonSerialized] public Animator animationController;
    public MeshRenderer model;
    public Material disabledMaterial;
    
    void Start()
    {
        animationController = GetComponent<Animator>();
    }

    void Disappear()
    {
        gameObject.SetActive(false);
    }

    public void SetDisabled()
    {
        model.materials = new[] { disabledMaterial, disabledMaterial };
    }
}
