# Roblox Revival Project - Agent Memory

## Project Overview
ASP.NET Core 8 MVC project for Roblox-style game platform with MySQL database.
Target Roblox Client: Version 0.338.0.202976 (May 2018)

## Technology Stack
- ASP.NET Core 8 MVC
- Entity Framework Core 8 with Pomelo MySQL provider
- BCrypt for password hashing
- Serilog for logging
- Session-based authentication with custom ticket system
- HttpClientFactory for RCCService communication

## Key Components

### Models (7 files)
- User - User accounts with profile info, reputation, social counts
- UserSession - Authentication session/ticket management
- Game - Game/experience metadata with ratings, plays
- Place - Game levels with .rbxl file paths
- Asset - Digital assets with Roblox types
- GameServer - Running game server instances
- GameServerPlayer - Player sessions on servers

### Services
- AuthService - Registration, login, logout, session management
- GameService - Game CRUD, join logic, server management
- AssetService - Asset metadata and file delivery
- RCCManager - HTTP client for RCCService API

### Controllers
- HomeController - Home page, games listing
- AccountController - Login, Register, Logout
- ProfileController - Profile viewing/editing
- GameController - Join.ashx, PlaceLauncher.ashx
- AssetController - Asset delivery endpoints
- InternalApiController - Internal API for RCCService

### Database Tables
- Users, Sessions, Games, Places, Assets, GameServers, GameServerPlayers

## Important Paths
- Project root: /workspace/project/Revival/Revival
- Models: Models/
- Services: Services/
- Controllers: Controllers/
- Views: Views/
- SQL Schema: schema.sql

## Build & Run
- dotnet restore
- dotnet run (runs on http://localhost:5000)

## Session Cookie
- Name: RevivalAuth
- HttpOnly, SameSite=Lax
- 24-hour expiration

## Game Join Flow
1. Client to /Game/Join.ashx?placeId=X
2. Server validates session
3. Server creates auth ticket
4. Server creates/finds game server
5. Returns: OK|serverId|address|port|ticket

## Seed Data
- Admin: Admin/Admin123!
- Player: PlayerOne/Player123!
