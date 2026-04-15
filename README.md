# GhClickHeatmap

`GhClickHeatmap` is a Rhino 8 and Grasshopper plugin for lightweight usability analytics on large Grasshopper definitions.

It records component clicks into shared `jsonl` logs and renders an aggregated heatmap overlay directly on the Grasshopper canvas. The project is designed for team and template-driven workflows where multiple users open copies of the same `.gh` file and you want to understand which components are used often, which are ignored, and where onboarding or documentation still needs work.

## Why use it

- understand real component usage in complex Grasshopper definitions
- spot overlooked tools and dead zones in templates
- identify onboarding friction based on actual clicks instead of guesswork
- review usage data without external analytics services

## Features

- `Usability Recorder` captures component clicks during normal work
- `Usability Review` loads collected logs and draws a heatmap overlay on the active canvas
- shared log storage using newline-delimited JSON (`jsonl`)
- configurable `LogRoot` input so teams can switch log folders without rebuilding the plugin
- offline-friendly deployment through a packaged `.gha`

## Components

### Usability Recorder

Inputs:

- `EnableRecording`
- `LogRoot`

Outputs:

- `Status`

Behavior:

- enables or disables automatic click recording
- creates a separate `jsonl` session log for each Rhino session
- switches to a new target folder when `LogRoot` changes

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

- reads `jsonl` logs from the selected folder
- reloads logs when `Reload = true`
- shows or hides the heatmap overlay on the active Grasshopper canvas

## Default log folder

The built-in default log folder is:

`X:\CompDesign_Projects\Library\wind\Templates New\usability_test_log`

Both nodes support a `LogRoot` input:

- if `LogRoot` is empty or not connected, the built-in default path is used
- if `LogRoot` is provided, both recording and review use that folder instead

## Build

Requirements:

- Rhino 8
- Grasshopper for Rhino 8
- .NET 8 SDK

Build command:

```powershell
dotnet build .\GhClickHeatmap.csproj
```

The build produces a compiled Grasshopper plugin assembly (`.gha`).

## Repository layout

- `Components/` - user-facing Grasshopper components
- `Services/` - logging, aggregation, overlay, and runtime services
- `GhClickHeatmap.csproj` - plugin project file

## Intended use

`GhClickHeatmap` fits best when:

- teams reuse shared Grasshopper templates
- multiple designers work on similar definitions
- you want lightweight internal analytics without cloud tooling

## Current scope

This repository focuses on local and shared-folder usability tracking for Grasshopper. It is not a general telemetry platform, cloud analytics product, or full plugin suite.

## License

Released under the [MIT License](LICENSE).
