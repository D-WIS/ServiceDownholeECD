# Data Sink for Downhole ECD Service

`DWIS.Service.DownholeECD.DataSink` is a .NET worker service that subscribes to and reads the computed Downhole ECD outputs from the DWIS blackboard.

It reads:

- downhole equivalent circulation density (ECD)
- top liquid level reference pressure
- top liquid level reference TVD

## What This Project Does

- Connects to the DWIS blackboard.
- Registers queries for `RealtimeOutputsData`.
- Runs a periodic read loop to fetch the latest computed values.
- Logs output values for runtime monitoring and validation.

## Runtime Role

This project acts as a lightweight consumer/observer in the pipeline:

1. `DataSource` publishes realtime input signals.
2. `Server` computes Downhole ECD and publishes outputs.
3. `DataSink` reads and displays those outputs.

It is primarily intended for integration testing, demonstrations, and quick verification that the full data path is functioning correctly.
