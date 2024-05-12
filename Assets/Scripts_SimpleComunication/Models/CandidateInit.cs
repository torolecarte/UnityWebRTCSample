using Newtonsoft.Json;
using System;
using UnityEngine;

[Serializable]
public class CandidateInit : BaseJsonObject<CandidateInit>
{
    public string SdpMid { get; set; }
    public int SdpMLineIndex { get; set; }
    public string Candidate { get; set; }
}
