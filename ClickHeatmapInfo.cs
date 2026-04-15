using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace GhClickHeatmap
{
  public sealed class ClickHeatmapInfo : GH_AssemblyInfo
  {
    public override string Name => "GH Click Heatmap";

    public override Bitmap? Icon => null;

    public override string Description =>
      "Tracks clicks on Grasshopper tools and draws a heatmap overlay on the canvas.";

    public override Guid Id => new Guid("9B2BB863-D281-4DCB-BD34-20A9990F2057");

    public override string AuthorName => "OpenAI + user workspace";

    public override string AuthorContact => "local prototype";
  }
}
