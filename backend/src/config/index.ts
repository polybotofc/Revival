import dotenv from 'dotenv';
import path from 'path';

// Load environment variables
dotenv.config();

export const config = {
  // Server
  port: parseInt(process.env['PORT'] ?? '3001', 10),
  nodeEnv: process.env['NODE_ENV'] ?? 'development',

  // JWT
  jwt: {
    secret: process.env['JWT_SECRET'] ?? 'fallback-secret-change-in-production',
    expiresIn: process.env['JWT_EXPIRES_IN'] ?? '7d',
  },

  // Database
  database: {
    url: process.env['DATABASE_URL'] ?? 'file:./prisma/dev.db',
  },

  // RCC (Roblox Cloud Console)
  rcc: {
    host: process.env['RCC_HOST'] ?? '127.0.0.1',
    port: parseInt(process.env['RCC_PORT'] ?? '64989', 10),
    url: process.env['RCC_URL'] ?? 'http://127.0.0.1:64989',
    executablePath: process.env['RCC_EXECUTABLE_PATH'] ?? 'C:\\New folder\\RCCService.exe',
    contentPath: process.env['RCC_CONTENT_PATH'] ?? 'C:\\New folder\\Content',
    scriptsPath: process.env['RCC_SCRIPTS_PATH'] ?? 'C:\\New folder\\internalscripts',
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
    outputDir: process.env['THUMBNAIL_OUTPUT_DIR'] ?? path.join(process.cwd(), 'thumbnails'),
  },
} as const;

export type Config = typeof config;
