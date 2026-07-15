# Revival - Roblox Avatar Revival System

A modern Roblox avatar revival system built with Node.js, Express, TypeScript, Next.js, and SQLite (Prisma).

## Features

### Completed Features (Phase 1)
- ✅ User Registration with default R6 avatar
- ✅ JWT Authentication
- ✅ Avatar API (Roblox-compatible format)
- ✅ Equip/Unequip assets API
- ✅ Body Colors API
- ✅ RCC Render Service with fallback
- ✅ Avatar Thumbnail API
- ✅ Login/Register/Profile pages

### What's NOT Included (Not in scope for Phase 1)
- ❌ Marketplace
- ❌ Economy
- ❌ Friends
- ❌ Messaging
- ❌ Groups
- ❌ Game Server
- ❌ Matchmaking
- ❌ Join Game
- ❌ Asset Upload
- ❌ Complex Inventory UI

## Project Structure

```
/Revival
├── /backend
│   ├── /controllers     # Request handlers
│   ├── /routes          # API route definitions
│   ├── /services        # Business logic
│   ├── /renderer        # RCC render service
│   ├── /database        # Prisma client
│   ├── /middleware      # Auth, validation, error handling
│   ├── /models          # Type definitions
│   ├── /utils           # Helper functions
│   ├── /prisma          # Database schema
│   └── /src             # Entry point and config
│
├── /frontend
│   ├── /app             # Next.js pages
│   ├── /components      # React components
│   └── /lib             # API client and utilities
│
└── README.md
```

## Tech Stack

### Backend
- **Runtime**: Node.js
- **Framework**: Express.js
- **Language**: TypeScript
- **Database**: SQLite with Prisma ORM
- **Auth**: JWT (jsonwebtoken)
- **Validation**: Zod

### Frontend
- **Framework**: Next.js 14 (App Router)
- **Language**: React + TypeScript
- **Styling**: TailwindCSS
- **HTTP Client**: Axios

### Renderer
- **Service**: Roblox RCCService (SOAP API)
- **Fallback**: SVG placeholder generator

## Getting Started

### Prerequisites
- Node.js 18+
- npm or yarn

### Backend Setup

1. Navigate to backend directory:
```bash
cd backend
```

2. Install dependencies:
```bash
npm install
```

3. Configure environment:
```bash
cp .env.example .env
# Edit .env with your settings
```

4. Initialize database:
```bash
npm run db:push
npm run db:generate
```

5. Start development server:
```bash
npm run dev
```

Backend will be available at `http://localhost:3001`

### Frontend Setup

1. Navigate to frontend directory:
```bash
cd frontend
```

2. Install dependencies:
```bash
npm install
```

3. Configure environment:
```bash
cp .env.example .env.local
# Edit .env.local if backend URL is different
```

4. Start development server:
```bash
npm run dev
```

Frontend will be available at `http://localhost:3000`

## API Endpoints

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | Login and get JWT |
| GET | `/api/auth/me` | Get current user |

### Avatar
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/avatar/:userId` | Get avatar data |
| POST | `/api/avatar/equip` | Equip asset |
| POST | `/api/avatar/unequip` | Unequip asset |
| GET | `/api/avatar/:userId/body-colors` | Get body colors |
| PATCH | `/api/avatar/body-colors` | Set body colors |
| PATCH | `/api/avatar/type` | Switch R6/R15 |

### Thumbnail
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/avatar/thumbnail` | Generate avatar PNG |
| GET | `/api/avatar/:userId/thumbnail` | Get avatar thumbnail |

### Health
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/health` | Server health check |

## API Examples

### Register User
```bash
curl -X POST http://localhost:3001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","email":"test@example.com","password":"password123"}'
```

### Login
```bash
curl -X POST http://localhost:3001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"password123"}'
```

### Get Avatar
```bash
curl http://localhost:3001/api/avatar/1
```

### Switch Avatar Type
```bash
curl -X PATCH http://localhost:3001/api/avatar/type \
  -H "Content-Type: application/json" \
  -d '{"userId":1,"avatarType":"R15"}'
```

## Database Schema

### Users
| Column | Type | Description |
|--------|------|-------------|
| id | Int (PK) | Auto-increment ID |
| username | String | Unique username |
| email | String | Unique email |
| passwordHash | String | Bcrypt hashed password |
| avatarType | String | "R6" or "R15" |
| createdAt | DateTime | Creation timestamp |
| updatedAt | DateTime | Last update timestamp |

### AvatarAssets
| Column | Type | Description |
|--------|------|-------------|
| id | Int (PK) | Auto-increment ID |
| userId | Int (FK) | Reference to User |
| assetId | Int | Roblox asset ID |
| assetTypeId | Int | Asset type category |

### AvatarBodyColors
| Column | Type | Description |
|--------|------|-------------|
| id | Int (PK) | Auto-increment ID |
| userId | Int (FK) | Reference to User |
| headColorId | Int | Head brick color ID |
| torsoColorId | Int | Torso brick color ID |
| leftArmColorId | Int | Left arm brick color ID |
| rightArmColorId | Int | Right arm brick color ID |
| leftLegColorId | Int | Left leg brick color ID |
| rightLegColorId | Int | Right leg brick color ID |

## Avatar API Response Format

The avatar API returns data compatible with Roblox's modern avatar API:

```json
{
  "playerAvatarType": "R6",
  "bodyColors": {
    "headColorId": 194,
    "torsoColorId": 194,
    "leftArmColorId": 194,
    "rightArmColorId": 194,
    "leftLegColorId": 194,
    "rightLegColorId": 194
  },
  "assets": []
}
```

## Development

### Running Tests
```bash
# Backend
cd backend
npm test

# Frontend
cd frontend
npm test
```

### Building for Production

```bash
# Backend
cd backend
npm run build
npm start

# Frontend
cd frontend
npm run build
npm start
```

## License

MIT License - See LICENSE file for details.

## Roadmap

### Phase 2 (Planned)
- [ ] Asset Catalog API
- [ ] Avatar Editor UI
- [ ] User Avatar Gallery

### Phase 3 (Planned)
- [ ] Game Server integration
- [ ] Join Game functionality
- [ ] Real-time avatar sync
