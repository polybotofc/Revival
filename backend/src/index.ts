import express, { Application } from 'express';
import cors from 'cors';
import { config } from './config/index.js';
import { logger } from './utils/logger.js';
import { errorHandler, notFoundHandler } from './middleware/index.js';
import routes from './routes/index.js';

/**
 * Create and configure Express application
 */
function createApp(): Application {
  const app = express();

  // Middleware
  app.use(cors({
    origin: process.env['CORS_ORIGIN'] ?? '*',
    methods: ['GET', 'POST', 'PATCH', 'DELETE', 'OPTIONS'],
    allowedHeaders: ['Content-Type', 'Authorization'],
  }));
  
  app.use(express.json({ limit: '1mb' }));
  app.use(express.urlencoded({ extended: true }));

  // Request logging
  app.use((req, _res, next) => {
    logger.debug(`${req.method} ${req.path}`, {
      query: req.query,
      ip: req.ip,
    });
    next();
  });

  // API routes
  app.use('/api', routes);

  // Root endpoint
  app.get('/', (_req, res) => {
    res.json({
      name: 'Revival API',
      version: '1.0.0',
      description: 'Roblox Avatar Revival System',
      endpoints: {
        health: '/api/health',
        auth: '/api/auth',
        avatar: '/api/avatar',
      },
    });
  });

  // 404 handler
  app.use(notFoundHandler);

  // Error handler
  app.use(errorHandler);

  return app;
}

/**
 * Start the server
 */
async function main(): Promise<void> {
  try {
    const app = createApp();
    
    app.listen(config.port, () => {
      logger.info(`Server started`, {
        port: config.port,
        env: config.nodeEnv,
        rccUrl: config.rcc.url,
      });
      logger.info(`API available at http://localhost:${config.port}/api`);
    });
  } catch (error) {
    logger.error('Failed to start server', { error });
    process.exit(1);
  }
}

// Handle uncaught exceptions
process.on('uncaughtException', (error) => {
  logger.error('Uncaught exception', { error: error.message, stack: error.stack });
  process.exit(1);
});

// Handle unhandled promise rejections
process.on('unhandledRejection', (reason) => {
  logger.error('Unhandled rejection', { reason });
  process.exit(1);
});

// Graceful shutdown
process.on('SIGTERM', () => {
  logger.info('SIGTERM received, shutting down gracefully');
  process.exit(0);
});

process.on('SIGINT', () => {
  logger.info('SIGINT received, shutting down gracefully');
  process.exit(0);
});

// Start the server
main();
