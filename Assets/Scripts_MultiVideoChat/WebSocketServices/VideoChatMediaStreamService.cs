using System.Collections.Generic;
using System;
using WebSocketSharp.Server;
using WebSocketSharp;
using System.Linq;

public class VideoChatMediaStreamService : WebSocketBehavior
{
    private static List<string> connections = new List<string>()
    {
        "01", "10"
    };

    private static int connectionCounter = 0;

    protected override void OnOpen()
    {
        UnityEngine.Debug.Log($"{nameof(VideoChatMediaStreamService)} OnOpen ID: {ID} | ConnCounter: {connectionCounter}");
        // send clientID
        Sessions.SendTo(connectionCounter.ToString(), ID);
        connectionCounter++;

        // send all connected users
        Sessions.SendTo(String.Join("|", connections), ID);
        UnityEngine.Debug.Log($"{nameof(VideoChatMediaStreamService)} OnOpen sent message to ID: {ID}");
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        UnityEngine.Debug.Log($"{nameof(VideoChatMediaStreamService)} OnMessage forwarding messages to {Sessions.ActiveIDs.Count() - 1} client(s)");

        // forward messages to all other clients
        foreach (var id in Sessions.ActiveIDs)
        {
            if (id != ID)
            {
                Sessions.SendTo(e.Data, id);
            }
        }
    }
}