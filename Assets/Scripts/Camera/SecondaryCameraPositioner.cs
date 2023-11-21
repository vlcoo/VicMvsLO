using UnityEngine;

public class SecondaryCameraPositioner : MonoBehaviour
{
    private bool destroyed;

    public void UpdatePosition()
    {
        if (destroyed)
            return;

        if (GameManager.Instance)
        {
            if (!GameManager.Instance.loopingLevel)
            {
                Destroy(gameObject);
                destroyed = true;
                return;
            }

            var right = Camera.main.transform.position.x > GameManager.Instance.GetLevelMiddleX();
            transform.localPosition = new Vector3(GameManager.Instance.levelWidthTile * (right ? -1 : 1), 0, 0);
        }
    }
}