using Grasshopper.Kernel;

namespace GhClickHeatmap
{
  public sealed class ClickHeatmapPriority : GH_AssemblyPriority
  {
    public override GH_LoadingInstruction PriorityLoad()
    {
      Services.CanvasHeatmapController.Initialize();
      return GH_LoadingInstruction.Proceed;
    }
  }
}
