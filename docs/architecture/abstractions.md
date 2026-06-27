# Core Abstractions

## IArchiveSource

Provides archive items from a source system.

Examples:

- File system
- Database
- S3 bucket

## IArchiveWriter

Creates an archive or export artifact.

Examples:

- ZIP
- TAR
- SQL Dump
- CSV Export
- Parquet Export

## IArchiveDestination

Provides the output target for generated artifacts.

Examples:

- Local filesystem
- Network share
- S3 bucket

## IArchiveVerifier

Validates a generated archive.

Examples:

- File exists
- Checksum verification
- Archive integrity checks

## IPostArchiveAction

Executes follow-up actions after a successful archive run.

Examples:

- Delete source files
- Move source files
- Create manifest