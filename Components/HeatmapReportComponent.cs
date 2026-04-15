using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using GhClickHeatmap.Services;

namespace GhClickHeatmap.Components
{
  public sealed class HeatmapReportComponent : GH_Component
  {
    public HeatmapReportComponent()
      : base(
        "Usability Review",
        "UsageMap",
        "Reloads shared usability logs and optionally shows the heatmap overlay on the current canvas.",
        "Analytics",
        "Telemetry")
    {
    }

    public override Guid ComponentGuid => new Guid("D479F90D-56BB-4E14-AE61-E70457D5AD4D");

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      pManager.AddBooleanParameter("EnableOverlay", "E", "Shows or hides the heatmap overlay on the current canvas.", GH_ParamAccess.item, true);
      pManager.AddBooleanParameter("Reload", "R", "Reloads shared usability logs when true.", GH_ParamAccess.item, false);
      pManager.AddTextParameter(
        "LogRoot",
        "L",
        "Optional folder for reading usability logs. Leave empty to use the built-in shared default path.",
        GH_ParamAccess.item,
        PluginPaths.DefaultLogRootPath);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddTextParameter("Status", "S", "Review status.", GH_ParamAccess.item);
      pManager.AddIntegerParameter("LoadedEvents", "E", "Total number of events loaded from logs.", GH_ParamAccess.item);
      pManager.AddIntegerParameter("MatchedObjects", "O", "Number of objects on the current canvas that were found in the aggregated logs.", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess da)
    {
      bool enableOverlay = true;
      bool reload = false;
      string logRoot = PluginPaths.DefaultLogRootPath;

      da.GetData(0, ref enableOverlay);
      da.GetData(1, ref reload);
      da.GetData(2, ref logRoot);

      UsabilityReviewService.OverlayEnabled = enableOverlay;
      UsabilityReviewService.LogRootPath = logRoot;

      if (reload || UsabilityReviewService.Snapshot.RankedObjects == null)
        UsabilityReviewService.Reload();

      ReviewSnapshot snapshot = UsabilityReviewService.Snapshot;

      int matchedObjects = 0;
      var doc = Instances.ActiveCanvas?.Document;
      if (doc != null && snapshot.ByObjectId != null)
      {
        foreach (IGH_DocumentObject obj in doc.Objects)
        {
          if (UsabilityReviewService.TryGetAggregate(obj, out UsabilityAggregate aggregate)
              && aggregate.TotalClicks >= UsabilityReviewService.MinimumClicksToDraw)
            matchedObjects++;
        }
      }

      da.SetData(0, snapshot.Status);
      da.SetData(1, snapshot.LoadedEventCount);
      da.SetData(2, matchedObjects);
    }
  }
}
