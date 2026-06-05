# Roblox Revival - ASP.NET Core 8 MVC Project

Proyek Roblox Revival adalah implementasi server website untuk platform game multiplayer, menggunakan ASP.NET Core 8 MVC dan MySQL.

## Fitur

### Autentikasi
- Register, Login, Logout
- Session management dengan authentication ticket
- Profile management

### Game System
- Daftar game publik
- Game server management
- Place launcher untuk client Roblox

### Asset Delivery
- `/asset/?id=` - Delivery asset berdasarkan ID
- `/thumbs/avatar` - Avatar thumbnail user
- `/thumbs/game` - Game thumbnail

### Game Endpoints (Roblox Client)
- `/Game/Join.ashx` - Join game dan dapatkan server info
- `/Game/PlaceLauncher.ashx` - Launch place dan connect ke server

### RCCService Integration
- Membuat game server baru
- Menghentikan game server
- Eksekusi Lua script
- Status monitoring

## Struktur Folder

```
Revival/
├── Controllers/
│   ├── AccountController.cs      # Autentikasi (login, register, logout)
│   ├── AssetController.cs        # Asset delivery system
│   ├── GameController.cs         # Game endpoints (Join.ashx, PlaceLauncher.ashx)
│   ├── HomeController.cs         # Halaman utama
│   ├── InternalApiController.cs  # Internal API untuk RCCService
│   └── ProfileController.cs       # Profile management
├── Data/
│   └── RevivalDbContext.cs       # EF Core DbContext
├── Models/
│   ├── Asset.cs                  # Asset model
│   ├── Game.cs                   # Game model
│   ├── GameServer.cs             # Game server model
│   ├── Place.cs                  # Place model
│   ├── Session.cs                # Session/ticket model
│   └── User.cs                   # User model
├── Services/
│   ├── AssetService.cs           # Asset management service
│   ├── AuthService.cs            # Authentication service
│   ├── GameService.cs            # Game management service
│   └── RCCManager.cs             # RCCService communication
├── Views/
│   ├── Account/
│   │   ├── Login.cshtml
│   │   └── Register.cshtml
│   ├── Home/
│   │   ├── Games.cshtml
│   │   └── Index.cshtml
│   ├── Profile/
│   │   └── ViewProfile.cshtml
│   └── Shared/
│       └── _Layout.cshtml
├── wwwroot/
│   ├── css/site.css
│   └── js/site.js
├── appsettings.json
└── Program.cs
```

## Database Schema

### Users
| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| Username | varchar(50) | Unique username |
| Email | varchar(255) | User email |
| PasswordHash | varchar(255) | Hashed password |
| DisplayName | varchar(255) | Display name |
| Description | varchar(500) | Bio/description |
| CreatedAt | datetime | Account creation date |
| LastLoginAt | datetime | Last login time |
| IsBanned | bool | Ban status |
| AvatarAssetId | int | Avatar asset ID |

### Sessions
| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| SessionToken | varchar(64) | Unique session token |
| UserId | int | FK to Users |
| IpAddress | varchar(45) | Client IP |
| CreatedAt | datetime | Session start |
| ExpiresAt | datetime | Session expiration |
| IsActive | bool | Active status |

### Games
| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| Name | varchar(100) | Game name |
| Description | varchar(1000) | Game description |
| CreatorId | int | FK to Users |
| TotalPlays | int | Total play count |
| ActivePlayers | int | Current players |
| IsPublic | bool | Public status |

### Places
| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| Name | varchar(100) | Place name |
| GameId | int | FK to Games |
| FilePath | varchar(500) | .rbxl file path |
| MaxPlayers | int | Max players |

### Assets
| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| Name | varchar(100) | Asset name |
| Type | enum | Asset type |
| FilePath | varchar(500) | File path |
| FileSize | long | File size |
| ContentType | varchar(100) | MIME type |
| Hash | varchar(255) | Content hash |

### GameServers
| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| ServerId | varchar(36) | Unique server ID |
| PlaceId | int | FK to Places |
| Host | varchar(45) | Server host |
| Port | int | Server port |
| Status | varchar(20) | Server status |
| RCCJobId | varchar(500) | RCC job ID |

## Konfigurasi

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=Revival;User=root;Password=your_password;"
  },
  "RCCService": {
    "Url": "http://localhost:3000",
    "ApiKey": "",
    "Timeout": 30
  },
  "Authentication": {
    "SessionExpirationHours": 24,
    "SecureCookies": false
  }
}
```

## Cara Menjalankan

### 1. Install Dependencies
```bash
cd Revival
dotnet restore
```

### 2. Setup Database
Pastikan MySQL berjalan dan buat database:
```sql
CREATE DATABASE Revival;
```

Update connection string di `appsettings.json`.

### 3. Run Application
```bash
dotnet run
```

Aplikasi akan berjalan di `http://localhost:5000`.

### 4. Setup RCCService
RCCService harus berjalan di port yang dikonfigurasi (default: 3000).

## API Endpoints

### Public Endpoints
- `GET /` - Home page
- `GET /Home/Games` - Games listing
- `GET /Account/Login` - Login page
- `GET /Account/Register` - Register page
- `GET /Profile/{username}` - User profile
- `GET /asset/?id=` - Asset delivery
- `GET /thumbs/avatar?userId=` - Avatar thumbnail
- `GET /thumbs/game?gameId=` - Game thumbnail

### Game Endpoints (Roblox Client)
- `GET /Game/Join.ashx?placeId=` - Join game
- `GET /Game/PlaceLauncher.ashx?placeId=` - Launch place
- `GET /Game/GetGames` - Get games list
- `POST /Game/PlayerJoined` - Player joined notification

### Internal API
- `POST /api/internal/server/create` - Create game server
- `POST /api/internal/server/stop` - Stop game server
- `GET /api/internal/server/status/{id}` - Get server status
- `POST /api/internal/game/join` - Join game
- `POST /api/internal/script/execute` - Execute Lua script

## Game Join Flow

1. Client menekan tombol Play
2. Client memanggil `/Game/Join.ashx?placeId=X`
3. Server memvalidasi session/user
4. Server membuat authentication ticket
5. Server mengirim job ke RCCService
6. RCCService membuat game server
7. Server mengembalikan: `OK|serverId|address|port|ticket`
8. Client connect ke server menggunakan info tersebut

## Lisensi

MIT License