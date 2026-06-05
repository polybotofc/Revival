# Roblox Revival Project - Agent Memory

## Project Overview
ASP.NET Core 8 MVC project for Roblox-style game platform with MySQL database.

## Technology Stack
- ASP.NET Core 8 MVC
- Entity Framework Core 8 with Pomelo MySQL provider
- Session-based authentication with custom ticket system
- HttpClient for RCCService communication

## Key Components

### Models (6 files)
- User - User accounts with profile info
- Session - Authentication session/ticket management
- Game - Game/experience metadata
- Place - Game levels with .rbxl file paths
- Asset - Digital assets (images, models, audio)
- GameServer - Running game server instances

### Services
- AuthService - Registration, login, logout, session management
- GameService - Game CRUD, join logic, server management
- AssetService - Asset metadata and file delivery
- RCCManager - HTTP client for RCCService API

### Controllers
- AccountController - /Account/Login, /Account/Register, /Account/Logout
- HomeController - Home page, games listing, game details
- ProfileController - User profile viewing/editing
- GameController - /Game/Join.ashx, /Game/PlaceLauncher.ashx
- AssetController - /asset/, /thumbs/avatar, /thumbs/game
- InternalApiController - /api/internal/* for RCCService communication

### Database Tables
- Users, Sessions, Games, Places, Assets, GameServers

## Important Paths
- Project root: `/workspace/project/Revival/Revival`
- Models: `Models/`
- Services: `Services/`
- Controllers: `Controllers/`
- Views: `Views/`
- Static files: `wwwroot/`
- Config: `appsettings.json`

## Build & Run
```bash
cd /workspace/project/Revival/Revival
dotnet restore
dotnet run
```

## External Services
- RCCService: http://localhost:3000 (configurable in appsettings.json)
- MySQL: localhost:3306 (configurable in ConnectionStrings)

## Session Cookie
- Name: "RevivalSession"
- HttpOnly, SameSite=Strict
- 24-hour expiration

## Game Join Flow
1. Client → /Game/Join.ashx?placeId=X
2. Server validates session
3. Server creates auth ticket
4. Server creates/finds game server via RCCService
5. Returns: OK|serverId|address|port|ticket