// ----------------------------------------------------------------------------
// <copyright file="PhotonAnimatorView.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Component to synchronize Mecanim animations via PUN.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using UnityEngine;

namespace Photon.Pun
{
    /// <summary>
    ///     This class helps you to synchronize Mecanim animations
    ///     Simply add the component to your GameObject and make sure that
    ///     the PhotonAnimatorView is added to the list of observed components
    /// </summary>
    /// <remarks>
    ///     When Using Trigger Parameters, make sure the component that sets the trigger is higher in the stack of Components
    ///     on the GameObject than 'PhotonAnimatorView'
    ///     Triggers are raised true during one frame only.
    /// </remarks>
    [AddComponentMenu("Photon Networking/Photon Animator View")]
    public class PhotonAnimatorView : MonoBehaviourPun, IPunObservable
    {
        #region Enums

        public enum ParameterType
        {
            Float = 1,
            Int = 3,
            Bool = 4,
            Trigger = 9
        }


        public enum SynchronizeType
        {
            Disabled = 0,
            Discrete = 1,
            Continuous = 2
        }


        [Serializable]
        public class SynchronizedParameter
        {
            public ParameterType Type;
            public SynchronizeType SynchronizeType;
            public string Name;
        }


        [Serializable]
        public class SynchronizedLayer
        {
            public SynchronizeType SynchronizeType;
            public int LayerIndex;
        }

        #endregion


        #region Properties

#if PHOTON_DEVELOP
        public PhotonAnimatorView ReceivingSender;
#endif

        #endregion


        #region Members

        private bool TriggerUsageWarningDone;

        private Animator m_Animator;

        private readonly PhotonStreamQueue m_StreamQueue = new(120);

        //These fields are only used in the CustomEditor for this script and would trigger a
        //"this variable is never used" warning, which I am suppressing here
#pragma warning disable 0414

        [HideInInspector] [SerializeField] private bool ShowLayerWeightsInspector = true;

        [HideInInspector] [SerializeField] private bool ShowParameterInspector = true;

#pragma warning restore 0414

        [HideInInspector] [SerializeField] private List<SynchronizedParameter> m_SynchronizeParameters = new();

        [HideInInspector] [SerializeField] private List<SynchronizedLayer> m_SynchronizeLayers = new();

        private Vector3 m_ReceiverPosition;
        private float m_LastDeserializeTime;
        private bool m_WasSynchronizeTypeChanged = true;

        /// <summary>
        ///     Cached raised triggers that are set to be synchronized in discrete mode. since a Trigger only stay up for less than
        ///     a frame,
        ///     We need to cache it until the next discrete serialization call.
        /// </summary>
        private readonly List<string> m_raisedDiscreteTriggersCache = new();

        #endregion


        #region Unity

        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (m_Animator.applyRootMotion && photonView.IsMine == false && PhotonNetwork.IsConnected)
                m_Animator.applyRootMotion = false;

            if (PhotonNetwork.InRoom == false || PhotonNetwork.CurrentRoom.PlayerCount <= 1)
            {
                m_StreamQueue.Reset();
                return;
            }

            if (photonView.IsMine)
            {
                SerializeDataContinuously();

                CacheDiscreteTriggers();
            }
            else
            {
                DeserializeDataContinuously();
            }
        }

        #endregion


        #region Setup Synchronizing Methods

        /// <summary>
        ///     Caches the discrete triggers values for keeping track of raised triggers, and will be reseted after the sync
        ///     routine got performed
        /// </summary>
        public void CacheDiscreteTriggers()
        {
            for (var i = 0; i < m_SynchronizeParameters.Count; ++i)
            {
                var parameter = m_SynchronizeParameters[i];

                if (parameter.SynchronizeType == SynchronizeType.Discrete && parameter.Type == ParameterType.Trigger &&
                    m_Animator.GetBool(parameter.Name))
                    if (parameter.Type == ParameterType.Trigger)
                    {
                        m_raisedDiscreteTriggersCache.Add(parameter.Name);
                        break;
                    }
            }
        }

        /// <summary>
        ///     Check if a specific layer is configured to be synchronize
        /// </summary>
        /// <param name="layerIndex">Index of the layer.</param>
        /// <returns>True if the layer is synchronized</returns>
        public bool DoesLayerSynchronizeTypeExist(int layerIndex)
        {
            return m_SynchronizeLayers.FindIndex(item => item.LayerIndex == layerIndex) != -1;
        }

        /// <summary>
        ///     Check if the specified parameter is configured to be synchronized
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>True if the parameter is synchronized</returns>
        public bool DoesParameterSynchronizeTypeExist(string name)
        {
            return m_SynchronizeParameters.FindIndex(item => item.Name == name) != -1;
        }

        /// <summary>
        ///     Get a list of all synchronized layers
        /// </summary>
        /// <returns>List of SynchronizedLayer objects</returns>
        public List<SynchronizedLayer> GetSynchronizedLayers()
        {
            return m_SynchronizeLayers;
        }

        /// <summary>
        ///     Get a list of all synchronized parameters
        /// </summary>
        /// <returns>List of SynchronizedParameter objects</returns>
        public List<SynchronizedParameter> GetSynchronizedParameters()
        {
            return m_SynchronizeParameters;
        }

        /// <summary>
        ///     Gets the type how the layer is synchronized
        /// </summary>
        /// <param name="layerIndex">Index of the layer.</param>
        /// <returns>Disabled/Discrete/Continuous</returns>
        public SynchronizeType GetLayerSynchronizeType(int layerIndex)
        {
            var index = m_SynchronizeLayers.FindIndex(item => item.LayerIndex == layerIndex);

            if (index == -1) return SynchronizeType.Disabled;

            return m_SynchronizeLayers[index].SynchronizeType;
        }

        /// <summary>
        ///     Gets the type how the parameter is synchronized
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>Disabled/Discrete/Continuous</returns>
        public SynchronizeType GetParameterSynchronizeType(string name)
        {
            var index = m_SynchronizeParameters.FindIndex(item => item.Name == name);

            if (index == -1) return SynchronizeType.Disabled;

            return m_SynchronizeParameters[index].SynchronizeType;
        }

        /// <summary>
        ///     Sets the how a layer should be synchronized
        /// </summary>
        /// <param name="layerIndex">Index of the layer.</param>
        /// <param name="synchronizeType">Disabled/Discrete/Continuous</param>
        public void SetLayerSynchronized(int layerIndex, SynchronizeType synchronizeType)
        {
            if (Application.isPlaying) m_WasSynchronizeTypeChanged = true;

            var index = m_SynchronizeLayers.FindIndex(item => item.LayerIndex == layerIndex);

            if (index == -1)
                m_SynchronizeLayers.Add(new SynchronizedLayer
                    { LayerIndex = layerIndex, SynchronizeType = synchronizeType });
            else
                m_SynchronizeLayers[index].SynchronizeType = synchronizeType;
        }

        /// <summary>
        ///     Sets the how a parameter should be synchronized
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="type">The type of the parameter.</param>
        /// <param name="synchronizeType">Disabled/Discrete/Continuous</param>
        public void SetParameterSynchronized(string name, ParameterType type, SynchronizeType synchronizeType)
        {
            if (Application.isPlaying) m_WasSynchronizeTypeChanged = true;

            var index = m_SynchronizeParameters.FindIndex(item => item.Name == name);

            if (index == -1)
                m_SynchronizeParameters.Add(new SynchronizedParameter
                    { Name = name, Type = type, SynchronizeType = synchronizeType });
            else
                m_SynchronizeParameters[index].SynchronizeType = synchronizeType;
        }

        #endregion


        #region Serialization

        private void SerializeDataContinuously()
        {
            if (m_Animator == null) return;

            for (var i = 0; i < m_SynchronizeLayers.Count; ++i)
                if (m_SynchronizeLayers[i].SynchronizeType == SynchronizeType.Continuous)
                    m_StreamQueue.SendNext(m_Animator.GetLayerWeight(m_SynchronizeLayers[i].LayerIndex));

            for (var i = 0; i < m_SynchronizeParameters.Count; ++i)
            {
                var parameter = m_SynchronizeParameters[i];

                if (parameter.SynchronizeType == SynchronizeType.Continuous)
                    switch (parameter.Type)
                    {
                        case ParameterType.Bool:
                            m_StreamQueue.SendNext(m_Animator.GetBool(parameter.Name));
                            break;
                        case ParameterType.Float:
                            m_StreamQueue.SendNext(m_Animator.GetFloat(parameter.Name));
                            break;
                        case ParameterType.Int:
                            m_StreamQueue.SendNext(m_Animator.GetInteger(parameter.Name));
                            break;
                        case ParameterType.Trigger:
                            if (!TriggerUsageWarningDone)
                            {
                                TriggerUsageWarningDone = true;
                                Debug.Log(
                                    "PhotonAnimatorView: When using triggers, make sure this component is last in the stack.\n" +
                                    "If you still experience issues, implement triggers as a regular RPC \n" +
                                    "or in custom IPunObservable component instead", this);
                            }

                            m_StreamQueue.SendNext(m_Animator.GetBool(parameter.Name));
                            break;
                    }
            }
        }


        private void DeserializeDataContinuously()
        {
            if (m_StreamQueue.HasQueuedObjects() == false) return;

            for (var i = 0; i < m_SynchronizeLayers.Count; ++i)
                if (m_SynchronizeLayers[i].SynchronizeType == SynchronizeType.Continuous)
                    m_Animator.SetLayerWeight(m_SynchronizeLayers[i].LayerIndex, (float)m_StreamQueue.ReceiveNext());

            for (var i = 0; i < m_SynchronizeParameters.Count; ++i)
            {
                var parameter = m_SynchronizeParameters[i];

                if (parameter.SynchronizeType == SynchronizeType.Continuous)
                    switch (parameter.Type)
                    {
                        case ParameterType.Bool:
                            m_Animator.SetBool(parameter.Name, (bool)m_StreamQueue.ReceiveNext());
                            break;
                        case ParameterType.Float:
                            m_Animator.SetFloat(parameter.Name, (float)m_StreamQueue.ReceiveNext());
                            break;
                        case ParameterType.Int:
                            m_Animator.SetInteger(parameter.Name, (int)m_StreamQueue.ReceiveNext());
                            break;
                        case ParameterType.Trigger:
                            m_Animator.SetBool(parameter.Name, (bool)m_StreamQueue.ReceiveNext());
                            break;
                    }
            }
        }

        private void SerializeDataDiscretly(PhotonStream stream)
        {
            for (var i = 0; i < m_SynchronizeLayers.Count; ++i)
                if (m_SynchronizeLayers[i].SynchronizeType == SynchronizeType.Discrete)
                    stream.SendNext(m_Animator.GetLayerWeight(m_SynchronizeLayers[i].LayerIndex));

            for (var i = 0; i < m_SynchronizeParameters.Count; ++i)
            {
                var parameter = m_SynchronizeParameters[i];

                if (parameter.SynchronizeType == SynchronizeType.Discrete)
                    switch (parameter.Type)
                    {
                        case ParameterType.Bool:
                            stream.SendNext(m_Animator.GetBool(parameter.Name));
                            break;
                        case ParameterType.Float:
                            stream.SendNext(m_Animator.GetFloat(parameter.Name));
                            break;
                        case ParameterType.Int:
                            stream.SendNext(m_Animator.GetInteger(parameter.Name));
                            break;
                        case ParameterType.Trigger:
                            if (!TriggerUsageWarningDone)
                            {
                                TriggerUsageWarningDone = true;
                                Debug.Log(
                                    "PhotonAnimatorView: When using triggers, make sure this component is last in the stack.\n" +
                                    "If you still experience issues, implement triggers as a regular RPC \n" +
                                    "or in custom IPunObservable component instead", this);
                            }

                            // here we can't rely on the current real state of the trigger, we might have missed its raise
                            stream.SendNext(m_raisedDiscreteTriggersCache.Contains(parameter.Name));
                            break;
                    }
            }

            // reset the cache, we've synchronized.
            m_raisedDiscreteTriggersCache.Clear();
        }

        private void DeserializeDataDiscretly(PhotonStream stream)
        {
            for (var i = 0; i < m_SynchronizeLayers.Count; ++i)
                if (m_SynchronizeLayers[i].SynchronizeType == SynchronizeType.Discrete)
                    m_Animator.SetLayerWeight(m_SynchronizeLayers[i].LayerIndex, (float)stream.ReceiveNext());

            for (var i = 0; i < m_SynchronizeParameters.Count; ++i)
            {
                var parameter = m_SynchronizeParameters[i];

                if (parameter.SynchronizeType == SynchronizeType.Discrete)
                    switch (parameter.Type)
                    {
                        case ParameterType.Bool:
                            if (stream.PeekNext() is bool == false) return;
                            m_Animator.SetBool(parameter.Name, (bool)stream.ReceiveNext());
                            break;
                        case ParameterType.Float:
                            if (stream.PeekNext() is float == false) return;

                            m_Animator.SetFloat(parameter.Name, (float)stream.ReceiveNext());
                            break;
                        case ParameterType.Int:
                            if (stream.PeekNext() is int == false) return;

                            m_Animator.SetInteger(parameter.Name, (int)stream.ReceiveNext());
                            break;
                        case ParameterType.Trigger:
                            if (stream.PeekNext() is bool == false) return;

                            if ((bool)stream.ReceiveNext()) m_Animator.SetTrigger(parameter.Name);
                            break;
                    }
            }
        }

        private void SerializeSynchronizationTypeState(PhotonStream stream)
        {
            var states = new byte[m_SynchronizeLayers.Count + m_SynchronizeParameters.Count];

            for (var i = 0; i < m_SynchronizeLayers.Count; ++i)
                states[i] = (byte)m_SynchronizeLayers[i].SynchronizeType;

            for (var i = 0; i < m_SynchronizeParameters.Count; ++i)
                states[m_SynchronizeLayers.Count + i] = (byte)m_SynchronizeParameters[i].SynchronizeType;

            stream.SendNext(states);
        }

        private void DeserializeSynchronizationTypeState(PhotonStream stream)
        {
            var state = (byte[])stream.ReceiveNext();

            for (var i = 0; i < m_SynchronizeLayers.Count; ++i)
                m_SynchronizeLayers[i].SynchronizeType = (SynchronizeType)state[i];

            for (var i = 0; i < m_SynchronizeParameters.Count; ++i)
                m_SynchronizeParameters[i].SynchronizeType = (SynchronizeType)state[m_SynchronizeLayers.Count + i];
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (m_Animator == null) return;

            if (stream.IsWriting)
            {
                if (m_WasSynchronizeTypeChanged)
                {
                    m_StreamQueue.Reset();
                    SerializeSynchronizationTypeState(stream);

                    m_WasSynchronizeTypeChanged = false;
                }

                m_StreamQueue.Serialize(stream);
                SerializeDataDiscretly(stream);
            }
            else
            {
#if PHOTON_DEVELOP
                if( ReceivingSender != null )
                {
                    ReceivingSender.OnPhotonSerializeView( stream, info );
                }
                else
#endif
                {
                    if (stream.PeekNext() is byte[]) DeserializeSynchronizationTypeState(stream);

                    m_StreamQueue.Deserialize(stream);
                    DeserializeDataDiscretly(stream);
                }
            }
        }

        #endregion
    }
}