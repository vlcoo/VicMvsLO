using System.Collections.Generic;
using System.Linq;
using NSMB.Utils;
using Photon.Pun;
using UnityEngine;

public class CustomNetworkSerializer : MonoBehaviour, IPunObservable
{
    [SerializeField] private List<Component> serializableViews;

    private readonly List<byte> buffer = new();
    private int lastReceivedTimestamp;
    private List<ICustomSerializeView> views;

    public void Awake()
    {
        views = serializableViews.Where(view => view as ICustomSerializeView is not null).Cast<ICustomSerializeView>()
            .ToList();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //dont serialize when game is over
        if (!GameManager.Instance)
            return;

        //clear byte buffer
        buffer.Clear();

        if (stream.IsWriting)
        {
            //write to buffer

            for (byte i = 0; i < views.Count; i++)
            {
                var view = views[i];
                if (!view.Active)
                    continue;

                var bufferSize = buffer.Count;

                view.Serialize(buffer);
                //Debug.Log(view.GetType().Name + " = " + string.Join(",", buffer.Skip(bufferSize)));

                //data was written, write component id header
                if (buffer.Count != bufferSize)
                    buffer.Insert(bufferSize, i);
            }

            var uncompressed = buffer.ToArray();
            /*
            //compression
            buffer.Insert(0, 0);
            uncompressed = buffer.ToArray();
            buffer[0] = 1;
            byte[] compressed = SerializationUtils.Compress(buffer.ToArray());
            if (compressed.Length >= buffer.Count) {
                stream.SendNext(uncompressed);
            } else {
                stream.SendNext(compressed);
            }
            */
            stream.SendNext(uncompressed);
        }
        else if (stream.IsReading)
        {
            //check that packet is coming in order
            var oldTimestamp = lastReceivedTimestamp;
            lastReceivedTimestamp = info.SentServerTimestamp;

            if (info.SentServerTimestamp - oldTimestamp < 0)
                return;

            //incoming bytes
            var bytes = (byte[])stream.ReceiveNext();
            /*
            byte compressed = bytes[0];
            if (bytes[0] == 1)
                bytes = SerializationUtils.Decompress(bytes);
            */

            buffer.AddRange(bytes);

            var index = 0;

            //deserialize
            while (index < buffer.Count)
            {
                SerializationUtils.ReadByte(buffer, ref index, out var view);
                views[view].Deserialize(buffer, ref index, info);
            }
        }
    }
}