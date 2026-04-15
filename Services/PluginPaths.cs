using System;
using System.IO;

namespace GhClickHeatmap.Services
{
  public static class PluginPaths
  {
    public const string DefaultLogRootPath = @"X:\CompDesign_Projects\Library\wind\Templates New\usability_test_log";

    public static string? LogRootPathOverride { get; set; }

    public static string EffectiveLogRootPath => GetLogRootPath();

    public static string GetLogRootPath()
    {
      return NormalizePath(string.IsNullOrWhiteSpace(LogRootPathOverride)
        ? DefaultLogRootPath
        : LogRootPathOverride);
    }

    public static string ConfigureLogRootPath(string? preferredPath)
    {
      string normalized = NormalizePath(preferredPath ?? string.Empty);
      LogRootPathOverride = string.IsNullOrWhiteSpace(normalized)
        ? null
        : normalized;

      return GetLogRootPath();
    }

    public static string NormalizePath(string path)
    {
      if (string.IsNullOrWhiteSpace(path))
        return string.Empty;

      try
      {
        return Path.GetFullPath(path)
          .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
      }
      catch
      {
        return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
      }
    }
  }
}
