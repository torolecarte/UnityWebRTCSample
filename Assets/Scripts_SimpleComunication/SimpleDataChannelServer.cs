using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using WebSocketSharp.Server;

public class SimpleDataChannel : MonoBehaviour
{
    private WebSocketServer webSocketServer;
    private string serverIpv4Address;
    private int serverPort = 8080;

    private void Awake()
    {
        Debug.Log("SERVER IS AWAKE");
        // get server ip in network
        var host = Dns.GetHostEntry(Dns.GetHostName());

        IPAddress hostIp = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

        if (hostIp is null)
        {
            Debug.Log("SERVER could not find an IP to host.");
            return;
        }

        serverIpv4Address = hostIp.ToString();
        Debug.Log($"SERVER starting at {serverIpv4Address}");

        var serverAddress = $"ws://{serverIpv4Address}:{serverPort}";
        webSocketServer = new WebSocketServer(serverAddress);
        Debug.Log($"SERVER started at {serverAddress}");

        webSocketServer.AddWebSocketService<SimpleDataChannelService>($"/{nameof(SimpleDataChannelService)}");
        webSocketServer.Start();
        Debug.Log($"SERVER included WebSocketService {nameof(SimpleDataChannelService)}");
    }
}
