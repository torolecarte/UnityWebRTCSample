using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class SimpleDataChannelService : WebSocketBehavior
{
    protected override void OnOpen()
    {
        Debug.Log("SERVER SimpleDataChannelService started!");
        base.OnOpen();
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        Debug.Log(ID + " - SERVER DataChannel got message " + e.Data);

        Parallel.ForEach(Sessions.ActiveIDs, id =>
        {
            if (id == ID)
                return;

            Debug.Log($"SERVER Sending message to {id} with message {e.Data}");
            Sessions.SendTo(e.Data, id);
        });

        Debug.Log($"SERVER Finished sending messages.");
        base.OnMessage(e);
    }
}
