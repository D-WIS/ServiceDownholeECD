# Data Source for Downhole ECD Service

`DWIS.Service.DownholeECD.DataSource` is a .NET worker service that publishes synthetic real-time input data used by the Downhole ECD server.

It generates and publishes:

- bottom-of-string depth
- downhole annulus pressure

## What This Project Does

- Connects to the DWIS blackboard.
- Registers the realtime input data model (`RealtimeInputsData`) for publishing.
- Runs a periodic loop that simulates drilling progression and hydrostatic pressure behavior.
- Publishes updated input values at each cycle.
- Logs generated values for monitoring and debugging.

## Data Generation Logic

The worker initializes a simple physical scenario and updates it each tick:

- increases measured depth with a fixed step
- updates TVD from inclination
- computes annulus pressure from:
  - atmospheric reference pressure
  - fluid density
  - gravitational acceleration
  - current TVD

This provides a deterministic input stream that can be used to validate the Downhole ECD computation pipeline end-to-end.

## Role in the Solution

This project is a helper publisher for development, integration testing, and demonstrations. It supplies the realtime inputs consumed by `DWIS.Service.DownholeECD.Server`, without requiring a live field data feed.
