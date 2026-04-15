using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GhClickHeatmap.Services
{
  public static class HeatmapIconFactory
  {
    private static readonly Color CardTop = Color.FromArgb(248, 255, 255, 255);
    private static readonly Color CardBottom = Color.FromArgb(236, 246, 249, 255);
    private static readonly Color CardStroke = Color.FromArgb(210, 170, 188, 214);
    private static readonly Color AccentText = Color.FromArgb(255, 56, 74, 98);
    private static readonly Color AccentBlue = Color.FromArgb(255, 73, 138, 220);
    private static readonly Color AccentOrange = Color.FromArgb(255, 244, 122, 52);
    private static readonly Color AccentRed = Color.FromArgb(255, 222, 72, 66);
    private static readonly Color AccentGlow = Color.FromArgb(255, 255, 176, 92);

    private static Bitmap? _pluginIcon;
    private static Bitmap? _rootMenuIcon;
    private static Bitmap? _toggleIcon;
    private static Bitmap? _reloadIcon;
    private static Bitmap? _labelsIcon;

    public static Bitmap GetPluginIcon()
    {
      return _pluginIcon ??= CreatePluginIcon();
    }

    public static Bitmap GetRootMenuIcon()
    {
      return _rootMenuIcon ??= CreateRootMenuIcon();
    }

    public static Bitmap GetToggleIcon()
    {
      return _toggleIcon ??= CreateToggleIcon();
    }

    public static Bitmap GetReloadIcon()
    {
      return _reloadIcon ??= CreateReloadIcon();
    }

    public static Bitmap GetLabelsIcon()
    {
      return _labelsIcon ??= CreateLabelsIcon();
    }

    private static Bitmap CreatePluginIcon()
    {
      Bitmap bitmap = CreateBlankIcon();
      using Graphics graphics = Graphics.FromImage(bitmap);
      Prepare(graphics);

      using GraphicsPath glowPath = CreateRoundedRect(2f, 2f, 20f, 20f, 6f);
      using PathGradientBrush glow = new PathGradientBrush(glowPath)
      {
        CenterColor = Color.FromArgb(165, AccentGlow),
        SurroundColors = new[] { Color.FromArgb(0, AccentGlow) }
      };

      graphics.FillPath(glow, glowPath);
      DrawHeatCard(graphics, new RectangleF(3f, 3f, 18f, 18f), 4.5f, true);

      using SolidBrush badgeFill = new SolidBrush(AccentRed);
      using Pen badgeStroke = new Pen(Color.FromArgb(240, 255, 255, 255), 1.2f);
      graphics.FillEllipse(badgeFill, 14.2f, 13.8f, 5.6f, 5.6f);
      graphics.DrawEllipse(badgeStroke, 14.2f, 13.8f, 5.6f, 5.6f);

      return bitmap;
    }

    private static Bitmap CreateRootMenuIcon()
    {
      Bitmap bitmap = CreateBlankIcon();
      using Graphics graphics = Graphics.FromImage(bitmap);
      Prepare(graphics);

      DrawHeatCard(graphics, new RectangleF(2.5f, 3f, 19f, 18f), 4.5f, true);

      return bitmap;
    }

    private static Bitmap CreateToggleIcon()
    {
      Bitmap bitmap = CreateBlankIcon();
      using Graphics graphics = Graphics.FromImage(bitmap);
      Prepare(graphics);

      DrawSoftCardBackground(graphics);

      using GraphicsPath eyePath = new GraphicsPath();
      eyePath.AddArc(2.5f, 6.3f, 19f, 10.8f, 0f, 180f);
      eyePath.AddArc(2.5f, 6.3f, 19f, 10.8f, 180f, 180f);
      eyePath.CloseFigure();

      using Pen eyeStroke = new Pen(AccentBlue, 1.55f);
      graphics.DrawPath(eyeStroke, eyePath);

      using SolidBrush iris = new SolidBrush(AccentOrange);
      using SolidBrush pupil = new SolidBrush(AccentText);
      using SolidBrush sparkle = new SolidBrush(Color.FromArgb(240, 255, 244, 233));
      graphics.FillEllipse(iris, 8.2f, 8.3f, 7.6f, 7.6f);
      graphics.FillEllipse(pupil, 10.45f, 10.55f, 3.2f, 3.2f);
      graphics.FillEllipse(sparkle, 11f, 9.1f, 1.25f, 1.25f);

      return bitmap;
    }

    private static Bitmap CreateReloadIcon()
    {
      Bitmap bitmap = CreateBlankIcon();
      using Graphics graphics = Graphics.FromImage(bitmap);
      Prepare(graphics);

      DrawSoftCardBackground(graphics);

      using Pen pen = new Pen(AccentBlue, 1.85f)
      {
        StartCap = LineCap.Round,
        EndCap = LineCap.Round
      };

      graphics.DrawArc(pen, 4.8f, 5.2f, 14.2f, 14.2f, 32f, 248f);

      using SolidBrush arrow = new SolidBrush(AccentBlue);
      PointF[] tip =
      {
        new PointF(16.8f, 4.2f),
        new PointF(20.3f, 5.2f),
        new PointF(17.8f, 8.2f)
      };
      graphics.FillPolygon(arrow, tip);

      return bitmap;
    }

    private static Bitmap CreateLabelsIcon()
    {
      Bitmap bitmap = CreateBlankIcon();
      using Graphics graphics = Graphics.FromImage(bitmap);
      Prepare(graphics);

      DrawSoftCardBackground(graphics);

      RectangleF tag = new RectangleF(4.2f, 5f, 15.6f, 8.1f);
      using GraphicsPath tagPath = CreateRoundedRect(tag.X, tag.Y, tag.Width, tag.Height, 2.7f);
      using LinearGradientBrush tagFill = new LinearGradientBrush(
        tag,
        Color.FromArgb(255, 255, 250, 241),
        Color.FromArgb(255, 245, 233, 211),
        90f);
      using Pen tagStroke = new Pen(Color.FromArgb(200, 175, 195, 222), 1f);
      using Font font = new Font("Segoe UI", 6.8f, FontStyle.Bold, GraphicsUnit.Pixel);
      using SolidBrush textBrush = new SolidBrush(AccentText);
      using Pen linePen = new Pen(Color.FromArgb(170, 190, 210, 232), 1f);

      graphics.FillPath(tagFill, tagPath);
      graphics.DrawPath(tagStroke, tagPath);
      graphics.DrawString("12", font, textBrush, 6f, 6.6f);
      graphics.DrawString("|", font, textBrush, 10.8f, 6.35f);
      graphics.DrawString("3", font, textBrush, 13.4f, 6.6f);

      graphics.DrawLine(linePen, 5.3f, 16.1f, 18.6f, 16.1f);
      graphics.DrawLine(linePen, 5.3f, 18.8f, 14.8f, 18.8f);

      return bitmap;
    }

    private static void DrawHeatCard(Graphics graphics, RectangleF cardRect, float radius, bool withShadow)
    {
      if (withShadow)
      {
        using GraphicsPath shadowPath = CreateRoundedRect(cardRect.X + 0.8f, cardRect.Y + 1.1f, cardRect.Width, cardRect.Height, radius);
        using SolidBrush shadow = new SolidBrush(Color.FromArgb(34, 44, 78, 120));
        graphics.FillPath(shadow, shadowPath);
      }

      using GraphicsPath cardPath = CreateRoundedRect(cardRect.X, cardRect.Y, cardRect.Width, cardRect.Height, radius);
      using LinearGradientBrush cardFill = new LinearGradientBrush(cardRect, CardTop, CardBottom, 90f);
      using Pen cardStroke = new Pen(CardStroke, 1.1f);

      graphics.FillPath(cardFill, cardPath);
      graphics.DrawPath(cardStroke, cardPath);

      float cellSize = 3.2f;
      float gap = 1.8f;
      float startX = cardRect.X + 3.2f;
      float startY = cardRect.Y + 3.2f;

      Color[] colors =
      {
        Color.FromArgb(255, 255, 244, 214),
        Color.FromArgb(255, 255, 198, 108),
        Color.FromArgb(255, 247, 142, 73),
        Color.FromArgb(255, 255, 214, 138),
        Color.FromArgb(255, 244, 122, 52),
        Color.FromArgb(255, 229, 86, 71),
        Color.FromArgb(255, 255, 176, 92),
        Color.FromArgb(255, 234, 103, 64),
        Color.FromArgb(255, 216, 68, 66)
      };

      int index = 0;
      for (int row = 0; row < 3; row++)
      {
        for (int column = 0; column < 3; column++)
        {
          float x = startX + column * (cellSize + gap);
          float y = startY + row * (cellSize + gap);
          DrawHeatCell(graphics, x, y, colors[index++]);
        }
      }
    }

    private static void DrawSoftCardBackground(Graphics graphics)
    {
      RectangleF cardRect = new RectangleF(2.5f, 3f, 19f, 18f);
      using GraphicsPath cardPath = CreateRoundedRect(cardRect.X, cardRect.Y, cardRect.Width, cardRect.Height, 4.6f);
      using LinearGradientBrush cardFill = new LinearGradientBrush(cardRect, CardTop, CardBottom, 90f);
      using Pen stroke = new Pen(Color.FromArgb(195, 183, 201, 226), 1f);

      graphics.FillPath(cardFill, cardPath);
      graphics.DrawPath(stroke, cardPath);
    }

    private static void DrawHeatCell(Graphics graphics, float x, float y, Color color)
    {
      using SolidBrush fill = new SolidBrush(color);
      using Pen stroke = new Pen(Color.FromArgb(132, 255, 255, 255), 0.85f);
      graphics.FillRectangle(fill, x, y, 3.2f, 3.2f);
      graphics.DrawRectangle(stroke, x, y, 3.2f, 3.2f);
    }

    private static GraphicsPath CreateRoundedRect(float x, float y, float width, float height, float radius)
    {
      float diameter = radius * 2f;
      GraphicsPath path = new GraphicsPath();
      path.AddArc(x, y, diameter, diameter, 180f, 90f);
      path.AddArc(x + width - diameter, y, diameter, diameter, 270f, 90f);
      path.AddArc(x + width - diameter, y + height - diameter, diameter, diameter, 0f, 90f);
      path.AddArc(x, y + height - diameter, diameter, diameter, 90f, 90f);
      path.CloseFigure();
      return path;
    }

    private static Bitmap CreateBlankIcon()
    {
      return new Bitmap(24, 24);
    }

    private static void Prepare(Graphics graphics)
    {
      graphics.SmoothingMode = SmoothingMode.AntiAlias;
      graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
      graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
      graphics.Clear(Color.Transparent);
    }
  }
}
