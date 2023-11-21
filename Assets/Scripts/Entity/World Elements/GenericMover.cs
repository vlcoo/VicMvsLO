using Photon.Pun;
using UnityEngine;

public class GenericMover : MonoBehaviour
{
    public AnimationCurve x;
    public AnimationCurve y;

    public float animationTimeSeconds = 1;

    private Vector3? origin;
    private double timestamp;

    public void Awake()
    {
        if (origin == null)
            origin = transform.position;
    }

    public void Update()
    {
        var start = GameManager.Instance.startServerTime;

        if (PhotonNetwork.Time <= timestamp)
            timestamp += Time.deltaTime;
        else
            timestamp = (float)PhotonNetwork.Time;

        var time = timestamp - start / (double)1000;
        time /= animationTimeSeconds;
        time %= animationTimeSeconds;

        transform.position = (origin ?? default) + new Vector3(x.Evaluate((float)time), y.Evaluate((float)time), 0);
    }
}