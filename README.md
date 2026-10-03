> [!CAUTION]
> Some parts of the source code is __AI-generated__ (or vibecoded), use it at your own risk.

> [!NOTE]
> **2021-only fork.** This copy is locked to the 2021 client (`2021M`) only.
> Requests to launch a place whose `year` is not 2021 are rejected, and only
> `2021` may be set as a place year. Catalog, games, economy and other features
> are unchanged. See "2021-only adaptation" below.

# Korone Source Code
Korone (formerly Pekora or Project X) is a popular modern Roblox revival featuring years from 2017 to 2021

We sincerely don't recommend hosting this source code for a number of reasons
1. It's vibecoded via Claude and GitHub Copilot which is a huge red flag for a Roblox revival's stability and security
2. Korone is extremely controversial and you'll definitely get mocked (or attacked) for even hosting it again
3. The people behind the source leak are the same people who put a backdoor on the August 2025 source code which put a reverse shell onto your computer (**[#7](https://github.com/muserlogiccpu/korone-src/issues/7)**)

If you really want to make a Roblox revival then learn how to code and reverse engineer firstly as revivals aren't an easy thing to develop.

Older versions:
- https://github.com/Anontux/pekora-latest-src - 30 August 2025

oh also this is just economy simulator to host it figure out urself idfk how to host it too

## 2021-only adaptation

This fork accepts the 2021 client (`2021M`) only. The legacy 2017/2018/2019/2020
launch paths were removed from the web backend; catalog, games, economy, avatar
and every other feature are untouched.

Where the year gate lives:

| File | Change |
| --- | --- |
| `Roblox/Roblox.Services/Games/Games.cs` | `clientVersionMap` only maps `2021 -> 2021M`; `AllowedGameYears` = `{2021}`; `SignJoinScript` signs with the 2048-bit key and rejects any other year. |
| `Roblox/Roblox.Services/Signer.cs` | `GenerateClientTicket` only handles `2021` (V4 ticket); other years throw. |
| `Roblox/Roblox.Services/Games/PlaceLauncher.cs` | Place launch and cloud-edit reject any place whose `year != 2021`. |
| `Roblox/Roblox.Website/Controllers/Internal/BypassController.cs` | Mobile join only allows `2021`; the pre-2020 membership downgrade was removed. |
| `Roblox/Roblox.Website/Controllers/RobloxApi/FeatureFlagsRoblox.cs` | Application allow-list keeps only 2021-era names (`PCDesktopClient2021`, `PCStudio223`, ...). |
| `Roblox/Korone.RccServiceArbiter/Configuration/ArbiterOptions.cs` | `Render.DefaultYear` defaults to `2021`, so the arbiter targets `RCCService2021`. |
| `api/migrations/20260506013844_addAssetPlaceYearAndRbxPlaceId.js` | New places default to `year = 2021`. |
| `frontend/components/updatePlace/components/access.js` | The place year selector only offers `2021`. |

Point your own 2021 RCCService build at the path the backend expects (the
`RCCService/` binaries are not included in this repo):

- Arbiter/renderer: `RCCService2021/RCCService.exe` (year is driven by `Render.DefaultYear`).
- Docker game server: `game-server/start-rcc.sh` expects `RCCService/content` at `/usr/src/app/RCCService/content`.

To grant an existing place the right to launch, set its year to 2021:

```sql
UPDATE asset_place SET year = 2021 WHERE asset_id = <PLACE_ID>;
```

or use the API `PATCH /v1/universes/{universeId}/set-year` with `{ "year": 2021 }`.
