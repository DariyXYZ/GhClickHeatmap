using System;
using Grasshopper;
using Grasshopper.Kernel;
using GhClickHeatmap.Services;

namespace GhClickHeatmap.Components
{
  public sealed class HeatmapSettingsComponent : GH_Component
  {
    public HeatmapSettingsComponent()
      : base(
        "Usability Recorder",
        "UsageRec",
        "Keeps automatic recording enabled and reports whether the usability logger is working.",
        "Analytics",
        "Telemetry")
    {
    }

    public override Guid ComponentGuid => new Guid("B5678A8A-75E6-4D70-9148-D44805E4A1A7");

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      pManager.AddBooleanParameter("EnableRecording", "E", "Turns automatic click recording on or off.", GH_ParamAccess.item, true);
      pManager.AddTextParameter(
        "LogRoot",
        "L",
        "Optional folder for writing usability logs. Leave empty to use the built-in shared default path.",
        GH_ParamAccess.item,
        PluginPaths.DefaultLogRootPath);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddTextParameter("Status", "S", "Current recorder status.", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess da)
    {
      bool enableTracking = true;
      string logRoot = PluginPaths.DefaultLogRootPath;

      da.GetData(0, ref enableTracking);
      da.GetData(1, ref logRoot);

      UsabilityRecorderService.SetLogRootPath(logRoot);
      UsabilityRecorderService.RecordingEnabled = enableTracking;
      UsabilityRecorderService.EnsureReadyForActiveCanvas();

      string status = (enableTracking ? "Recording enabled. " : "Recording disabled. ")
        + UsabilityRecorderService.LastStatus;

      da.SetData(0, status);
    }
  }
}
