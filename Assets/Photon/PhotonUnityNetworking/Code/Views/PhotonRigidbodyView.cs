// ----------------------------------------------------------------------------
// <copyright file="PhotonRigidbodyView.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Component to synchronize rigidbodies via PUN.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


using UnityEngine;

namespace Photon.Pun
{
    [RequireComponent(typeof(Rigidbody))]
    [AddComponentMenu("Photon Networking/Photon Rigidbody View")]
    public class PhotonRigidbodyView : MonoBehaviourPun, IPunObservable
    {
        [HideInInspector] public bool m_SynchronizeVelocity = true;

        [HideInInspector] public bool m_SynchronizeAngularVelocity;

        [HideInInspector] public bool m_TeleportEnabled;

        [HideInInspector] public float m_TeleportIfDistanceGreaterThan = 3.0f;

        private float m_Angle;

        private Rigidbody m_Body;
        private float m_Distance;

        private Vector3 m_NetworkPosition;

        private Quaternion m_NetworkRotation;

        public void Awake()
        {
            m_Body = GetComponent<Rigidbody>();

            m_NetworkPosition = new Vector3();
            m_NetworkRotation = new Quaternion();
        }

        public void FixedUpdate()
        {
            if (!photonView.IsMine)
            {
                m_Body.position = Vector3.MoveTowards(m_Body.position, m_NetworkPosition,
                    m_Distance * (1.0f / PhotonNetwork.SerializationRate));
                m_Body.rotation = Quaternion.RotateTowards(m_Body.rotation, m_NetworkRotation,
                    m_Angle * (1.0f / PhotonNetwork.SerializationRate));
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(m_Body.position);
                stream.SendNext(m_Body.rotation);

                if (m_SynchronizeVelocity) stream.SendNext(m_Body.velocity);

                if (m_SynchronizeAngularVelocity) stream.SendNext(m_Body.angularVelocity);
            }
            else
            {
                m_NetworkPosition = (Vector3)stream.ReceiveNext();
                m_NetworkRotation = (Quaternion)stream.ReceiveNext();

                if (m_TeleportEnabled)
                    if (Vector3.Distance(m_Body.position, m_NetworkPosition) > m_TeleportIfDistanceGreaterThan)
                        m_Body.position = m_NetworkPosition;

                if (m_SynchronizeVelocity || m_SynchronizeAngularVelocity)
                {
                    var lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));

                    if (m_SynchronizeVelocity)
                    {
                        m_Body.velocity = (Vector3)stream.ReceiveNext();

                        m_NetworkPosition += m_Body.velocity * lag;

                        m_Distance = Vector3.Distance(m_Body.position, m_NetworkPosition);
                    }

                    if (m_SynchronizeAngularVelocity)
                    {
                        m_Body.angularVelocity = (Vector3)stream.ReceiveNext();

                        m_NetworkRotation = Quaternion.Euler(m_Body.angularVelocity * lag) * m_NetworkRotation;

                        m_Angle = Quaternion.Angle(m_Body.rotation, m_NetworkRotation);
                    }
                }
            }
        }
    }
}