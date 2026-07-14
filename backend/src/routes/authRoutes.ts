import { Router } from 'express';
import { authController, registerSchema, loginSchema } from '../controllers/index.js';
import { validateBody, authMiddleware } from '../middleware/index.js';

const router = Router();

/**
 * POST /auth/register
 * Register a new user account
 * Body: { username, email, password }
 * Creates user with default R6 avatar and body colors
 */
router.post('/register', validateBody(registerSchema), authController.register);

/**
 * POST /auth/login
 * Login user and receive JWT token
 * Body: { email, password }
 */
router.post('/login', validateBody(loginSchema), authController.login);

/**
 * GET /auth/me
 * Get current authenticated user profile
 * Requires: Bearer token in Authorization header
 */
router.get('/me', authMiddleware, authController.getCurrentUser);

export default router;
