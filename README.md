# LoggerLite.EventLog

[![CI](https://github.com/PFalkowski/LoggerLite.EventLog/actions/workflows/ci.yml/badge.svg)](https://github.com/PFalkowski/LoggerLite.EventLog/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/LoggerLite.EventLog.svg)](https://www.nuget.org/packages/LoggerLite.EventLog/)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=PFalkowski_LoggerLite.EventLog&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=PFalkowski_LoggerLite.EventLog)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://choosealicense.com/licenses/mit/)
[![Buy Me a Coffee](https://img.shields.io/badge/Buy%20Me%20a%20Coffee-support-yellow.svg)](https://www.buymeacoffee.com/piotrfalkowski)

Windows EventLog implementation of the [LoggerLite](https://www.nuget.org/packages/LoggerLite/) `ILogger` interface.

## Install

```bash
dotnet add package LoggerLite.EventLog
```

## Requirements

- Windows (uses `System.Diagnostics.EventLog`)
- Administrator privileges to create new event log sources

## Usage

```csharp
using LoggerLite.EventLog;

// logSource: name of the source within the log
// logName: name of the event log (e.g. "Application")
var logger = new EventLogLogger(logSource: "MyApp", logName: "Application");

logger.LogInfo("Application started");
logger.LogWarning("Disk space low");
logger.LogError("Unhandled exception occurred");
logger.LogError(exception);
logger.LogAuditSuccess("User authenticated");
logger.LogAuditFailure("Login attempt failed");
```

Messages exceeding `MaxSingleMessageLength` (31 837 bytes) are automatically truncated with a `/truncated` suffix.
