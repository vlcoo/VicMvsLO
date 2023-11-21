// ----------------------------------------------------------------------------
// <copyright file="PhotonTransformView.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Component to synchronize Transforms via PUN PhotonView.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


using UnityEngine;

namespace Photon.Pun
{
    [AddComponentMenu("Photon Networking/Photon Transform View")]
    [HelpURL("https://doc.photonengine.com/en-us/pun/v2/gameplay/synchronization-and-state")]
    public class PhotonTransformView : MonoBehaviourPun, IPunObservable
    {
        public bool m_SynchronizePosition = true;
        public bool m_SynchronizeRotation = true;
        public bool m_SynchronizeScale;

        [Tooltip(
            "Indicates if localPosition and localRotation should be used. Scale ignores this setting, and always uses localScale to avoid issues with lossyScale.")]
        public bool m_UseLocal;

        private float m_Angle;

        private Vector3 m_Direction;
        private float m_Distance;

        private bool m_firstTake;
        private Vector3 m_NetworkPosition;

        private Quaternion m_NetworkRotation;
        private Vector3 m_StoredPosition;

        public void Awake()
        {
            m_StoredPosition = transform.localPosition;
            m_NetworkPosition = Vector3.zero;

            m_NetworkRotation = Quaternion.identity;
        }

        private void Reset()
        {
            // Only default to true with new instances. useLocal will remain false for old projects that are updating PUN.
            m_UseLocal = true;
        }

        public void Update()
        {
            var tr = transform;

            if (!photonView.IsMine)
            {
                if (m_UseLocal)

                {
                    tr.localPosition = Vector3.MoveTowards(tr.localPosition, m_NetworkPosition,
                        m_Distance * Time.deltaTime * PhotonNetwork.SerializationRate);
                    tr.localRotation = Quaternion.RotateTowards(tr.localRotation, m_NetworkRotation,
                        m_Angle * Time.deltaTime * PhotonNetwork.SerializationRate);
                }
                else
                {
                    tr.position = Vector3.MoveTowards(tr.position, m_NetworkPosition,
                        m_Distance * Time.deltaTime * PhotonNetwork.SerializationRate);
                    tr.rotation = Quaternion.RotateTowards(tr.rotation, m_NetworkRotation,
                        m_Angle * Time.deltaTime * PhotonNetwork.SerializationRate);
                }
            }
        }

        private void OnEnable()
        {
            m_firstTake = true;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            var tr = transform;

            // Write
            if (stream.IsWriting)
            {
                if (m_SynchronizePosition)
                {
                    if (m_UseLocal)
                    {
                        m_Direction = tr.localPosition - m_StoredPosition;
                        m_StoredPosition = tr.localPosition;
                        stream.SendNext(tr.localPosition);
                        stream.SendNext(m_Direction);
                    }
                    else
                    {
                        m_Direction = tr.position - m_StoredPosition;
                        m_StoredPosition = tr.position;
                        stream.SendNext(tr.position);
                        stream.SendNext(m_Direction);
                    }
                }

                if (m_SynchronizeRotation)
                {
                    if (m_UseLocal)
                        stream.SendNext(tr.localRotation);
                    else
                        stream.SendNext(tr.rotation);
                }

                if (m_SynchronizeScale) stream.SendNext(tr.localScale);
            }
            // Read
            else
            {
                if (m_SynchronizePosition)
                {
                    m_NetworkPosition = (Vector3)stream.ReceiveNext();
                    m_Direction = (Vector3)stream.ReceiveNext();

                    if (m_firstTake)
                    {
                        if (m_UseLocal)
                            tr.localPosition = m_NetworkPosition;
                        else
                            tr.position = m_NetworkPosition;

                        m_Distance = 0f;
                    }
                    else
                    {
                        var lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                        m_NetworkPosition += m_Direction * lag;
                        if (m_UseLocal)
                            m_Distance = Vector3.Distance(tr.localPosition, m_NetworkPosition);
                        else
                            m_Distance = Vector3.Distance(tr.position, m_NetworkPosition);
                    }
                }

                if (m_SynchronizeRotation)
                {
                    m_NetworkRotation = (Quaternion)stream.ReceiveNext();

                    if (m_firstTake)
                    {
                        m_Angle = 0f;

                        if (m_UseLocal)
                            tr.localRotation = m_NetworkRotation;
                        else
                            tr.rotation = m_NetworkRotation;
                    }
                    else
                    {
                        if (m_UseLocal)
                            m_Angle = Quaternion.Angle(tr.localRotation, m_NetworkRotation);
                        else
                            m_Angle = Quaternion.Angle(tr.rotation, m_NetworkRotation);
                    }
                }

                if (m_SynchronizeScale) tr.localScale = (Vector3)stream.ReceiveNext();

                if (m_firstTake) m_firstTake = false;
            }
        }
    }
}