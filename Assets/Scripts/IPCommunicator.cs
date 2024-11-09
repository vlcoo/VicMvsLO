using System.Collections.Generic;
using Newtonsoft.Json;
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
    public string LastIncomingMessage { get; private set; } = "";

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
            args.Response = HandleIncomingMessage(args.Request);
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
        // the response indicates if the message was accepted.
        // should be GenericAccept in most cases (only that will be deemed as "accepted").
        // Can be GenericReject to signify an unknown error, or a different string for a specific error.
        var response = client.Send(message);
        if (response is null)
        {
            Debug.LogError($"Other end never responded!");
        }

        if (response != Enums.IpcMessages.GenericAccept)
        {
            Debug.LogError($"Other end rejected the message!");
            if (response != Enums.IpcMessages.GenericReject)
            {
                Debug.LogError($"...this is what it had to say: {response}");
            }
        }
    }

    private string HandleIncomingMessage(string request)
    {
        var response = "";

        // check for specific messages in specific situations.
        if (LastIncomingMessage == Enums.IpcMessages.BeginLevelContentsTransmission)
        {
            // level transmission began last message. this one must contain a stringified json of the contents.
            var contents = JsonConvert.DeserializeObject<Dictionary<string, Object>>(request);
            if (contents is null)
            {
                response = Enums.IpcMessages.GenericReject;
                Debug.Log(request);
            }
            // TODO: give the contents dict to the GameManager to process.
        }

        if (response != "")
        {
            LastIncomingMessage = request;
            return response;
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
                Debug.Log($"CheckHeakth requested. Result: {response}");
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

        LastIncomingMessage = request;
        return response;
    }
}
