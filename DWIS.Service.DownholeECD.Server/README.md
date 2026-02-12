# Service: Downhole ECD Server

`DWIS.Service.DownholeECD.Server` is a .NET worker service that runs the real-time Downhole ECD computation loop and integrates with the DWIS blackboard.

It consumes:

- real-time downhole annulus pressure and bottom-of-string depth
- contextual trajectory data
- wellbore architecture data
- BHA/drill string description

It computes and publishes:

- downhole equivalent circulation density (ECD)
- top liquid level reference TVD
- top liquid level reference pressure

## What This Project Does

- Hosts a background worker (`Worker`) that connects to the DWIS blackboard.
- Registers model queries for required input and contextual data.
- Reads live data on each loop tick, executes the pressure-to-ECD conversion, and publishes outputs.
- Logs key input/output values for operational monitoring.

## Runtime Flow

1. Connect to the blackboard.
2. Register subscriptions/queries for realtime and contextual inputs.
3. Register output channels for computed ECD-related values.
4. On each cycle:
   - read latest inputs
   - deserialize trajectory/architecture/drill string payloads
   - run `DownholePressureConverter.Process(...)`
   - publish computed outputs

## Dependencies

This project depends on:

- `DWIS.Service.DownholeECD.Model` for the data contracts and conversion logic
- `DWIS.RigOS.Common.Worker` infrastructure (through the model dependency chain)

The ECD conversion logic is aligned with the definition described in **SPE-217668-MS**: https://doi.org/10.2118/217668-MS.

## Docker

Run container:

```sh
docker run -dit --name DWISDownholeECD -v c:\Volumes\DWISServiceDownholeECD:/home digiwells/dwisservicedownholeecdserver:stable
```
