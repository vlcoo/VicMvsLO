using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using NSMB.Utils;
using PimDeWitte.UnityMainThreadDispatcher;
using UnityEngine;
using ZetaIpc.Runtime.Client;
using ZetaIpc.Runtime.Server;

// The "IP" stands for "Inter-Process".
public class IPCommunicator : MonoBehaviour
{
    public enum ConnectionStatus
    {
        Failed = -1,
        Disconnected = 0,
        Connected = 1
    }

    private const int PortPlayer = 45001; // our port
    private const int PortEditor = 45002; // the other's port

    private IpcServer server;
    private IpcClient client;

    public ConnectionStatus Status { get; private set; } = ConnectionStatus.Disconnected;
    public string LastIncomingMessage = "";

    public string LastIncomingMessage1
    {
        get => LastIncomingMessage;
        set
        {
            UnityMainThreadDispatcher.Instance().Enqueue(HandleIncomingMessage(value));
            LastIncomingMessage = value;
        }
    }

    private void Start()
    {
        BeginServer();
        BeginClient();
        Status = ConnectionStatus.Connected;
    }

    private void BeginServer()
    {
        server = new IpcServer();
        server.Start(PortPlayer);
        server.ReceivedRequest += (sender, args) =>
        {
            LastIncomingMessage1 = args.Request;
            args.Response = Enums.IpcMessages.GenericAccept;
            args.Handled = true;
        };
    }

    private void BeginClient()
    {
        client = new IpcClient();
        client.Initialize(PortEditor);
    }

    public void SendOutgoingMessage(string message)
    {
        var response = client.Send(message);
        if (response is null)
        {
            Debug.LogError($"Other end never responded!");
            return;
        }

        if (response == Enums.IpcMessages.GenericAccept)
        {
        }

        else if (response == Enums.IpcMessages.GenericReject)
        {
            Debug.LogError($"Other end rejected the message!");
        }
    }

    private IEnumerator HandleIncomingMessage(string request)
    {
        Debug.Log(request);
        var response = "";

        // check for specific messages in specific situations.
        if (request.StartsWith("{"))
        {
            // level transmission began last message. this one must contain a stringified json of the contents.
            Debug.Log("deserializing lvl!");
            var level_dict = Utils.DeserializeNestedJson(request);
            if (level_dict == null)
            {
                response = Enums.IpcMessages.GenericReject;
                request = Enums.IpcMessages.GenericReject;
                Debug.LogError("failure...");
            }
            else
            {
                response = Enums.IpcMessages.GenericAccept;
                request = Enums.IpcMessages.GenericAccept;  // avoid leaving the level contents in this persistent var.
                // TODO: give the contents dict to the GameManager to process.
                Debug.Log("success!");
                if (GameManager.Instance is not null)
                {
                    GameManager.Instance.mvlxTools.BuildLevelFromContents(level_dict.GetValueOrDefault("contents") as Dictionary<string, object>);
                }
            }
        }

        if (response != "")
        {
            // LastIncomingMessage = request;
            Debug.Log(response);
        }

        // all done. check rest of generic messages.
        switch (request)
        {
            case Enums.IpcMessages.GenericAccept:
                response = Enums.IpcMessages.GenericAccept;
                break;

            case Enums.IpcMessages.GenericReject:
                response = Enums.IpcMessages.GenericAccept;
                break;

            case Enums.IpcMessages.CheckHealth:
                // if we received this message, the connection is obviously working.
                // check everything just in case.
                response = client is not null && server is not null
                    ? Enums.IpcMessages.GenericAccept
                    : Enums.IpcMessages.GenericReject;
                Debug.Log($"CheckHealth requested. Result: {response}");
                break;

            case Enums.IpcMessages.GenericGiveFocus:
                // not implemented in the player.
                response = "focus not implemented in the player";
                break;

            case Enums.IpcMessages.BeginLevelContentsTransmission:
                response = Enums.IpcMessages.GenericAccept;
                break;

            default:
                // unexpected message:
                response = Enums.IpcMessages.GenericReject;
                break;
        }

        // LastIncomingMessage = request;
        yield return null;
    }
}
