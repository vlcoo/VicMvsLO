using System.Collections.Generic;
using NSMB.Utils;
using Photon.Pun;
using UnityEngine;

public class CustomAnimatorSerializer : MonoBehaviour, ICustomSerializeView
{
    [SerializeField] private Animator animator;

    [SerializeField] private int layerIndex;

    private float lastSendTimestamp;
    private AnimatorStateInfo? lastSentState;

    public bool Active { get; set; }

    public void Deserialize(List<byte> buffer, ref int index, PhotonMessageInfo info)
    {
        SerializationUtils.ReadInt(buffer, ref index, out int stateHash);
        SerializationUtils.UnpackFromByte(buffer, ref index, 0, 1, out var normalizedTime);

        /*
        float lag = (float) (PhotonNetwork.Time - info.SentServerTime);
        normalizedTime += lag;
        */

        animator.Play(stateHash, layerIndex, normalizedTime);
    }

    public void Serialize(List<byte> buffer)
    {
        if (lastSentState != null || PhotonNetwork.Time - lastSendTimestamp < 1000)
            //don't send anything
            return;

        var currentState = animator.GetCurrentAnimatorStateInfo(layerIndex);
        SerializationUtils.WriteInt(buffer, currentState.fullPathHash);
        SerializationUtils.PackToByte(buffer, currentState.normalizedTime, 0, 1);

        lastSendTimestamp = (float)PhotonNetwork.Time;
    }
}