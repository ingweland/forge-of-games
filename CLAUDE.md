# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Forge of Games (forgeofgames.com) — a companion website for the mobile game *Heroes of History* (HoH). It provides a City Planner, Command Center, Stats Hub, calculators, and other tools. .NET 9 solution (`ForgeOfGames.sln`), Blazor WebAssembly frontend, ASP.NET Core backend, Azure Functions for background data processing. Root namespace prefix is `Ingweland.Fog.*`.

## Rules for working in this repo

- **Never build the whole solution.** To verify your changes, build only the affected project(s), e.g. `dotnet build .\src\WebApp\WebApp.csproj`. Success criterion is simply that it compiles without errors.
- **Never run anything** — no `dotnet run`, no `func start`, no launching WebApp or Functions locally. Build-only verification.
- **Never run scripts** from `src/scripts/`.
- **Database:** never apply or push migrations (`dotnet ef database update` is forbidden). The only allowed EF command is creating a migration:
  ```powershell
  dotnet ef migrations add <name> --project .\src\Infrastructure\ --startup-project .\src\WebApp\
  ```

There are no test projects in the solution.

## Architecture

Layered solution under `src/`, wired together via per-project `DependencyInjection.cs` extension methods (`AddInfrastructureServices()`, `AddApplicationServices()`, etc.) that are composed in each host's `Program.cs`.

**Hosts:**
- `WebApp` — ASP.NET Core host. Serves the Blazor WASM client (`MapRazorComponents<App>().AddInteractiveWebAssemblyRenderMode()`) plus minimal-API endpoints (`MapHohApi()`, `MapStatsApi()`, `MapFogApi()` in `Apis/`). Middleware for maintenance mode and domain restriction.
- `WebApp.Client` — Blazor WebAssembly client (MudBlazor + Syncfusion UI). Pages/components in `Components/`, calls WebApp APIs.
- `Functions` — Azure Functions (isolated worker). Timer/queue-triggered jobs that fetch game data via the Inn SDK, process player/alliance/battle stats, and write to SQL + Table Storage. Durable orchestration lives in `Orchestration/`.

**Application layers:**
- `Application.Server` — server-side business logic (MediatR handlers, stats hub, battle processing, caching). Used by WebApp and Functions.
- `Application.Client.Web` / `Application.Client.Core` — client-side logic: view models, factories, City Planner / Command Center / Stats Hub UI logic, localization resources.
- `Application.Core` — logic shared by server and client (calculators, helpers, `FogUrlBuilder` for routes).
- `Shared` — low-level cross-cutting utilities used everywhere.

**Data and models** (separate model projects — don't conflate them):
- `Infrastructure` — EF Core (`FogDbContext`, SQL Server, connection string `DefaultSQL`), EF migrations, plus Azure Table Storage / Blob Storage / Queue repositories. Game core data (unit/building/hero definitions) is read from Azure Blob Storage, not SQL.
- `Models.Hoh` — game-domain models/enums (heroes, buildings, units, battles). Used directly throughout the codebase, not just for transfer.
- `Models.Fog` — site-domain models, including the EF entities persisted via `FogDbContext` (alliances, rankings, battle stats, player profiles).
- `Dtos.Hoh` — the dedicated DTO project: wire-transfer types between WebApp APIs and the WASM client, with protobuf serialization (generated from `src/protos/hoh/*.proto`).
- `Inn/InnModels.Hoh` — models mirroring the game's own backend API responses; used by `Inn/InnSdk.Hoh`, the SDK for that API (auth, game-design data fetching). `Misc/HohCoreDataParserSdk` parses raw game data into the app's models.

**Data flow (big picture):** Functions pull raw data from the game servers via InnSdk → parse with HohCoreDataParserSdk → persist to SQL/Table/Blob storage → WebApp reads via Application.Server + Infrastructure → serves DTOs (JSON or protobuf) to the WASM client.

## Localization

UI strings live in `src/Application.Client.Core/Localization/FogResource.resx` with per-locale variants (`FogResource.de.resx`, etc.; ~10 locales). When adding UI strings, add the English key to `FogResource.resx` and translate to all locale files — the translation workflow/prompt is documented in `guides/prompts/localization.txt`. `FogResource.Designer.cs` is generated.

## Non-code directories

- `guides/commands.txt` — useful one-off commands (asset sync to S3 CDN, cert conversion, proto decoding); `guides/sql_queries.txt` — handy SQL.
- `social/` — Discord announcement drafts (multi-language); not code.
- `src/scripts/` — Python utility scripts for asset/data wrangling (has its own `.venv`).
- `ui/` — Figma design file.

## Deployment

GitHub Actions (`.github/workflows/`) build and deploy to Azure App Service (staging + production). Configuration uses Azure App Configuration in production and user secrets / `appsettings.Development.json` locally.
