using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Grasshopper.Kernel;

namespace GhClickHeatmap.Services
{
  public sealed class ClickHeatmapStore
  {
    private readonly ConcurrentDictionary<Guid, ComponentClickStat> _stats =
      new ConcurrentDictionary<Guid, ComponentClickStat>();

    public IReadOnlyList<ComponentClickStat> Snapshot()
    {
      return _stats.Values
        .OrderByDescending(x => x.TotalClicks)
        .ThenBy(x => x.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
        .ToList();
    }

    public int TotalClicks => _stats.Values.Sum(x => x.TotalClicks);

    public int TrackedToolCount => _stats.Count;

    public void Clear()
    {
      _stats.Clear();
    }

    public void RecordClick(IGH_DocumentObject obj, MouseButtons button)
    {
      if (obj == null)
        return;

      Guid componentId = obj.ComponentGuid;
      if (componentId == Guid.Empty)
        return;

      string objectName = ResolveObjectName(obj);

      ComponentClickStat stat = _stats.AddOrUpdate(
        componentId,
        id => CreateStat(id, objectName, button),
        (id, existing) => UpdateStat(existing, objectName, button));

      if (stat != null)
      {
        stat.Name = objectName;
      }
    }

    public bool TryGetStat(Guid componentId, out ComponentClickStat stat)
    {
      return _stats.TryGetValue(componentId, out stat);
    }

    public void ExportCsv(string path)
    {
      if (string.IsNullOrWhiteSpace(path))
        throw new ArgumentException("Export path is empty.", nameof(path));

      string fullPath = Path.GetFullPath(path);
      string directory = Path.GetDirectoryName(fullPath);
      if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        Directory.CreateDirectory(directory);

      StringBuilder builder = new StringBuilder();
      builder.AppendLine("ComponentId,Name,TotalClicks,LeftClicks,RightClicks,LastClickedUtc");

      foreach (ComponentClickStat stat in Snapshot())
      {
        builder.Append(stat.ComponentId.ToString("D"));
        builder.Append(",");
        builder.Append(EscapeCsv(stat.Name));
        builder.Append(",");
        builder.Append(stat.TotalClicks);
        builder.Append(",");
        builder.Append(stat.LeftClicks);
        builder.Append(",");
        builder.Append(stat.RightClicks);
        builder.Append(",");
        builder.Append(stat.LastClickedUtc.ToString("O"));
        builder.AppendLine();
      }

      File.WriteAllText(fullPath, builder.ToString(), Encoding.UTF8);
    }

    private static ComponentClickStat CreateStat(Guid componentId, string name, MouseButtons button)
    {
      ComponentClickStat stat = new ComponentClickStat
      {
        ComponentId = componentId,
        Name = name,
        LastClickedUtc = DateTime.UtcNow
      };

      ApplyButton(stat, button);
      return stat;
    }

    private static ComponentClickStat UpdateStat(ComponentClickStat stat, string name, MouseButtons button)
    {
      lock (stat)
      {
        stat.Name = name;
        stat.LastClickedUtc = DateTime.UtcNow;
        ApplyButton(stat, button);
        return stat;
      }
    }

    private static void ApplyButton(ComponentClickStat stat, MouseButtons button)
    {
      stat.TotalClicks++;

      if (button == MouseButtons.Right)
        stat.RightClicks++;
      else
        stat.LeftClicks++;
    }

    private static string ResolveObjectName(IGH_DocumentObject obj)
    {
      if (!string.IsNullOrWhiteSpace(obj.Name))
        return obj.Name;

      if (!string.IsNullOrWhiteSpace(obj.NickName))
        return obj.NickName;

      return obj.GetType().Name;
    }

    private static string EscapeCsv(string value)
    {
      string text = value ?? string.Empty;
      return "\"" + text.Replace("\"", "\"\"") + "\"";
    }
  }
}
