import { Request, Response, NextFunction } from 'express';
import { userService } from '../services/index.js';
import { AuthenticationError } from '../utils/errors.js';
import type { JwtPayload } from '../types/index.js';

// Extend Express Request type
declare global {
  namespace Express {
    interface Request {
      user?: JwtPayload;
    }
  }
}

/**
 * Authentication middleware
 * Validates JWT token and attaches user to request
 */
export function authMiddleware(req: Request, _res: Response, next: NextFunction): void {
  try {
    const authHeader = req.headers['authorization'];

    if (!authHeader) {
      throw new AuthenticationError('No authorization header');
    }

    // Expect: "Bearer <token>"
    const parts = authHeader.split(' ');
    if (parts.length !== 2 || parts[0] !== 'Bearer') {
      throw new AuthenticationError('Invalid authorization header format');
    }

    const token = parts[1];
    if (!token) {
      throw new AuthenticationError('No token provided');
    }
    const payload = userService.verifyToken(token);
    req.user = payload;

    next();
  } catch (error) {
    next(error);
  }
}

/**
 * Optional authentication middleware
 * Attaches user if token is valid, but doesn't fail if not provided
 */
export function optionalAuthMiddleware(req: Request, _res: Response, next: NextFunction): void {
  try {
    const authHeader = req.headers['authorization'];

    if (!authHeader) {
      return next();
    }

    const parts = authHeader.split(' ');
    if (parts.length !== 2 || parts[0] !== 'Bearer') {
      return next();
    }

    const token = parts[1];
    if (token) {
      try {
        const payload = userService.verifyToken(token);
        req.user = payload;
      } catch {
        // Ignore invalid tokens for optional auth
      }
    }

    next();
  } catch {
    next();
  }
}
