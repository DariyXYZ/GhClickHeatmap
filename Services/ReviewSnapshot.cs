using System.Collections.Generic;

namespace GhClickHeatmap.Services
{
  public sealed class ReviewSnapshot
  {
    public static readonly ReviewSnapshot Empty = new ReviewSnapshot();

    public IReadOnlyDictionary<string, UsabilityAggregate>? ByObjectId { get; set; }

    public IReadOnlyList<UsabilityAggregate>? RankedObjects { get; set; }

    public int LoadedFileCount { get; set; }

    public int LoadedEventCount { get; set; }

    public string? Status { get; set; }
  }
}
