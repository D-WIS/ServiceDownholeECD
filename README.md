# DWIS Service: Downhole ECD

This repository contains a .NET-based DWIS solution for real-time computation of **Downhole Equivalent Circulation Density (ECD)** from:

- downhole annulus pressure
- bottom-of-string depth
- well trajectory
- wellbore architecture
- BHA/drill string context

The computational definition is aligned with **SPE-217668-MS**: https://doi.org/10.2118/217668-MS.

## Solution Overview

This is a multi-project solution built around a DWIS blackboard data exchange pattern:

1. A data source publishes realtime input signals.
2. A server service subscribes to inputs and contextual data, computes ECD, and publishes outputs.
3. A data sink subscribes to outputs for validation/monitoring.

The solution also includes a shared-model generator used to produce contextual DTOs and merged schema artifacts.

## Project Structure

- `DWIS.Service.DownholeECD.Model`
  - Domain and semantic data contracts for realtime inputs/outputs and contextual data.
  - Includes conversion logic (`DownholePressureConverter`) for pressure-to-ECD processing.
- `DWIS.Service.DownholeECD.Server`
  - Hosted worker that runs the realtime computation loop.
  - Reads inputs and context from the blackboard, computes ECD, and publishes outputs.
- `DWIS.Service.DownholeECD.DataSource`
  - Helper worker that publishes synthetic realtime inputs (depth and annulus pressure).
  - Useful for testing and demonstrations without field data.
- `DWIS.Service.DownholeECD.DataSink`
  - Helper worker that subscribes to computed outputs and logs them.
  - Used to verify end-to-end data flow.
- `DWIS.Service.DownholeECD.ModelSharedOut`
  - Console utility that merges OpenAPI schema dependencies and generates shared C# DTO/client code.
  - Produces `DownholeECDMergedModel.cs` and `DownholeECDMergedModel.json`.

## Data Flow

1. `DataSource` publishes:
   - `BottomOfStringDepth`
   - `AnnulusPressure`
2. `Server` subscribes to:
   - realtime inputs
   - trajectory JSON payload
   - wellbore architecture JSON payload
   - BHA/drill string JSON payload
3. `Server` computes and publishes:
   - `DownholeEquivalentCirculationDensity`
   - `TopLiquidLevelReferenceTVD`
   - `TopLiquidLevelReferencePressure`
4. `DataSink` subscribes to these outputs and logs values.

## Prerequisites

- .NET 8 SDK
- Access to a DWIS-compatible blackboard endpoint
- Configuration for loop timing and blackboard endpoint

Default local configuration is provided in:

- `home/config.json`

Example values:

- `LoopDuration`: `00:00:01`
- `GeneralBlackboard`: `opc.tcp://localhost:48030`

## Build

From repository root:

```sh
dotnet restore
dotnet build DWIS.Service.DownholeECD.sln
```

## Run (Local End-to-End)

Start services in separate terminals from repository root:

```sh
dotnet run --project DWIS.Service.DownholeECD.Server
```

```sh
dotnet run --project DWIS.Service.DownholeECD.DataSource
```

```sh
dotnet run --project DWIS.Service.DownholeECD.DataSink
```

Recommended startup order:

1. `Server`
2. `DataSource`
3. `DataSink`

## Shared Model Generation

To regenerate shared contextual DTOs and merged OpenAPI schema:

```sh
dotnet run --project DWIS.Service.DownholeECD.ModelSharedOut
```

Inputs:

- `DWIS.Service.DownholeECD.ModelSharedOut/json-schemas/*.json`

Generated outputs:

- `DWIS.Service.DownholeECD.ModelSharedOut/DownholeECDMergedModel.cs`
- `DWIS.Service.DownholeECD.Service/wwwroot/json-schema/DownholeECDMergedModel.json`

## Notes

- `DataSource` and `DataSink` are helper applications for integration/testing workflows.
- The core runtime service is `DWIS.Service.DownholeECD.Server`, using contracts and logic from `DWIS.Service.DownholeECD.Model`.
