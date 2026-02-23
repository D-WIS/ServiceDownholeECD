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
- Buffers realtime input/output snapshots and periodically dumps them to file.

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

## Realtime Input/Output Dumping

The worker keeps an in-process buffer of realtime snapshots for:
- `RealtimeInputsData`
- `RealtimeOutputsData`

These snapshots are dumped to JSON files on fixed UTC boundary intervals and the in-memory buffer is cleared after each successful dump to avoid memory growth.

Defaults:
- dump directory: `/home`
- dump interval: `01:00:00` (every plain UTC hour)

Configuration keys (`ConfigurationForDownholeECD`):
- `EnableRealtimeDataDump` (`bool`, default `true`)
- `RealtimeDataDumpDirectory` (`string`, default `"/home"`)
- `RealtimeDataDumpInterval` (`TimeSpan`, default `"01:00:00"`)

Output file naming:
- `downholeecd-realtime-YYYYMMDDTHHMMSSZ.json`

## Docker

Run container:

```sh
docker run -dit --name DWISDownholeECD -v c:\Volumes\DWISServiceDownholeECD:/home digiwells/dwisservicedownholeecdserver:stable
```
