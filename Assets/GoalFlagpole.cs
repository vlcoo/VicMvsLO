using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.U2D;

public class GoalFlagpole : MonoBehaviour
{
    [FormerlySerializedAs("collider")] public BoxCollider2D colliderPole;
    public BoxCollider2D colliderBottom;
    public SpriteShapeController spline;

    public void SetUnlocked(bool how)
    {
        spline.gameObject.SetActive(!how);
    }
}
