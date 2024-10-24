﻿// ----------------------------------------------------------------------------
// <copyright file="PhotonStreamQueue.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
// Contains the PhotonStreamQueue.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


using System.Collections.Generic;
using UnityEngine;

namespace Photon.Pun
{
    /// <summary>
    ///     The PhotonStreamQueue helps you poll object states at higher frequencies than what
    ///     PhotonNetwork.SendRate dictates and then sends all those states at once when
    ///     Serialize() is called.
    ///     On the receiving end you can call Deserialize() and then the stream will roll out
    ///     the received object states in the same order and timeStep they were recorded in.
    /// </summary>
    public class PhotonStreamQueue
    {
        private readonly List<object> m_Objects = new();
        private readonly int m_SampleRate;
        private bool m_IsWriting;
        private int m_LastFrameCount = -1;

        private float m_LastSampleTime = -Mathf.Infinity;
        private int m_NextObjectIndex = -1;
        private int m_ObjectsPerSample = -1;
        private int m_SampleCount;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PhotonStreamQueue" /> class.
        /// </summary>
        /// <param name="sampleRate">How many times per second should the object states be sampled</param>
        public PhotonStreamQueue(int sampleRate)
        {
            m_SampleRate = sampleRate;
        }

        private void BeginWritePackage()
        {
            //If not enough time has passed since the last sample, we don't want to write anything
            if (Time.realtimeSinceStartup < m_LastSampleTime + 1f / m_SampleRate)
            {
                m_IsWriting = false;
                return;
            }

            if (m_SampleCount == 1)
                m_ObjectsPerSample = m_Objects.Count;
            //Debug.Log( "Setting m_ObjectsPerSample to " + m_ObjectsPerSample );
            else if (m_SampleCount > 1)
                if (m_Objects.Count / m_SampleCount != m_ObjectsPerSample)
                {
                    Debug.LogWarning(
                        "The number of objects sent via a PhotonStreamQueue has to be the same each frame");
                    Debug.LogWarning("Objects in List: " + m_Objects.Count + " / Sample Count: " + m_SampleCount +
                                     " = " + m_Objects.Count / m_SampleCount + " != " + m_ObjectsPerSample);
                }

            m_IsWriting = true;
            m_SampleCount++;
            m_LastSampleTime = Time.realtimeSinceStartup;

            /*if( m_SampleCount  > 1 )
            {
                Debug.Log( "Check: " + m_Objects.Count + " / " + m_SampleCount + " = " + ( m_Objects.Count / m_SampleCount ) + " = " + m_ObjectsPerSample );
            }*/
        }

        /// <summary>
        ///     Resets the PhotonStreamQueue. You need to do this whenever the amount of objects you are observing changes
        /// </summary>
        public void Reset()
        {
            m_SampleCount = 0;
            m_ObjectsPerSample = -1;

            m_LastSampleTime = -Mathf.Infinity;
            m_LastFrameCount = -1;

            m_Objects.Clear();
        }

        /// <summary>
        ///     Adds the next object to the queue. This works just like PhotonStream.SendNext
        /// </summary>
        /// <param name="obj">The object you want to add to the queue</param>
        public void SendNext(object obj)
        {
            if (Time.frameCount != m_LastFrameCount) BeginWritePackage();

            m_LastFrameCount = Time.frameCount;

            if (m_IsWriting == false) return;

            m_Objects.Add(obj);
        }

        /// <summary>
        ///     Determines whether the queue has stored any objects
        /// </summary>
        public bool HasQueuedObjects()
        {
            return m_NextObjectIndex != -1;
        }

        /// <summary>
        ///     Receives the next object from the queue. This works just like PhotonStream.ReceiveNext
        /// </summary>
        /// <returns></returns>
        public object ReceiveNext()
        {
            if (m_NextObjectIndex == -1) return null;

            if (m_NextObjectIndex >= m_Objects.Count) m_NextObjectIndex -= m_ObjectsPerSample;

            return m_Objects[m_NextObjectIndex++];
        }

        /// <summary>
        ///     Serializes the specified stream. Call this in your OnPhotonSerializeView method to send the whole recorded stream.
        /// </summary>
        /// <param name="stream">The PhotonStream you receive as a parameter in OnPhotonSerializeView</param>
        public void Serialize(PhotonStream stream)
        {
            // TODO: find a better solution for this:
            // the "if" is a workaround for packages which have only 1 sample/frame. in that case, SendNext didn't set the obj per sample.
            if (m_Objects.Count > 0 && m_ObjectsPerSample < 0) m_ObjectsPerSample = m_Objects.Count;

            stream.SendNext(m_SampleCount);
            stream.SendNext(m_ObjectsPerSample);

            for (var i = 0; i < m_Objects.Count; ++i) stream.SendNext(m_Objects[i]);

            //Debug.Log( "Serialize " + m_SampleCount + " samples with " + m_ObjectsPerSample + " objects per sample. object count: " + m_Objects.Count + " / " + ( m_SampleCount * m_ObjectsPerSample ) );

            m_Objects.Clear();
            m_SampleCount = 0;
        }

        /// <summary>
        ///     Deserializes the specified stream. Call this in your OnPhotonSerializeView method to receive the whole recorded
        ///     stream.
        /// </summary>
        /// <param name="stream">The PhotonStream you receive as a parameter in OnPhotonSerializeView</param>
        public void Deserialize(PhotonStream stream)
        {
            m_Objects.Clear();

            m_SampleCount = (int)stream.ReceiveNext();
            m_ObjectsPerSample = (int)stream.ReceiveNext();

            for (var i = 0; i < m_SampleCount * m_ObjectsPerSample; ++i) m_Objects.Add(stream.ReceiveNext());

            if (m_Objects.Count > 0)
                m_NextObjectIndex = 0;
            else
                m_NextObjectIndex = -1;

            //Debug.Log( "Deserialized " + m_SampleCount + " samples with " + m_ObjectsPerSample + " objects per sample. object count: " + m_Objects.Count + " / " + ( m_SampleCount * m_ObjectsPerSample ) );
        }
    }
}