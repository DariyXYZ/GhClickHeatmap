using System.Collections.Generic;

namespace GhClickHeatmap.Services
{
  public sealed class UsabilityAggregate
  {
    private readonly HashSet<string> _users = new HashSet<string>();
    private readonly HashSet<string> _documents = new HashSet<string>();

    public string? ObjectInstanceGuid { get; set; }

    public string? ComponentGuid { get; set; }

    public string? ObjectName { get; set; }

    public string? ObjectNickName { get; set; }

    public int TotalClicks { get; set; }

    public int LeftClicks { get; set; }

    public int RightClicks { get; set; }

    public int UniqueUserCount => _users.Count;

    public int UniqueDocumentCount => _documents.Count;

    public void RegisterUser(string userName)
    {
      if (!string.IsNullOrWhiteSpace(userName))
        _users.Add(userName);
    }

    public void RegisterDocument(string documentPath)
    {
      if (!string.IsNullOrWhiteSpace(documentPath))
        _documents.Add(documentPath);
    }
  }
}
