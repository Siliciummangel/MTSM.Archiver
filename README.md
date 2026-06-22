# MTSM.Archiver

> [!WARNING]
> This project is currently under active development and not yet production ready.

MTSM.Archiver is an open-source archive automation tool.

The project focuses on configuration-driven archival of files and directories.

## Features

- YAML based configuration
- Multiple archive jobs
- Configuration validation
- Command line interface
- Environment validation

## Installation

```bash
git clone https://github.com/Siliciummangel/MTSM.Archiver.git
cd MTSM.Archiver
dotnet build
```

## Commands

> [!NOTE]
> The examples below use dotnet run -- to execute the built binary directly from the project folder on all platforms (Windows, Linux, macOS).

### Show version

```bash
dotnet run -- version
```

### Validate configuration

```bash
dotnet run -- config validate --config root-config.yaml
```

### Validate configuration and environment

```bash
dotnet run -- config validate --config root-config.yaml --check-path-exists
```

## Roadmap

- [x] v0.1.0 Configuration system

- [ ] v0.2.0 Archive execution

- [ ] v0.3.0 TAR support

- [ ] v0.4.0 Logging

- [ ] v0.5.0 Post archive actions

- [ ] v0.6.0 Scheduler

- [ ] v0.7.0 Encryption & Compression tweaks

- [ ] v0.8.0 Stabilization

- [ ] v1.0.0 Production ready