using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Rhino;

namespace GhClickHeatmap.Services
{
  [SupportedOSPlatform("windows")]
  public static class CanvasHeatmapController
  {
    private static readonly object SyncRoot = new object();
    private static readonly PropertyInfo? CanvasGraphicsProperty =
      typeof(GH_Canvas).GetProperty("Graphics", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    private static GH_Canvas? _attachedCanvas;
    private static bool _initialized;

    public static void Initialize()
    {
      lock (SyncRoot)
      {
        if (_initialized)
          return;

        RhinoApp.Idle += RhinoAppOnIdle;
        _initialized = true;
      }
    }

    private static void RhinoAppOnIdle(object? sender, EventArgs e)
    {
      GH_Canvas activeCanvas = Instances.ActiveCanvas;
      if (activeCanvas == null)
        return;

      if (ReferenceEquals(activeCanvas, _attachedCanvas))
        return;

      lock (SyncRoot)
      {
        if (ReferenceEquals(activeCanvas, _attachedCanvas))
          return;

        DetachCanvas(_attachedCanvas);
        AttachCanvas(activeCanvas);
      }

      if (UsabilityRecorderService.RecordingEnabled)
        UsabilityRecorderService.EnsureReady(activeCanvas.Document);
    }

    private static void AttachCanvas(GH_Canvas canvas)
    {
      if (canvas == null)
        return;

      canvas.MouseDown += CanvasOnMouseDown;
      canvas.CanvasPostPaintOverlay += CanvasOnPostPaintOverlay;
      _attachedCanvas = canvas;
    }

    private static void DetachCanvas(GH_Canvas canvas)
    {
      if (canvas == null)
        return;

      canvas.MouseDown -= CanvasOnMouseDown;
      canvas.CanvasPostPaintOverlay -= CanvasOnPostPaintOverlay;

      if (ReferenceEquals(_attachedCanvas, canvas))
        _attachedCanvas = null;
    }

    private static void CanvasOnMouseDown(object? sender, System.Windows.Forms.MouseEventArgs e)
    {
      GH_Canvas? canvas = sender as GH_Canvas;
      GH_Document? document = canvas?.Document;
      if (canvas == null || document == null)
        return;

      PointF canvasPoint = canvas.Viewport.UnprojectPoint(new PointF(e.X, e.Y));
      IGH_DocumentObject clickedObject = FindTopMostObject(document, canvasPoint);
      if (clickedObject == null)
        return;

      UsabilityRecorderService.RecordClick(document, clickedObject, e.Button);
      canvas.Invalidate();
    }

    private static IGH_DocumentObject? FindTopMostObject(GH_Document document, PointF canvasPoint)
    {
      if (document == null)
        return null;

      return document.Objects
        .Where(obj => obj?.Attributes != null)
        .Reverse()
        .FirstOrDefault(obj => obj.Attributes.Bounds.Contains(canvasPoint));
    }

    private static void CanvasOnPostPaintOverlay(GH_Canvas sender)
    {
      if (!UsabilityReviewService.OverlayEnabled)
        return;

      GH_Document? document = sender?.Document;
      if (sender == null || document == null)
        return;

      Graphics graphics = TryGetCanvasGraphics(sender, out bool disposeGraphics);
      if (graphics == null)
        return;

      try
      {
        int maxClicks = UsabilityReviewService.Snapshot.RankedObjects
          ?.Select(x => x.TotalClicks)
          .DefaultIfEmpty(0)
          .Max() ?? 0;

        if (maxClicks <= 0)
          return;

        foreach (IGH_DocumentObject obj in document.Objects)
        {
          if (obj?.Attributes == null)
            continue;

          if (!UsabilityReviewService.TryGetAggregate(obj, out UsabilityAggregate aggregate))
            continue;

          if (aggregate.TotalClicks < Math.Max(1, UsabilityReviewService.MinimumClicksToDraw))
            continue;

          DrawObjectHeat(sender, graphics, obj, aggregate, maxClicks);
        }
      }
      finally
      {
        if (disposeGraphics)
          graphics.Dispose();
      }
    }

    private static Graphics? TryGetCanvasGraphics(GH_Canvas canvas, out bool disposeGraphics)
    {
      disposeGraphics = false;

      if (CanvasGraphicsProperty != null)
      {
        Graphics? liveGraphics = CanvasGraphicsProperty.GetValue(canvas, null) as Graphics;
        if (liveGraphics != null)
          return liveGraphics;
      }

      disposeGraphics = true;
      return canvas.GetGraphicsObject(false);
    }

    private static void DrawObjectHeat(
      GH_Canvas canvas,
      Graphics graphics,
      IGH_DocumentObject obj,
      UsabilityAggregate aggregate,
      int maxClicks)
    {
      RectangleF screenBounds = obj.Attributes.Bounds;

      if (screenBounds.Width < 6f || screenBounds.Height < 6f)
        return;

      float ratio = maxClicks <= 0 ? 0f : (float) aggregate.TotalClicks / maxClicks;
      int glowAlpha = 20 + (int) Math.Round(95f * ratio);
      glowAlpha = Math.Max(20, Math.Min(glowAlpha, 120));

      Color fillColor = Blend(Color.FromArgb(255, 250, 214, 120), Color.FromArgb(255, 229, 78, 52), ratio);
      Color strokeColor = Blend(Color.FromArgb(255, 182, 126, 48), Color.FromArgb(255, 145, 27, 27), ratio);
      RectangleF outerRect = Inflate(screenBounds, 10f + 10f * ratio);
      RectangleF innerRect = Inflate(screenBounds, 4f + 4f * ratio);

      using (Brush outerFill = new SolidBrush(Color.FromArgb(glowAlpha / 2, fillColor)))
      using (Brush innerFill = new SolidBrush(Color.FromArgb(glowAlpha, fillColor)))
      using (Pen stroke = new Pen(Color.FromArgb(Math.Min(190, glowAlpha + 60), strokeColor), 2f))
      using (Font font = new Font("Segoe UI", 8f, FontStyle.Bold))
      using (Brush textBrush = new SolidBrush(Color.FromArgb(225, 70, 22, 12)))
      {
        graphics.FillRectangle(outerFill, outerRect);
        graphics.FillRectangle(innerFill, innerRect);
        graphics.DrawRectangle(stroke, innerRect.X, innerRect.Y, innerRect.Width, innerRect.Height);

        if (!UsabilityReviewService.ShowLabels)
          return;

        string label = aggregate.TotalClicks + " | " + aggregate.UniqueUserCount;
        SizeF textSize = graphics.MeasureString(label, font);
        RectangleF labelRect = new RectangleF(
          innerRect.Right - textSize.Width - 8f,
          innerRect.Y + 4f,
          textSize.Width + 6f,
          textSize.Height + 3f);

        using (Brush labelBackground = new SolidBrush(Color.FromArgb(205, 255, 252, 245)))
          graphics.FillRectangle(labelBackground, labelRect);

        graphics.DrawString(label, font, textBrush, labelRect.X + 3f, labelRect.Y + 1f);
      }
    }

    private static RectangleF Inflate(RectangleF rect, float amount)
    {
      return RectangleF.FromLTRB(
        rect.Left - amount,
        rect.Top - amount,
        rect.Right + amount,
        rect.Bottom + amount);
    }

    private static Color Blend(Color cold, Color hot, float ratio)
    {
      ratio = Math.Max(0f, Math.Min(1f, ratio));

      int r = cold.R + (int) Math.Round((hot.R - cold.R) * ratio);
      int g = cold.G + (int) Math.Round((hot.G - cold.G) * ratio);
      int b = cold.B + (int) Math.Round((hot.B - cold.B) * ratio);
      return Color.FromArgb(255, r, g, b);
    }
  }
}
