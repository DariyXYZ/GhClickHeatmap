using System;
using System.Reflection;
using System.Runtime.Versioning;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI;
using Rhino;

namespace GhClickHeatmap.Services
{
  [SupportedOSPlatform("windows")]
  public static class HeatmapWidgetMenuService
  {
    private const string RootItemName = "GhClickHeatmap.CanvasWidgets.Root";
    private const string ToggleItemName = "GhClickHeatmap.CanvasWidgets.Toggle";
    private const string ReloadItemName = "GhClickHeatmap.CanvasWidgets.Reload";
    private const string LabelsItemName = "GhClickHeatmap.CanvasWidgets.Labels";

    private static readonly object SyncRoot = new object();
    private static readonly FieldInfo? WidgetsMenuField =
      typeof(GH_DocumentEditor).GetField("_mnuWidgets", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo? BlankWidgetMenuItemField =
      typeof(GH_DocumentEditor).GetField("_mnuBlankWidgetItem", BindingFlags.Instance | BindingFlags.NonPublic);

    private static GH_DocumentEditor? _attachedEditor;
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
      GH_DocumentEditor editor = Instances.DocumentEditor;
      if (editor == null)
        return;

      lock (SyncRoot)
      {
        if (!ReferenceEquals(_attachedEditor, editor))
          _attachedEditor = editor;

        EnsureInstalled(editor);
        RefreshStates(editor);
      }
    }

    private static void EnsureInstalled(GH_DocumentEditor editor)
    {
      ToolStripMenuItem? widgetsMenu = GetWidgetsMenu(editor);
      if (widgetsMenu == null)
        return;

      if (widgetsMenu.DropDownItems.ContainsKey(RootItemName))
        return;

      ToolStripMenuItem rootItem = BuildRootItem();
      ToolStripItem? anchor = GetBlankWidgetItem(editor);

      int insertIndex = anchor == null
        ? widgetsMenu.DropDownItems.Count
        : Math.Max(0, widgetsMenu.DropDownItems.IndexOf(anchor) + 1);

      widgetsMenu.DropDownItems.Insert(insertIndex, rootItem);
      widgetsMenu.DropDownItems.Insert(insertIndex + 1, new ToolStripSeparator());
    }

    private static ToolStripMenuItem BuildRootItem()
    {
      ToolStripMenuItem rootItem = new ToolStripMenuItem("Heatmap")
      {
        Name = RootItemName,
        Image = HeatmapIconFactory.GetRootMenuIcon()
      };

      ToolStripMenuItem toggleItem = new ToolStripMenuItem("Show Overlay")
      {
        Name = ToggleItemName,
        CheckOnClick = true,
        Image = HeatmapIconFactory.GetToggleIcon()
      };
      toggleItem.Click += (_, __) =>
      {
        UsabilityReviewService.OverlayEnabled = toggleItem.Checked;
        InvalidateCanvas();
      };

      ToolStripMenuItem reloadItem = new ToolStripMenuItem("Reload Logs")
      {
        Name = ReloadItemName,
        Image = HeatmapIconFactory.GetReloadIcon()
      };
      reloadItem.Click += (_, __) =>
      {
        UsabilityReviewService.Reload();
        InvalidateCanvas();
      };

      ToolStripMenuItem labelsItem = new ToolStripMenuItem("Show Labels")
      {
        Name = LabelsItemName,
        CheckOnClick = true,
        Image = HeatmapIconFactory.GetLabelsIcon()
      };
      labelsItem.Click += (_, __) =>
      {
        UsabilityReviewService.ShowLabels = labelsItem.Checked;
        InvalidateCanvas();
      };

      rootItem.DropDownOpening += (_, __) => RefreshStates(_attachedEditor);
      rootItem.DropDownItems.Add(toggleItem);
      rootItem.DropDownItems.Add(reloadItem);
      rootItem.DropDownItems.Add(new ToolStripSeparator());
      rootItem.DropDownItems.Add(labelsItem);

      return rootItem;
    }

    private static void RefreshStates(GH_DocumentEditor? editor)
    {
      if (editor == null)
        return;

      ToolStripMenuItem? widgetsMenu = GetWidgetsMenu(editor);
      ToolStripMenuItem? rootItem = widgetsMenu?.DropDownItems[RootItemName] as ToolStripMenuItem;
      if (rootItem == null)
        return;

      if (rootItem.DropDownItems[ToggleItemName] is ToolStripMenuItem toggleItem)
        toggleItem.Checked = UsabilityReviewService.OverlayEnabled;

      if (rootItem.DropDownItems[LabelsItemName] is ToolStripMenuItem labelsItem)
        labelsItem.Checked = UsabilityReviewService.ShowLabels;
    }

    private static ToolStripMenuItem? GetWidgetsMenu(GH_DocumentEditor editor)
    {
      return WidgetsMenuField?.GetValue(editor) as ToolStripMenuItem;
    }

    private static ToolStripItem? GetBlankWidgetItem(GH_DocumentEditor editor)
    {
      return BlankWidgetMenuItemField?.GetValue(editor) as ToolStripItem;
    }

    private static void InvalidateCanvas()
    {
      Instances.ActiveCanvas?.Invalidate();
    }
  }
}
