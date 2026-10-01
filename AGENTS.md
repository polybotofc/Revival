# AGENTS.md

Persistent notes for working in this repository. Read this before making changes.

## What this repository is

An isolated extraction of the **web backend** from the legacy *Korone* (Pekora /
Project X) Roblox revival. It targets a custom **2021 client (`--clientversion 2021M`)**
and RCC stack. Client/RCC binaries, GridServer, the Node `api`/`frontend` apps and large
assets are intentionally **not** present.

## Layout

- `Roblox.Website/` — ASP.NET Core web app. Controllers live in
  `Controllers/` (`Internal/BypassController.cs` holds the legacy `.ashx` handlers,
  `Internal/WebController.cs` holds the join-script/bootstrapper endpoints).
- `Roblox.Web.Infrastructure/` — request context, session resolution, cookie writing,
  controller base (`RobloxControllerBase`).
- `Roblox.Services/` — domain services (`Games/Games.cs`, `Games/PlaceLauncher.cs`,
  `SessionNegotiationTicketService.cs`, `Signer.cs`).
- `database/` — original knex migrations plus a generated consolidated `schema.sql`.
  Regenerate with `python3 database/tools/generate-schema.py`.
- `Services/Roblox.ServiceDefaults/` — telemetry/service defaults.

## Build and run

```bash
dotnet build Roblox.Website/Roblox.Website.csproj -c Release
dotnet run --project Roblox.Website
```

- SDK is pinned to .NET 10 in `global.json`.
- In a `Debug` build a dev account `ROBLox : roblox_dev_pass` is seeded on first boot.
- PostgreSQL and Redis are required at runtime.

### Building in a minimal container

The SDK needs ICU. If it is unavailable, build with
`DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1` to avoid `Couldn't find a valid ICU package`.

## Configuration

- Commit only `Roblox.Website/appsettings.template.json` and
  `Roblox.Website/game-servers.example.json`. The real `appsettings.json` /
  `game-servers.json` are git-ignored.
- **Never commit private keys.** `Roblox.Website/Keys/` holds only public keys;
  the private keys are git-ignored. Generate your own (see `README.md`).
- RCC SOAP defaults to port `64989` (`Rcc:SoapHost` / `Rcc:SoapPort`).

## 2021 launch flow (do not regress)

- `PlaceLauncherService.RequestGame` issues a short-lived **negotiation ticket**
  (IP-bound, stored in Redis via `SessionNegotiationTicketService`) and returns
  `status: 2` with `joinScriptUrl`, `authenticationUrl` and `authenticationTicket`.
- The ticket is bound to the **hashed** requester IP (`GetIpHash()`); all call sites and
  `Login/Negotiate.ashx` must use the same value.
- `Game/Join.ashx` authenticates via the ticket when no session cookie is present. It
  **peeks** (does not consume) the ticket so the client can still redeem it once at
  `Login/Negotiate.ashx`.
- The join script must include `LocalPlayerInfo` (for `Players:SetLocalPlayerInfo`) and
  `ServerConnections` (for `game:Connect`).

## Notes

- The source is a legacy "vibecoded" codebase. Treat it as untrusted input and review
  security-sensitive changes carefully.
- Client version mapping lives in `GamesService.clientVersionMap`
  (`2021 -> 2021M`). 2018 places still resolve to `2018L`.
