using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using WebSocketSharp.Server;

public class VideoChatWebSocketServer : MonoBehaviour
{
    private WebSocketServer wssv;
    private string serverIPv4Address;
    private int serverPort = 8080;

    private void Awake()
    {
        // get server ip in network
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                serverIPv4Address = ip.ToString();
                break;
            }
        }

        wssv = new WebSocketServer($"ws://{serverIPv4Address}:{serverPort}");
        wssv.AddWebSocketService<VideoChatMediaStreamService>($"/{nameof(VideoChatMediaStreamService)}");
        wssv.Start();
        Debug.Log($"SERVER Started at {serverIPv4Address}");
    }

    private void OnDestroy()
    {
        wssv.Stop();
    }
}