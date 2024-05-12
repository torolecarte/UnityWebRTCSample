using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SignalingMessageType
{
    OFFER,
    ANSWER,
    CANDIDATE,
    OTHER
}

public class SignalingMessage : BaseJsonObject<SignalingMessage>
{
    public readonly SignalingMessageType Type;
    public readonly string ChannelId;
    public readonly string Message;

    public SignalingMessage(string messageString)
    {
        UnityEngine.Debug.Log($"{nameof(SignalingMessage)} Constructing: {messageString}");

        var messageArray = messageString.Split('!');
        if (messageArray.Length < 3)
        {
            Type = SignalingMessageType.OTHER;
            ChannelId = "";
            Message = messageString;
        }
        else if (Enum.TryParse(messageArray[0], out SignalingMessageType resultType))
        {
            switch (resultType)
            {
                case SignalingMessageType.OFFER:
                case SignalingMessageType.ANSWER:
                case SignalingMessageType.CANDIDATE:
                    Type = resultType;
                    ChannelId = messageArray[1];
                    Message = messageArray[2];
                    break;
                case SignalingMessageType.OTHER:
                default:
                    break;
            }
        }
    }
}