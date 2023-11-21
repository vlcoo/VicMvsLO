using UnityEngine;

public class WrappingHitbox : MonoBehaviour
{
    private Rigidbody2D body;
    private float levelMiddle, levelWidth;
    private Vector2 offset;
    private BoxCollider2D[] ourColliders, childColliders;

    public void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        if (!body)
            body = GetComponentInParent<Rigidbody2D>();
        ourColliders = GetComponents<BoxCollider2D>();

        // null propagation is ok w/ GameManager.Instance
        if (!(GameManager.Instance?.loopingLevel ?? false))
        {
            enabled = false;
            return;
        }

        childColliders = new BoxCollider2D[ourColliders.Length];
        for (var i = 0; i < ourColliders.Length; i++)
            childColliders[i] = gameObject.AddComponent<BoxCollider2D>();
        levelWidth = GameManager.Instance.levelWidthTile / 2f;
        levelMiddle = GameManager.Instance.GetLevelMinX() + levelWidth / 2f;
        offset = new Vector2(levelWidth, 0);

        LateUpdate();
    }

    public void LateUpdate()
    {
        for (var i = 0; i < ourColliders.Length; i++)
            UpdateChildColliders(i);
    }

    private void UpdateChildColliders(int index)
    {
        var ourCollider = ourColliders[index];
        var childCollider = childColliders[index];

        childCollider.autoTiling = ourCollider.autoTiling;
        childCollider.edgeRadius = ourCollider.edgeRadius;
        childCollider.enabled = ourCollider.enabled;
        childCollider.isTrigger = ourCollider.isTrigger;
        childCollider.offset = ourCollider.offset +
                               (body.position.x < levelMiddle ? offset : -offset) / body.transform.lossyScale;
        childCollider.sharedMaterial = ourCollider.sharedMaterial;
        childCollider.size = ourCollider.size;
        childCollider.usedByComposite = ourCollider.usedByComposite;
        childCollider.usedByEffector = ourCollider.usedByComposite;
    }
}