import { Request, Response } from 'express';
import { z } from 'zod';
import { userService } from '../services/index.js';
import { sendSuccess, asyncHandler } from '../utils/index.js';

/**
 * Validation schemas
 */
export const registerSchema = z.object({
  username: z.string().min(3).max(20),
  email: z.string().email(),
  password: z.string().min(6),
});

export const loginSchema = z.object({
  email: z.string().email(),
  password: z.string().min(1),
});

/**
 * AuthController handles user authentication endpoints
 */
export class AuthController {
  /**
   * POST /auth/register
   * Register a new user
   */
  register = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const { username, email, password } = req.body;

    const user = await userService.createUser({ username, email, password });

    sendSuccess(res, {
      user,
      message: 'User registered successfully',
    }, 201);
  });

  /**
   * POST /auth/login
   * Login user and return JWT token
   */
  login = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const { email, password } = req.body;

    const result = await userService.login(email, password);

    sendSuccess(res, {
      user: result.user,
      token: result.token,
      message: 'Login successful',
    });
  });

  /**
   * GET /auth/me
   * Get current user profile
   * Requires authentication
   */
  getCurrentUser = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const userId = req.user?.userId;
    if (!userId) {
      res.status(401).json({ success: false, error: 'Unauthorized' });
      return;
    }

    const user = await userService.getUserById(userId);

    sendSuccess(res, { user });
  });
}

// Singleton instance
export const authController = new AuthController();
