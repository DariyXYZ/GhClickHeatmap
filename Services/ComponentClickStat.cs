using System;

namespace GhClickHeatmap.Services
{
  public sealed class ComponentClickStat
  {
    public Guid ComponentId { get; set; }

    public string Name { get; set; }

    public int TotalClicks { get; set; }

    public int LeftClicks { get; set; }

    public int RightClicks { get; set; }

    public DateTime LastClickedUtc { get; set; }
  }
}
