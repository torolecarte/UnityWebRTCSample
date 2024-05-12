using System;
using Newtonsoft.Json;

[Serializable]
public class SessionDescription : BaseJsonObject<SessionDescription>
{
    public string SessionType { get; set; }
    public string Sdp { get; set; }
}

