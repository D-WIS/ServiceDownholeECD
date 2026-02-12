# Data Model for Downhole ECD Calculation Service

`DWIS.Service.DownholeECD.Model` contains the domain and semantic data contracts used by the Downhole ECD service to compute **downhole Equivalent Circulation Density (ECD)** from:

- real-time downhole annulus pressure
- bottom-of-string depth
- well trajectory
- wellbore architecture
- BHA/drill string context

The model follows the ECD definition described in **SPE-217668-MS**: https://doi.org/10.2118/217668-MS.

## What This Project Provides

- **Realtime input model** (`RealtimeInputsData`) for dynamic pressure and depth signals.
- **Realtime output model** (`RealtimeOutputsData`) for computed downhole ECD and related reference quantities.
- **Context models** for structural data needed by the calculation:
  - `TrajectoryData`
  - `WellBoreArchitectureData`
  - `BHADrillStringData`
- **Conversion logic** (`DownholePressureConverter`) that transforms pressure-at-depth into density using trajectory interpolation and an inferred top-liquid reference.

## Role in the Solution

This project is the modeling layer for the service pipeline. It defines:

- strongly typed drilling data objects
- semantic annotations used by DWIS/RigOS integration
- manifest/query metadata used to bind data sources and outputs

In short, it provides the canonical input/output and contextual model required by the runtime service that publishes Downhole ECD.
