using System.Collections;
using System.Collections.Generic;
using Unity.WebRTC;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using WebSocketSharp;

public class SimpleDataChannelSender : MonoBehaviour
{
    [SerializeField]
    private bool sendMessageViaChannel = false;

    private RTCPeerConnection peerConnection;
    private RTCDataChannel dataChannel;

    private WebSocket webSocket;
    private string clientId;

    private bool hasReceivedAnswer = false;
    private SessionDescription receivedAnswerSessionDescTemp;

    private string createOfferCoroutineName = "CreateOffer";
    private string setRemoteDescCoroutineName = "SetRemoteDesc";

    private void Start()
    {
        InitClient("192.168.18.43", 8080);
    }

    private void Update()
    {
        if (hasReceivedAnswer)
        {
            hasReceivedAnswer = !hasReceivedAnswer;
            StartCoroutine(setRemoteDescCoroutineName);
        }

        if (sendMessageViaChannel)
        {
            sendMessageViaChannel = false;
            Debug.Log($"SENDER {clientId} SendMessageViaChannel Enabled sent test message.");
            dataChannel.Send("TEST TEST TEST");
        }
    }

    private void OnDestroy()
    {
        dataChannel.Close();
        peerConnection.Close();
    }

    private void InitClient(string serverIp, int serverPort)
    {
        int port = serverPort == 0 ? 8080 : serverPort;
        clientId = gameObject.name;
        
        var serverAddress = $"ws://{serverIp}:{port}/{nameof(SimpleDataChannelService)}";
        Debug.Log($"SENDER trying to connect to {serverAddress}");
        webSocket = new WebSocket(serverAddress);
        webSocket.OnMessage += WebSocket_OnMessage;
        webSocket.Connect();

        if (webSocket.IsAlive)
            Debug.Log($"SENDER connected to {serverAddress}");

        peerConnection = new RTCPeerConnection();
        peerConnection.OnIceCandidate = candidate =>
        {
            var candidateInit = new CandidateInit
            {
                Candidate = candidate.Candidate,
                SdpMid = candidate.SdpMid,
                SdpMLineIndex = candidate.SdpMLineIndex ?? 0
            };

            var json = candidateInit.ConvertToJSON();
            Debug.Log($"SENDER {clientId} InitClient sent: {json}");
            webSocket.Send("CANDIDATE!" + json);
        };
        peerConnection.OnIceConnectionChange = state => Debug.Log(state);
        peerConnection.OnNegotiationNeeded = () => StartCoroutine(createOfferCoroutineName);

        dataChannel = peerConnection.CreateDataChannel("sendChannel");
        dataChannel.OnOpen = () => Debug.Log("SENDER Data channel opened");
        dataChannel.OnClose = () => Debug.Log("SENDER Data channel closed");
    }

    // Delegates
    private void WebSocket_OnMessage(object sender, MessageEventArgs e)
    {
        var requestArray = e.Data.Split('!');
        var requestType = requestArray[0];
        var requestData = requestArray[1];

        switch (requestType)
        {
            case "ANSWER":
                Debug.Log($"SENDER got ANSWER from {requestData}");
                receivedAnswerSessionDescTemp = SessionDescription.FromJSON(requestData);
                hasReceivedAnswer = true;
                break;
            case "CANDIDATE":
                Debug.Log($"SENDER got CANDIDATE from {requestData}");
                var candidateInit = CandidateInit.FromJSON(requestData);

                var rtcCandidateInit = new RTCIceCandidateInit
                {
                    candidate = candidateInit.Candidate,
                    sdpMid = candidateInit.SdpMid,
                    sdpMLineIndex = candidateInit.SdpMLineIndex
                };
                var rtcCandidate = new RTCIceCandidate(rtcCandidateInit);
                peerConnection.AddIceCandidate(rtcCandidate);
                break;
            default:
                Debug.Log($"SENDER got unknown message: {e.Data}");
                break;
        }
    }


    // CoRoutines
    private IEnumerator CreateOffer()
    {
        var offer = peerConnection.CreateOffer();
        yield return offer;

        var offerDesc = offer.Desc;
        var localDesc = peerConnection.SetLocalDescription(ref offerDesc);
        yield return localDesc;

        var offerSessionDesc = new SessionDescription
        {
            Sdp = offerDesc.sdp,
            SessionType = offerDesc.type.ToString()
        };
        var json = offerSessionDesc.ConvertToJSON();
        Debug.Log($"SENDER {clientId} CreateOffer sent: {json}");
        webSocket.Send("OFFER!" + json);
    }
    private IEnumerator SetRemoteDesc()
    {
        var answerSessionDesc = new RTCSessionDescription()
        {
            type = RTCSdpType.Answer,
            sdp = receivedAnswerSessionDescTemp.Sdp
        };

        var remoteDesc = peerConnection.SetRemoteDescription(ref answerSessionDesc);
        yield return remoteDesc;
    }

}
