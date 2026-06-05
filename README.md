# Roblox Revival Platform

A complete ASP.NET Core 8 MVC implementation for a Roblox-style multiplayer game platform with MySQL database support.

**Target Roblox Client Version: 0.338.0.202976 (May 2018)**

## Features

### Core Features
- **User Authentication**: Register, Login, Logout with secure session tickets
- **Profile Management**: View and edit user profiles with avatars
- **Game System**: Create, browse, and play games
- **Asset Delivery**: Serve game assets, avatars, and thumbnails
- **Game Server Management**: Create and manage game servers via RCCService
- **Place Launcher**: Launch game places with Roblox client compatibility

### Technical Stack
- ASP.NET Core 8 MVC
- Entity Framework Core 8
- MySQL (via Pomelo.EntityFrameworkCore.MySql)
- Bootstrap 5 for responsive UI
- Cookie-based session authentication
- RCCService integration for game server management

## Project Structure

```
Revival/
├── Controllers/                    # MVC Controllers
│   ├── AccountController.cs        # Authentication (Login, Register, Logout)
│   ├── AssetController.cs          # Asset Delivery System
│   ├── GameController.cs           # Game Endpoints (Join.ashx, PlaceLauncher.ashx)
│   ├── HomeController.cs          # Home & Games listing
│   ├── InternalApiController.cs   # Internal API for RCCService
│   └── ProfileController.cs        # User Profile management
├── Data/
│   ├── RevivalDbContext.cs         # EF Core DbContext
│   └── SeedData.cs                # Initial database seed data
├── Middleware/
│   └── RequestLoggingMiddleware.cs # HTTP request logging
├── Models/
│   ├── User.cs                    # User model
│   ├── UserSession.cs             # Session/Ticket model
│   ├── Game.cs                   # Game model
│   ├── Place.cs                  # Place model
│   ├── Asset.cs                  # Asset model
│   ├── GameServer.cs             # Game Server model
│   └── GameServerPlayer.cs        # Player on server model
├── Services/
│   ├── AuthService.cs             # Authentication service
│   ├── GameService.cs            # Game management service
│   ├── AssetService.cs           # Asset delivery service
│   └── RCCManager.cs             # RCCService communication
├── Views/                        # Razor Views
├── Migrations/                   # EF Core migrations
├── wwwroot/                      # Static files (CSS, JS, images)
├── schema.sql                    # MySQL database schema
├── appsettings.json              # Configuration
└── Program.cs                    # Application entry point
```

## Database Schema

### Tables
| Table | Description |
|-------|-------------|
| Users | User accounts with profile information |
| Sessions | Authentication session tickets |
| Games | Game/experience metadata |
| Places | Game levels with .rbxl file paths |
| Assets | Digital assets for delivery |
| GameServers | Running game server instances |
| GameServerPlayers | Player sessions on servers |

## API Endpoints

### Web Pages
| Endpoint | Description |
|---------|-------------|
| `/` | Home page with game listings |
| `/Home/Games` | Browse all games |
| `/Home/GameDetails/{id}` | View game details |
| `/Account/Login` | User login page |
| `/Account/Register` | User registration page |
| `/Profile/{username}` | View user profile |
| `/Profile/Edit` | Edit profile |

### Game Endpoints (Roblox Client Compatible)
| Endpoint | Description |
|---------|-------------|
| `/Game/Join.ashx?placeId=` | Join a game server |
| `/Game/PlaceLauncher.ashx?placeId=` | Launch a place |
| `/Game/GetGames` | Get games list (JSON) |

### Asset Delivery
| Endpoint | Description |
|---------|-------------|
| `/asset/?id=` | Deliver asset by ID |
| `/thumbs/avatar?userId=` | Get user avatar thumbnail |
| `/thumbs/game?gameId=` | Get game thumbnail |

### Internal API
| Endpoint | Method | Description |
|---------|--------|-------------|
| `/api/internal/server/create` | POST | Create game server |
| `/api/internal/server/stop` | POST | Stop game server |
| `/api/internal/server/status/{id}` | GET | Get server status |
| `/api/internal/game/join` | POST | Join game |
| `/api/internal/script/execute` | POST | Execute Lua script |

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=Revival;User=root;Password=your_password;"
  },
  "RCCService": {
    "Enabled": true,
    "BaseUrl": "http://localhost:3000",
    "TimeoutSeconds": 30
  },
  "Authentication": {
    "CookieName": "RevivalAuth",
    "SessionExpirationHours": 24,
    "SecureCookies": false
  },
  "GameServer": {
    "DefaultMaxPlayers": 50,
    "ServerExpirationMinutes": 120
  }
}
```

## Getting Started

### Prerequisites
- .NET 8 SDK
- MySQL 8.0+
- RCCService (optional, for full functionality)

### Installation

1. **Clone the repository**
```bash
git clone <repository-url>
cd Revival/Revival
```

2. **Install dependencies**
```bash
dotnet restore
```

3. **Configure MySQL**
Update `appsettings.json` with your MySQL connection string:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=Revival;User=root;Password=your_password;"
}
```

4. **Create the database**
Option A: Use Entity Framework migrations
```bash
dotnet ef database update
```

Option B: Run the SQL script directly
```bash
mysql -u root -p < schema.sql
```

5. **Run the application**
```bash
dotnet run
```

6. **Access the application**
Open http://localhost:5000 in your browser.

### Default Accounts
After seeding, these accounts are created:
- **Admin**: `Admin` / `Admin123!`
- **Player**: `PlayerOne` / `Player123!`

## Game Join Flow

1. Client calls `/Game/Join.ashx?placeId=X`
2. Server validates session authentication
3. Server creates authentication ticket
4. Server creates/finds game server via RCCService
5. Server returns: `OK|serverId|address|port|ticket`
6. Client connects to game server

## Game Server Management

### RCCService Integration
The RCCManager service handles:
- Creating new game servers
- Stopping running servers
- Executing Lua scripts
- Server health monitoring

### PlaceLauncher Format
Returns pipe-delimited string:
```
machineAddress|machinePort|placeId|gameId|serverId|ticket|placeVersionId
```

## Entity Framework Migrations

### Create Migration
```bash
dotnet ef migrations add InitialCreate
```

### Update Database
```bash
dotnet ef database update
```

### Generate SQL Script
```bash
dotnet ef migrations script -o schema.sql
```

## Development

### Run in Development Mode
```bash
dotnet run --environment Development
```

### Enable Detailed Logging
Update `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

## Security Features

- BCrypt password hashing
- Session token expiration
- Account lockout after failed login attempts
- Input validation and sanitization
- SQL injection prevention via EF Core
- XSS protection via Razor

## License

MIT License - See LICENSE file for details.

---

Built with ASP.NET Core 8, Entity Framework Core, and MySQL. Compatible with Roblox client version 0.338.0.202976.