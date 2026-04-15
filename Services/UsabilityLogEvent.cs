using System.Runtime.Serialization;

namespace GhClickHeatmap.Services
{
  [DataContract]
  public sealed class UsabilityLogEvent
  {
    [DataMember(Order = 1)]
    public string? UtcTimestamp { get; set; }

    [DataMember(Order = 2)]
    public string? SessionId { get; set; }

    [DataMember(Order = 3)]
    public string? UserName { get; set; }

    [DataMember(Order = 4)]
    public string? MachineName { get; set; }

    [DataMember(Order = 5)]
    public string? DocumentPath { get; set; }

    [DataMember(Order = 6)]
    public string? DocumentName { get; set; }

    [DataMember(Order = 7)]
    public string? ObjectInstanceGuid { get; set; }

    [DataMember(Order = 8)]
    public string? ComponentGuid { get; set; }

    [DataMember(Order = 9)]
    public string? ObjectName { get; set; }

    [DataMember(Order = 10)]
    public string? ObjectNickName { get; set; }

    [DataMember(Order = 11)]
    public float CanvasX { get; set; }

    [DataMember(Order = 12)]
    public float CanvasY { get; set; }

    [DataMember(Order = 13)]
    public float Width { get; set; }

    [DataMember(Order = 14)]
    public float Height { get; set; }

    [DataMember(Order = 15)]
    public string? Button { get; set; }
  }
}
