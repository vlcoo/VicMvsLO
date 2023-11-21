using System.Collections.Generic;
using NSMB.Utils;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private static readonly Vector2 airOffset = new(0, .65f);

    public static float ScreenShake;
    public Vector3 currentPosition;

    private readonly Vector2 airThreshold = new(0.5f, 1.3f);
    private readonly Vector2 groundedThreshold = new(0.5f, 0f);
    private readonly List<SecondaryCameraPositioner> secondaryPositioners = new();
    private PlayerController controller;
    private Vector3 smoothDampVel, playerPos;
    private float startingZ, lastFloor;
    private Camera targetCamera;
    public bool IsControllingCamera { get; set; } = false;

    public void Awake()
    {
        //only control the camera if we're the local player.
        targetCamera = Camera.main;
        startingZ = targetCamera.transform.position.z;
        controller = GetComponent<PlayerController>();
        targetCamera.GetComponentsInChildren(secondaryPositioners);
    }

    public void LateUpdate()
    {
        currentPosition = CalculateNewPosition();
        if (IsControllingCamera)
        {
            var shakeOffset = Vector3.zero;
            if ((ScreenShake -= Time.deltaTime) > 0 && controller.onGround)
                shakeOffset = new Vector3((Random.value - 0.5f) * ScreenShake, (Random.value - 0.5f) * ScreenShake);

            targetCamera.transform.position = currentPosition + shakeOffset;
            if (BackgroundLoop.Instance)
                BackgroundLoop.Instance.Reposition();

            secondaryPositioners.RemoveAll(scp => scp == null);
            secondaryPositioners.ForEach(scp => scp.UpdatePosition());
        }
    }

    private void OnDrawGizmos()
    {
        if (!controller)
            return;

        Gizmos.color = Color.blue;
        var threshold = controller.onGround ? groundedThreshold : airThreshold;
        Gizmos.DrawWireCube(playerPos, threshold * 2);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(playerPos.x, lastFloor), Vector3.right / 2);
    }

    public void Recenter()
    {
        currentPosition = (Vector2)transform.position + airOffset;
        smoothDampVel = Vector3.zero;
        LateUpdate();
    }

    private Vector3 CalculateNewPosition()
    {
        float minY = GameManager.Instance.cameraMinY, heightY = GameManager.Instance.cameraHeightY;
        float minX = GameManager.Instance.cameraMinX, maxX = GameManager.Instance.cameraMaxX;

        if (!controller.dead)
            playerPos = AntiJitter(transform.position);

        var vOrtho = targetCamera.orthographicSize;
        var xOrtho = vOrtho * targetCamera.aspect;

        // instant camera movements. we dont want to lag behind in these cases

        var cameraBottomMax = Mathf.Max(3.5f - transform.lossyScale.y, 1.5f);
        //bottom camera clip
        if (playerPos.y - (currentPosition.y - vOrtho) < cameraBottomMax)
            currentPosition.y = playerPos.y + vOrtho - cameraBottomMax;

        var playerHeight = controller.WorldHitboxSize.y;
        var cameraTopMax = Mathf.Min(1.5f + playerHeight, 4f);

        //top camera clip
        if (playerPos.y - (currentPosition.y + vOrtho) + cameraTopMax > 0)
            currentPosition.y = playerPos.y - vOrtho + cameraTopMax;

        Utils.WrapWorldLocation(ref playerPos);
        var xDifference = Vector2.Distance(Vector2.right * currentPosition.x, Vector2.right * playerPos.x);
        var right = currentPosition.x > playerPos.x;

        if (xDifference >= 8)
        {
            currentPosition.x += (right ? -1 : 1) * GameManager.Instance.levelWidthTile / 2f;
            xDifference = Vector2.Distance(Vector2.right * currentPosition.x, Vector2.right * playerPos.x);
            right = currentPosition.x > playerPos.x;
            if (IsControllingCamera)
                BackgroundLoop.Instance.wrap = true;
        }

        if (xDifference > 0.25f)
            currentPosition.x += (0.25f - xDifference - 0.01f) * (right ? 1 : -1);

        // lagging camera movements
        var targetPosition = currentPosition;
        if (controller.onGround)
            lastFloor = playerPos.y;
        var validFloor = controller.onGround || lastFloor < playerPos.y;

        //top camera clip ON GROUND. slowly pan up, dont do it instantly.
        if (validFloor && lastFloor - (currentPosition.y + vOrtho) + cameraTopMax + 2f > 0)
            targetPosition.y = playerPos.y - vOrtho + cameraTopMax + 2f;


        // Smoothing

        targetPosition = Vector3.SmoothDamp(currentPosition, targetPosition, ref smoothDampVel, .5f);

        // Clamping to within level bounds

        targetPosition.x = Mathf.Clamp(targetPosition.x, minX + xOrtho, maxX - xOrtho);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY + vOrtho,
            heightY == 0 ? minY + vOrtho : minY + heightY - vOrtho);

        // Z preservation

        //targetPosition = AntiJitter(targetPosition);
        targetPosition.z = startingZ;

        return targetPosition;
    }

    private static Vector2 AntiJitter(Vector3 vec)
    {
        vec.y = (int)(vec.y * 100) / 100f;
        return vec;
    }
}