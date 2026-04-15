using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Grasshopper.Kernel;

namespace GhClickHeatmap.Services
{
  [SupportedOSPlatform("windows")]
  public static class UsabilityReviewService
  {
    private static ReviewSnapshot _snapshot = ReviewSnapshot.Empty;

    public static bool OverlayEnabled { get; set; } = false;

    public static int MinimumClicksToDraw { get; set; } = 1;

    public static bool ShowLabels { get; set; } = true;

    public static ReviewSnapshot Snapshot => _snapshot ?? ReviewSnapshot.Empty;

    public static string LogRootPath
    {
      get { return PluginPaths.GetLogRootPath(); }
      set { PluginPaths.ConfigureLogRootPath(value); }
    }

    public static ReviewSnapshot Reload()
    {
      try
      {
        string root = PluginPaths.GetLogRootPath();
        if (string.IsNullOrWhiteSpace(root))
        {
          _snapshot = ReviewSnapshot.Empty;
          _snapshot.Status = "Log root path is empty.";
          return _snapshot;
        }

        if (!Directory.Exists(root))
        {
          _snapshot = new ReviewSnapshot
          {
            ByObjectId = new Dictionary<string, UsabilityAggregate>(),
            RankedObjects = new List<UsabilityAggregate>(),
            LoadedFileCount = 0,
            LoadedEventCount = 0,
            Status = "Log folder does not exist: " + root
          };
          return _snapshot;
        }

        string[] files = Directory.GetFiles(root, "*.jsonl", SearchOption.AllDirectories);
        Dictionary<string, UsabilityAggregate> byObjectId =
          new Dictionary<string, UsabilityAggregate>(StringComparer.OrdinalIgnoreCase);

        int eventCount = 0;

        for (int i = 0; i < files.Length; i++)
        {
          foreach (string line in File.ReadLines(files[i]))
          {
            if (string.IsNullOrWhiteSpace(line))
              continue;

            UsabilityLogEvent logEvent;
            try
            {
              logEvent = JsonLineSerializer.Deserialize(line);
            }
            catch
            {
              continue;
            }

            if (logEvent == null || string.IsNullOrWhiteSpace(logEvent.ObjectInstanceGuid))
              continue;

            eventCount++;

            if (!byObjectId.TryGetValue(logEvent.ObjectInstanceGuid, out UsabilityAggregate aggregate))
            {
              aggregate = new UsabilityAggregate
              {
                ObjectInstanceGuid = logEvent.ObjectInstanceGuid,
                ComponentGuid = logEvent.ComponentGuid ?? string.Empty,
                ObjectName = !string.IsNullOrWhiteSpace(logEvent.ObjectName) ? logEvent.ObjectName : logEvent.ObjectNickName,
                ObjectNickName = logEvent.ObjectNickName ?? string.Empty
              };

              byObjectId.Add(logEvent.ObjectInstanceGuid, aggregate);
            }

            aggregate.TotalClicks++;
            if (string.Equals(logEvent.Button, "Right", StringComparison.OrdinalIgnoreCase))
              aggregate.RightClicks++;
            else
              aggregate.LeftClicks++;

            aggregate.RegisterUser(logEvent.UserName);
            aggregate.RegisterDocument(logEvent.DocumentPath);

            if (string.IsNullOrWhiteSpace(aggregate.ObjectName) && !string.IsNullOrWhiteSpace(logEvent.ObjectName))
              aggregate.ObjectName = logEvent.ObjectName;
          }
        }

        List<UsabilityAggregate> ranked = byObjectId.Values
          .OrderByDescending(x => x.TotalClicks)
          .ThenBy(x => x.ObjectName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
          .ToList();

        _snapshot = new ReviewSnapshot
        {
          ByObjectId = byObjectId,
          RankedObjects = ranked,
          LoadedFileCount = files.Length,
          LoadedEventCount = eventCount,
          Status = string.Format(
            "Loaded {0} events from {1} log files in {2}.",
            eventCount,
            files.Length,
            root)
        };

        return _snapshot;
      }
      catch (Exception ex)
      {
        _snapshot = new ReviewSnapshot
        {
          ByObjectId = new Dictionary<string, UsabilityAggregate>(),
          RankedObjects = new List<UsabilityAggregate>(),
          LoadedFileCount = 0,
          LoadedEventCount = 0,
          Status = "Review load failed: " + ex.Message
        };
        return _snapshot;
      }
    }

    public static bool TryGetAggregate(IGH_DocumentObject obj, out UsabilityAggregate aggregate)
    {
      aggregate = null!;
      if (obj == null || Snapshot.ByObjectId == null)
        return false;

      string objectId = UsabilityRecorderService.ResolveObjectInstanceGuid(obj);
      if (string.IsNullOrWhiteSpace(objectId))
        return false;

      return Snapshot.ByObjectId.TryGetValue(objectId, out aggregate);
    }
  }
}
