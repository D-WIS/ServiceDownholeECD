# Shared Model Generator for Downhole ECD

`DWIS.Service.DownholeECD.ModelSharedOut` is a code-generation utility project that builds the shared DTO model used for contextual data in the Downhole ECD solution.

It produces:

- a merged OpenAPI schema bundle (`DownholeECDMergedModel.json`)
- generated C# DTO/client classes (`DownholeECDMergedModel.cs`)

## What This Project Does

- Reads dependency OpenAPI schemas from `json-schemas/`.
- Merges schema documents into one normalized OpenAPI bundle.
- Rewrites schema references to consistent short type names.
- Generates C# model/client code from the merged schema using NSwag.
- Writes the merged JSON schema to the service static schema folder for downstream exposure/consumption.

## Inputs and Outputs

- **Input**: local schema dependency files in `json-schemas/*.json`.
- **Output (C#)**: `DWIS.Service.DownholeECD.ModelSharedOut/DownholeECDMergedModel.cs`.
- **Output (OpenAPI JSON)**: `DWIS.Service.DownholeECD.Service/wwwroot/json-schema/DownholeECDMergedModel.json`.

## Role in the Solution

This project provides the canonical shared contextual model for:

- trajectory data
- wellbore architecture data
- drill string/BHA data

The generated types are consumed by the model and server projects so contextual payloads can be serialized, deserialized, and exchanged consistently across the Downhole ECD pipeline.
