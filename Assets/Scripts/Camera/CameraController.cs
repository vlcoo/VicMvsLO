﻿using System.Collections.Generic;
using UnityEngine;

using Fusion;
using NSMB.Entities.Player;
using NSMB.Game;
using NSMB.Utils;


public class CameraController : NetworkBehaviour {

    //---Static Variables
    private static readonly Vector2 AirOffset = new(0, .65f);
    private static readonly Vector2 AirThreshold = new(0.6f, 1.3f), GroundedThreshold = new(0.6f, 0f);
    private static CameraController CurrentController;

    private static float _screenShake;
    public static float ScreenShake {
        get => _screenShake;
        set {
            if (CurrentController && !CurrentController.controller.IsOnGround)
                return;

            _screenShake = value;
        }
    }

    //---Networked Variables
    [Networked] public Vector3 CurrentPosition { get; set; }
    [Networked] private Vector3 SmoothDampVel { get; set; }
    [Networked] private Vector3 PlayerPos { get; set; }
    [Networked] private float LastFloorHeight { get; set; }

    //---Properties
    private bool _isControllingCamera;
    public bool IsControllingCamera {
        get => _isControllingCamera;
        set {
            _isControllingCamera = value;
            if (value) {
                UIUpdater.Instance.player = controller;
                CurrentController = this;
            }
        }
    }

    //---Serialized Variables
    [SerializeField] private PlayerController controller;

    //---Private Variables
    private readonly List<SecondaryCameraPositioner> secondaryPositioners = new();
    private Camera targetCamera;
    private Interpolator<Vector3> positionInterpolator;

    public void OnValidate() {
        if (!controller) controller = GetComponentInParent<PlayerController>();
    }

    public void Awake() {
        targetCamera = Camera.main;
        targetCamera.GetComponentsInChildren(secondaryPositioners);
    }

    public override void Spawned() {
        positionInterpolator = GetInterpolator<Vector3>(nameof(CurrentPosition));
    }

    public void LateUpdate() {
        if (!IsControllingCamera)
            return;

        Vector3 position;
        if (GetInterpolationData(out InterpolationData data)) {
            // Get extrapolated position.
            position = CalculateNewPosition(data.Alpha);
        } else {
            position = positionInterpolator.Value;
        }

        Vector3 shakeOffset = Vector3.zero;
        if ((_screenShake -= Time.deltaTime) > 0)
            shakeOffset = new Vector3((Random.value - 0.5f) * _screenShake, (Random.value - 0.5f) * _screenShake);

        SetPosition(position + shakeOffset);
    }

    public override void FixedUpdateNetwork() {
        CurrentPosition = CalculateNewPosition();
    }

    public void Recenter(Vector2 pos) {
        PlayerPos = CurrentPosition = pos + AirOffset;
        SmoothDampVel = Vector3.zero;
        SetPosition(PlayerPos);
    }

    private void SetPosition(Vector3 position) {
        if (!IsControllingCamera)
            return;

        targetCamera.transform.position = position;
        if (BackgroundLoop.Instance)
            BackgroundLoop.Instance.Reposition();

        secondaryPositioners.RemoveAll(scp => !scp);
        secondaryPositioners.ForEach(scp => scp.UpdatePosition());
    }

    private Vector3 CalculateNewPosition(float delta = 1) {
        float minY = GameManager.Instance.cameraMinY, heightY = GameManager.Instance.cameraHeightY;
        float minX = GameManager.Instance.cameraMinX, maxX = GameManager.Instance.cameraMaxX;

        if (!controller.IsDead && !controller.IsRespawning)
            PlayerPos = AntiJitter(transform.position);

        float vOrtho = targetCamera.orthographicSize;
        float xOrtho = vOrtho * targetCamera.aspect;

        // Instant camera movements. we dont want to lag behind in these cases
        Vector3 newCameraPosition = CurrentPosition;

        // Bottom camera clip
        float cameraBottomDistanceToPlayer = PlayerPos.y - (newCameraPosition.y - vOrtho);
        float cameraBottomMinDistance = Mathf.Max(3.5f - controller.models.transform.lossyScale.y, 1.5f);
        if (cameraBottomDistanceToPlayer < cameraBottomMinDistance)
            newCameraPosition.y -= (cameraBottomMinDistance - cameraBottomDistanceToPlayer);

        // Top camera clip
        float playerHeight = controller.models.transform.lossyScale.y;
        float cameraTopMax = Mathf.Min(1.5f + playerHeight, 3.5f);
        if (PlayerPos.y - (newCameraPosition.y + vOrtho) + cameraTopMax > 0)
            newCameraPosition.y = PlayerPos.y - vOrtho + cameraTopMax;

        Vector3 wrappedPos = PlayerPos;
        Utils.WrapWorldLocation(ref wrappedPos);
        PlayerPos = wrappedPos;

        float xDifference = Vector2.Distance(Vector2.right * newCameraPosition.x, Vector2.right * PlayerPos.x);
        bool right = newCameraPosition.x > PlayerPos.x;

        if (xDifference >= 2) {
            newCameraPosition.x += (right ? -1 : 1) * GameManager.Instance.LevelWidth;
            xDifference = Vector2.Distance(Vector2.right * newCameraPosition.x, Vector2.right * PlayerPos.x);
            right = newCameraPosition.x > PlayerPos.x;
        }

        if (xDifference > 0.25f)
            newCameraPosition.x += (0.25f - xDifference - 0.01f) * (right ? 1 : -1);

        // Lagging camera movements
        Vector3 targetPosition = newCameraPosition;
        if (controller.IsOnGround)
            LastFloorHeight = PlayerPos.y;
        bool validFloor = controller.IsOnGround || LastFloorHeight < PlayerPos.y;

        // Top camera clip ON GROUND. slowly pan up, dont do it instantly.
        if (validFloor && LastFloorHeight - (newCameraPosition.y + vOrtho) + cameraTopMax + 1.5f > 0)
            targetPosition.y = PlayerPos.y - vOrtho + cameraTopMax + 1.5f;

        // Smoothing
        Vector3 smoothDamp = SmoothDampVel;
        targetPosition = Vector3.SmoothDamp(newCameraPosition, targetPosition, ref smoothDamp, 0.5f, float.MaxValue, Runner.DeltaTime * delta);
        SmoothDampVel = smoothDamp;

        // Clamping to within level bounds
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX + xOrtho, maxX - xOrtho);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY + vOrtho, heightY == 0 ? (minY + vOrtho) : (minY + heightY - vOrtho));

        // Z preservation
        targetPosition.z = -10;

        return targetPosition;
    }

    //---Helpers
    private static Vector2 AntiJitter(Vector3 vec) {
        vec.y = ((int) (vec.y * 100)) * 0.01f;
        return vec;
    }

    //---Debug
#if UNITY_EDITOR
    private static Vector3 HalfRight = Vector3.right * 0.5f;
    public void OnDrawGizmos() {
        if (!controller || !controller.Object)
            return;

        Gizmos.color = Color.blue;
        Vector2 threshold = controller.IsOnGround ? GroundedThreshold : AirThreshold;
        Gizmos.DrawWireCube(PlayerPos, threshold * 2);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new(PlayerPos.x, LastFloorHeight), HalfRight);
    }
#endif
}
