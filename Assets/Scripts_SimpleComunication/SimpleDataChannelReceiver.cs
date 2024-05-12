using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.WebRTC;
using UnityEngine;
using WebSocketSharp;

public class SimpleDataChannelReceiver : MonoBehaviour
{
    private RTCPeerConnection peerConnection;
    private RTCDataChannel dataChannel;

    private WebSocket webSocket;
    private string clientId;

    private bool hasReceivedOffer = false;
    private SessionDescription receivedOfferSessionDescTemp;

    private string createAnswerCoroutineName = "CreateAnswer";

    void Start()
    {
        Debug.Log("CLIENT IS STARTED");
        InitClient("192.168.18.43", 8080);
    }
    private void Update()
    {
        if (hasReceivedOffer)
        {
            hasReceivedOffer = !hasReceivedOffer;
            StartCoroutine(createAnswerCoroutineName);
        }
    }
    private void OnDestroy()
    {
        dataChannel.Close();
        peerConnection.Close();
    }

    public void InitClient(string serverIp, int serverPort)
    {
        int port = serverPort == 0 ? 8080 : serverPort;
        clientId = gameObject.name;

        var serverAddress = $"ws://{serverIp}:{port}/{nameof(SimpleDataChannelService)}";
        Debug.Log($"CLIENT trying to connect to {serverAddress}");
        webSocket = new WebSocket(serverAddress);
        webSocket.OnMessage += WebSocket_OnMessage;
        webSocket.Connect();

        if (webSocket.IsAlive)
            Debug.Log($"CLIENT connected to {serverAddress}");

        peerConnection = new RTCPeerConnection();
        peerConnection.OnIceCandidate = candidate =>
        {
            Debug.Log($"CLIENT OnIceCandidate ClientId: {clientId}");

            var candidateInit = new CandidateInit()
            {
                SdpMid = candidate.SdpMid,
                SdpMLineIndex = candidate.SdpMLineIndex ?? 0,
                Candidate = candidate.Candidate
            };

            var json = candidateInit.ConvertToJSON();
            Debug.Log($"CLIENT {clientId} OnIceCandidate sent: {json}");
            webSocket.Send("CANDIDATE!" + json);
        };

        peerConnection.OnIceConnectionChange = state => Debug.Log(state);

        peerConnection.OnDataChannel = channel =>
        {
            Debug.Log($"CLIENT OnDataChannel {clientId}");
            dataChannel = channel;
            dataChannel.OnMessage = bytes =>
            {
                var message = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.Log($"CLIENT OnMessage received: {message}");
            };
        };
    }
    public IEnumerator CreateAnswer()
    {
        var offerSessionDesc = new RTCSessionDescription()
        {
            sdp = receivedOfferSessionDescTemp.Sdp,
            type = RTCSdpType.Offer
        };

        var remoteDesc = peerConnection.SetRemoteDescription(ref offerSessionDesc);
        yield return remoteDesc;

        var answer = peerConnection.CreateAnswer();
        yield return answer;

        var answerDesc = answer.Desc;
        var localDesc = peerConnection.SetLocalDescription(ref answerDesc);
        yield return localDesc;

        var answerSessionDesc = new SessionDescription()
        {
            SessionType = answerDesc.type.ToString(),
            Sdp = answerDesc.sdp
        };
        var json = answerSessionDesc.ConvertToJSON();
        Debug.Log($"CLIENT {clientId} CreateAnswer sent: {json}");
        webSocket.Send("ANSWER!" + json);
    }

    public void WebSocket_OnMessage(object sender, MessageEventArgs e)
    {
        var requestArray = e.Data.Split("!");
        var requestType = requestArray[0];
        var requestData = requestArray[1];

        switch (requestType)
        {
            case "OFFER":
                Debug.Log($"CLIENT received OFFER: {requestData}");
                receivedOfferSessionDescTemp = SessionDescription.FromJSON(requestData);
                hasReceivedOffer = true;
                break;
            case "CANDIDATE":
                Debug.Log($"CLIENT received CANDIDATE: {requestData}");
                var candidateInit = CandidateInit.FromJSON(requestData);

                var rctCandidateInit = new RTCIceCandidateInit()
                {
                    sdpMid = candidateInit.SdpMid,
                    sdpMLineIndex = candidateInit.SdpMLineIndex,
                    candidate = candidateInit.Candidate
                };
                var rtcCandidate = new RTCIceCandidate(rctCandidateInit);

                peerConnection.AddIceCandidate(rtcCandidate);
                break;
            default:
                Debug.Log($"CLIENT received unknown request: {e.Data}");
                break;
        }
    }
}
