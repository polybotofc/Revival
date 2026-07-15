import { Router } from 'express';
import authRoutes from './authRoutes.js';
import avatarRoutes from './avatarRoutes.js';
import healthRoutes from './healthRoutes.js';

const router = Router();

// Mount routes
router.use('/auth', authRoutes);
router.use('/avatar', avatarRoutes);
router.use('/health', healthRoutes);

export default router;
