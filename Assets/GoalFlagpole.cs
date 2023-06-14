using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class GoalFlagpole : MonoBehaviour
{
    public BoxCollider2D collider, colliderBottom;
    public SpriteShapeController spline;

    public void SetUnlocked(bool how)
    {
        spline.gameObject.SetActive(!how);
    }
}
