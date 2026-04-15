# GhClickHeatmap

`GhClickHeatmap` is a Rhino 8 / Grasshopper plugin for lightweight usability analytics on large Grasshopper definitions.

It records component clicks into shared `jsonl` logs and can render an aggregated heatmap overlay on top of the same Grasshopper canvas. The plugin was designed for template-based team workflows where multiple users open copies of the same `.gh` file and you want to understand which tools are used often, which are ignored, and where onboarding materials are still needed.

## Features

- `Usability Recorder` records user clicks automatically during normal work.
- `Usability Review` loads collected logs and draws a visual heatmap over matched components.
- Shared log storage with a built-in default path.
- Optional `LogRoot` input on both nodes for switching to a different log folder without rebuilding the plugin.
- Offline-friendly deployment via a packaged `.gha`.

## Default Log Folder

By default the plugin uses:

`X:\CompDesign_Projects\Library\wind\Templates New\usability_test_log`

Both nodes support a `LogRoot` input:

- if `LogRoot` is empty or not connected, the built-in default path is used;
- if `LogRoot` is provided, both recording and review use that folder instead.

## Components

### Usability Recorder

Inputs:
- `EnableRecording`
- `LogRoot`

Outputs:
- `Status`

Behavior:
- enables or disables automatic click recording;
- creates a separate `jsonl` session log for each Rhino session;
- switches to a new target folder when `LogRoot` changes.

### Usability Review

Inputs:
- `EnableOverlay`
- `Reload`
- `LogRoot`

Outputs:
- `Status`
- `LoadedEvents`
- `MatchedObjects`

Behavior:
- reads `jsonl` logs from the selected folder;
- reloads logs when `Reload = true`;
- shows or hides the heatmap overlay on the active Grasshopper canvas.

## Build

Requirements:
- Rhino 8
- Grasshopper for Rhino 8
- .NET 8 SDK

Build command:

```powershell
dotnet build .\GhClickHeatmap.csproj
```

The project builds a `.gha` plugin assembly.

## Repository Layout

- `Components/` - Grasshopper components exposed to users.
- `Services/` - log storage, aggregation, overlay, and runtime services.
- `GhClickHeatmap.csproj` - plugin project file.

## License

This project is released under the [MIT License](LICENSE).
