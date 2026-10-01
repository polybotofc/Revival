# Revival — Isolated Korone Web Backend (2021 Client / RCC)

This repository contains only the **web application / backend** extracted from the
legacy *Korone* (formerly Pekora / Project X) Roblox revival stack, adapted to serve a
custom **2021 client (`--clientversion 2021M`) and RCC** deployment.

All legacy client binaries, `RCCService`/`RCCService2020` executables, GridServer
binaries, the React/`frontend` app, and unused assets were intentionally excluded.
What remains is the ASP.NET Core web backend, the shared service/library projects it
depends on, the database migrations, and the configuration templates needed to run it.

## What is included

| Path | Purpose |
| --- | --- |
| `Roblox.Website/` | The web application: controllers, Razor pages, API endpoints (`Join.ashx`, `PlaceLauncher.ashx`, `Negotiate.ashx`, v1/v2 APIs), middleware and startup. |
| `Roblox.Web.Infrastructure/` | Request context, session handling, controller base classes, auth/cookie helpers. |
| `Roblox.Services/` | Domain services: games, place launcher, signer, session-negotiation tickets, economy, assets, etc. |
| `Roblox.Models`, `Roblox.Dto`, `Roblox.Exceptions` | Data models, DTOs and exception types. |
| `Roblox.Libraries`, `Roblox.Rendering*`, `Roblox.Cache`, `Roblox.Metrics`, `Roblox.Logging`, `Roblox.Configuration` | Supporting libraries. |
| `Roblox.AbuseDetection`, `Roblox.EconomyChat`, `Services/Roblox.ServiceDefaults` | Optional runtime components referenced by the website. |
| `database/` | The original knex migrations plus a generated, consolidated PostgreSQL `schema.sql`. |

> **Not included:** `RCCService2018`/`RCCService2020` binaries, GridServer, the legacy
> Node `api`/`frontend` applications, client binaries and large assets. You must supply
> your own 2021 client and RCCService build.

## Requirements

- .NET SDK 10.0 (`global.json` pins `10.0.0` with `latestMajor` roll-forward).
- PostgreSQL (the migrations target Postgres; several columns use `macaddr`, `uuid`
  and `jsonb`).
- Redis (sessions, tickets, caching and locks).
- A 2021 RCCService build and an arbiter/renderer endpoint reachable by the backend.
- A 2021 client build for testing joins.

## 1. Set up the database

The schema is provided in two forms.

**Option A — run the consolidated SQL (recommended for a clean install):**

```bash
createdb korone
psql -d korone -f database/schema.sql
```

`schema.sql` is generated from `database/migrations/*.js`. Regenerate it after adding
migrations:

```bash
python3 database/tools/generate-schema.py
```

**Option B — use the original knex migrations** if you prefer the Node toolchain:

```bash
cd database
npm install knex pg
npx knex --knexfile knexfile.js migrate:latest
```

## 2. Configure the backend

Copy the templates and fill in every `<PLACEHOLDER>`:

```bash
cp Roblox.Website/appsettings.template.json Roblox.Website/appsettings.json
cp Roblox.Website/game-servers.example.json Roblox.Website/game-servers.json   # optional
```

Generate your own join-script signing keys (the private keys are **not** committed and
are git-ignored):

```bash
openssl genrsa -out Roblox.Website/Keys/PrivateKey2048.pem 2048
openssl rsa -in Roblox.Website/Keys/PrivateKey2048.pem -pubout -out Roblox.Website/Keys/PublicKey2048.pem

# Legacy 1024-bit CSP blob used by SignService for --rbxsig
openssl genrsa -out /tmp/legacy.pem 1024
openssl rsa -in /tmp/legacy.pem -traditional -out Roblox.Website/Keys/PrivateKeyBlob.pem
openssl rsa -in /tmp/legacy.pem -pubout -out Roblox.Website/Keys/PublicKey.pem
# PrivateKeyBlob.txt must be the base64 CSP blob expected by ImportCspBlob; derive it
# from PrivateKeyBlob.pem with your key tooling, or reuse a compatible key pair.
```

The public keys (`PublicKey.pem`, `PublicKey2048.pem`, `PublicKeyBlob.txt`,
`PublicKeyBlob2048.bin`) are committed because the client embeds them for verification;
rotate them only together with the client build.

Key settings:

| Setting | Meaning |
| --- | --- |
| `Postgres` | Npgsql connection string for the database you created above. |
| `Redis` / `RedisAuthentication` | Redis host:port and optional password. |
| `BaseUrl` | Public HTTPS base URL, e.g. `https://<DOMAIN>`. Used in join/negotiate URLs. |
| `Jwt:Sessions` | Signing key for session JWTs (at least 32 bytes). |
| `Rcc:SoapHost` / `Rcc:SoapPort` | RCCService SOAP listener. **Default SOAP port is `64989`.** |
| `RccAuthorization` | Shared secret between the web backend and RCC. |
| `Render:BaseUrl` / `ArbiterAuthorization` | Arbiter/renderer endpoint and its shared secret. |
| `GameServerIp` | IP that clients should connect to for game servers. |

`appsettings.json` and `game-servers.json` are git-ignored on purpose; the committed
`*.template.json` / `*.example.json` files are safe placeholders. Do not commit real
secrets.

## 3. Run the web backend

```bash
dotnet restore
dotnet run --project Roblox.Website
```

In a `Debug` build the app seeds a development account on first boot:

```
ROBLOX : roblox_dev_pass
```

## 2021 client / RCC adaptations

These changes make the launcher and join flow match what a 2021 (`2021M`) client expects.

- **`Game/PlaceLauncher.ashx`** returns the 2021 launch payload:

  ```json
  {
    "jobId": "<JOB_ID>",
    "status": 2,
    "joinScriptUrl": "https://<DOMAIN>/Game/Join.ashx?placeId=<PLACE_ID>&ticket=<TICKET>&jobId=<JOB_ID>",
    "authenticationUrl": "https://<DOMAIN>/Login/Negotiate.ashx",
    "authenticationTicket": "<TICKET>"
  }
  ```

  `status: 2` means "joining". The `authenticationTicket` is a short-lived
  session-negotiation ticket bound to the caller's IP, **not** the raw `.PUPPYSECURITY`
  cookie.

- **`Game/Join.ashx`** accepts `placeId` and `ticket`. When the 2021 client calls it
  without a session cookie, the ticket is peeked (not consumed) to resolve the session,
  so the same ticket can still be redeemed once at `Login/Negotiate.ashx`. A supplied
  `placeId` is validated against the job's place.

- **Join script** includes 2021 Luau launch data: `LocalPlayerInfo` (for
  `Players:SetLocalPlayerInfo`) and `ServerConnections` (the address/port the client
  dials via `game:Connect`).

- **Launch arguments** for the bootstrapper now include `--clientversion 2021M` and pass
  `--authenticationTicket` / `--authenticationUrl` as separate, unescaped arguments:

  ```
  --authenticationUrl https://<DOMAIN>/Login/Negotiate.ashx --authenticationTicket <TICKET> --clientversion 2021M --joinScriptUrl https://<DOMAIN>/Game/PlaceLauncher.ashx?request=RequestGame&placeId=<PLACE_ID>&isPartyLeader=false&gender=&isTeleport=true
  ```

  The RCC SOAP port defaults to `64989` and is configurable through `Rcc:SoapHost` /
  `Rcc:SoapPort`.

## Client version target

This extraction targets the **2021 `2021M`** client. **2021 is the minimum supported
year:** `GamesService.SetYear`, `GetJoinScript`, `SignJoinScript`, `PlaceLauncher` and
the launcher/join controllers all reject any place with `year < 2021`. The
`clientVersionMap` only contains `2021 -> 2021M`; there is no fallback to older clients.
Older PRs referenced a May 2018 target, which is intentionally superseded here.

## Notes

- This codebase is a legacy "vibecoded" revival backend. Treat it as untrusted input:
  rotate all placeholder secrets, review the authorization middleware, and do not expose
  it to the public internet without a security review.
- The schema generator handles the knex subset used by these migrations. If you add a
  migration using a builder method it does not recognize, extend
  `database/tools/generate-schema.py` and regenerate.
