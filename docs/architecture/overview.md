# Architecture Overview

MTSM.Archiver is built around a provider-based architecture.

The goal is to separate:

- Source systems
- Archive writers
- Destinations
- Verification
- Post-processing actions

This allows the archive engine to remain independent from specific archive formats and source types.

## Core Components

- IArchiveSource
- IArchiveWriter
- IArchiveDestination
- IArchiveVerifier
- IPostArchiveAction
- IArchiveJobExecutor