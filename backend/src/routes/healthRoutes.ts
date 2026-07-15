import { Router, Request, Response } from 'express';
import { prisma } from '../database/index.js';

const router = Router();

/**
 * GET /health
 * Health check endpoint
 * Returns server status and database connectivity
 */
router.get('/', async (_req: Request, res: Response): Promise<void> => {
  try {
    // Check database connection
    await prisma.$queryRaw`SELECT 1`;
    
    res.json({
      status: 'ok',
      timestamp: new Date().toISOString(),
      database: 'connected',
      version: '1.0.0',
    });
  } catch (error) {
    res.status(503).json({
      status: 'error',
      timestamp: new Date().toISOString(),
      database: 'disconnected',
      error: error instanceof Error ? error.message : 'Unknown error',
    });
  }
});

export default router;
