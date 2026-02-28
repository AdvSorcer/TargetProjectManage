# AGENTS.md

## Cursor Cloud specific instructions

### Overview
This is a monolithic ASP.NET Core Razor Pages project management system (專案管理系統) targeting **.NET 10** (`net10.0`). It uses an embedded **SQLite** database that auto-creates and auto-migrates on startup — no external services required.

### Prerequisites
- .NET 10 SDK must be installed. The VM update script handles `dotnet restore` only; if the SDK is missing, install it via:
  ```
  curl -L https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh && chmod +x /tmp/dotnet-install.sh
  sudo /tmp/dotnet-install.sh --channel 10.0 --install-dir /usr/share/dotnet
  sudo ln -sf /usr/share/dotnet/dotnet /usr/local/bin/dotnet
  ```

### Common commands
| Task | Command | Working dir |
|------|---------|-------------|
| Restore | `dotnet restore` | `TargetProjectManage/` |
| Build | `dotnet build` | `TargetProjectManage/` |
| Run (dev) | `dotnet run --launch-profile http` | `TargetProjectManage/` |
| Run (container) | `docker-compose up --build` | repo root |

### Dev server
- The `http` launch profile listens on **http://localhost:5001** (no HTTPS in dev).
- The SQLite database file is created at `TargetProjectManage/Data/ProjectManage.db` in development mode. Seed data is auto-inserted on first run.
- Hot reload is supported via `dotnet watch run` if preferred.

### Gotchas
- The project targets `net10.0` (preview/LTS). Ensure the .NET 10 SDK is installed, not .NET 8 or 9.
- No `UseHttpsRedirection` in development — the `http` profile intentionally skips HTTPS to avoid port-detection warnings.
- No automated test project exists in the repo; `dotnet build` with zero errors/warnings is the primary compile-time check.
- The `.cursor/rules/project-rule.mdc` specifies using C# 10+ syntax and verifying builds with `dotnet build`.
