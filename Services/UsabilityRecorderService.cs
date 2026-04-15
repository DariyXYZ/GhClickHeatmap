using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Versioning;
using Grasshopper;
using Grasshopper.Kernel;

namespace GhClickHeatmap.Services
{
  [SupportedOSPlatform("windows")]
  public static class UsabilityRecorderService
  {
    private static readonly object SyncRoot = new object();
    private static readonly Type? GhDocumentType =
      typeof(IGH_DocumentObject).Assembly.GetType("Grasshopper.Kernel.GH_Document");

    private static readonly string SessionIdValue = Guid.NewGuid().ToString("N");
    private static string? _sessionFilePath;
    private static int _recordedEventCount;
    private static string _lastStatus = "Recorder idle";

    public static bool RecordingEnabled { get; set; } = true;

    public static int RecordedEventCount => _recordedEventCount;

    public static string SessionId => SessionIdValue;

    public static string SessionFilePath => _sessionFilePath ?? string.Empty;

    public static string LastStatus => _lastStatus ?? string.Empty;

    public static string LogRootPath
    {
      get { return PluginPaths.GetLogRootPath(); }
      set { SetLogRootPath(value); }
    }

    public static void SetLogRootPath(string? preferredPath)
    {
      string current = PluginPaths.GetLogRootPath();
      string next = PluginPaths.ConfigureLogRootPath(preferredPath);

      if (!string.Equals(current, next, StringComparison.OrdinalIgnoreCase))
      {
        lock (SyncRoot)
        {
          _sessionFilePath = null;
          _lastStatus = "Log folder changed to " + next;
        }
      }
    }

    public static void EnsureReady(object document)
    {
      try
      {
        EnsureSessionFilePath();

        if (!File.Exists(_sessionFilePath))
        {
          using (FileStream stream = new FileStream(_sessionFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
          {
          }
        }

        _lastStatus = "Ready to record to " + _sessionFilePath;
      }
      catch (Exception ex)
      {
        _lastStatus = "Recorder initialization failed: " + ex.Message;
      }
    }

    public static void EnsureReadyForActiveCanvas()
    {
      EnsureReady(Instances.ActiveCanvas?.Document);
    }

    public static void RecordClick(object document, IGH_DocumentObject obj, MouseButtons button)
    {
      if (!RecordingEnabled)
        return;

      if (obj == null || (button != MouseButtons.Left && button != MouseButtons.Right))
        return;

      string documentPath = ResolveDocumentPath(document);

      string objectInstanceGuid = ResolveObjectInstanceGuid(obj);
      if (string.IsNullOrWhiteSpace(objectInstanceGuid))
      {
        _lastStatus = "Object instance guid was not resolved. Recording skipped.";
        return;
      }

      try
      {
        EnsureReady(document!);
        if (string.IsNullOrWhiteSpace(_sessionFilePath))
          return;

        UsabilityLogEvent logEvent = new UsabilityLogEvent
        {
          UtcTimestamp = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
          SessionId = SessionIdValue,
          UserName = Environment.UserName,
          MachineName = Environment.MachineName,
          DocumentPath = documentPath,
          DocumentName = Path.GetFileName(documentPath),
          ObjectInstanceGuid = objectInstanceGuid,
          ComponentGuid = obj.ComponentGuid.ToString("D"),
          ObjectName = obj.Name ?? string.Empty,
          ObjectNickName = obj.NickName ?? string.Empty,
          CanvasX = obj.Attributes?.Bounds.X ?? 0f,
          CanvasY = obj.Attributes?.Bounds.Y ?? 0f,
          Width = obj.Attributes?.Bounds.Width ?? 0f,
          Height = obj.Attributes?.Bounds.Height ?? 0f,
          Button = button.ToString()
        };

        string line = JsonLineSerializer.Serialize(logEvent) + Environment.NewLine;
        byte[] bytes = Encoding.UTF8.GetBytes(line);

        lock (SyncRoot)
        {
          using (FileStream stream = new FileStream(_sessionFilePath, FileMode.Append, FileAccess.Write, FileShare.Read))
          {
            stream.Write(bytes, 0, bytes.Length);
          }

          _recordedEventCount++;
          _lastStatus = "Recording to " + _sessionFilePath;
        }
      }
      catch (Exception ex)
      {
        _lastStatus = "Recording failed: " + ex.Message;
      }
    }

    public static string ResolveDocumentPath(object document)
    {
      if (document == null)
        return string.Empty;

      try
      {
        Type documentType = document.GetType();
        PropertyInfo? filePathProperty = documentType.GetProperty(
          "FilePath",
          BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (filePathProperty == null && GhDocumentType != null && GhDocumentType.IsAssignableFrom(documentType))
        {
          filePathProperty = GhDocumentType.GetProperty(
            "FilePath",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        if (filePathProperty != null)
        {
          string? path = filePathProperty.GetValue(document, null) as string;
          return PluginPaths.NormalizePath(path);
        }
      }
      catch
      {
      }

      return string.Empty;
    }

    public static string ResolveObjectInstanceGuid(IGH_DocumentObject obj)
    {
      if (obj == null)
        return string.Empty;

      try
      {
        Type objectType = obj.GetType();
        PropertyInfo? instanceGuidProperty = objectType.GetProperty(
          "InstanceGuid",
          BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (instanceGuidProperty != null)
        {
          object value = instanceGuidProperty.GetValue(obj, null);
          if (value != null)
            return value.ToString();
        }
      }
      catch
      {
      }

      return string.Empty;
    }

    private static void EnsureSessionFilePath()
    {
      string folder = PluginPaths.GetLogRootPath();
      if (!string.IsNullOrWhiteSpace(_sessionFilePath))
      {
        string currentFolder = PluginPaths.NormalizePath(Path.GetDirectoryName(_sessionFilePath) ?? string.Empty);
        if (string.Equals(currentFolder, folder, StringComparison.OrdinalIgnoreCase))
          return;

        _sessionFilePath = null;
      }

      Directory.CreateDirectory(folder);

      string fileName = string.Format(
        CultureInfo.InvariantCulture,
        "{0}_{1}_{2}_{3}.jsonl",
        DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture),
        SafeToken(Environment.UserName),
        SafeToken(Environment.MachineName),
        SessionIdValue.Substring(0, 8));

      _sessionFilePath = Path.Combine(folder, fileName);
    }

    private static string SafeToken(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
        return "unknown";

      StringBuilder builder = new StringBuilder(value.Length);
      for (int i = 0; i < value.Length; i++)
      {
        char c = value[i];
        if (char.IsLetterOrDigit(c) || c == '-' || c == '_')
          builder.Append(c);
        else
          builder.Append('_');
      }

      return builder.ToString();
    }
  }
}
