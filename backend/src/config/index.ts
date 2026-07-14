import dotenv from 'dotenv';

// Load environment variables
dotenv.config();

// Validate required environment variables
const requiredEnvVars = ['JWT_SECRET', 'DATABASE_URL'];
for (const envVar of requiredEnvVars) {
  if (!process.env[envVar]) {
    throw new Error(`Missing required environment variable: ${envVar}`);
  }
}

export const config = {
  // Server
  port: parseInt(process.env['PORT'] ?? '3001', 10),
  nodeEnv: process.env['NODE_ENV'] ?? 'development',

  // JWT
  jwt: {
    secret: process.env['JWT_SECRET'] as string,
    expiresIn: process.env['JWT_EXPIRES_IN'] ?? '7d',
  },

  // Database
  database: {
    url: process.env['DATABASE_URL'] as string,
  },

  // RCC (Roblox Cloud Console)
  rcc: {
    host: process.env['RCC_HOST'] ?? 'localhost',
    port: parseInt(process.env['RCC_PORT'] ?? '64989', 10),
    url: process.env['RCC_URL'] ?? 'http://localhost:64989',
  },

  // Default Body Colors (Roblox Classic - Pearl)
  defaultBodyColors: {
    headColorId: parseInt(process.env['DEFAULT_BODY_COLOR_HEAD'] ?? '194', 10),
    torsoColorId: parseInt(process.env['DEFAULT_BODY_COLOR_TORSO'] ?? '194', 10),
    leftArmColorId: parseInt(process.env['DEFAULT_BODY_COLOR_LEFT_ARM'] ?? '194', 10),
    rightArmColorId: parseInt(process.env['DEFAULT_BODY_COLOR_RIGHT_ARM'] ?? '194', 10),
    leftLegColorId: parseInt(process.env['DEFAULT_BODY_COLOR_LEFT_LEG'] ?? '194', 10),
    rightLegColorId: parseInt(process.env['DEFAULT_BODY_COLOR_RIGHT_LEG'] ?? '194', 10),
  },

  // Thumbnail
  thumbnail: {
    size: parseInt(process.env['THUMBNAIL_SIZE'] ?? '420', 10),
    cameraType: process.env['THUMBNAIL_CAMERA_TYPE'] ?? 'Portrait',
  },
} as const;

export type Config = typeof config;
