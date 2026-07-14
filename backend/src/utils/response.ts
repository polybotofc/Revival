import { Request, Response } from 'express';
import { ZodError } from 'zod';
import { AppError } from './errors.js';
import { logger } from './logger.js';
import type { ApiResponse } from '../types/index.js';

/**
 * Send a success response
 */
export function sendSuccess<T>(
  res: Response,
  data: T,
  statusCode = 200,
  message?: string
): void {
  const response: ApiResponse<T> = {
    success: true,
    data,
    ...(message && { message }),
  };
  res.status(statusCode).json(response);
}

/**
 * Send an error response
 */
export function sendError(
  res: Response,
  statusCode: number,
  error: string
): void {
  const response: ApiResponse = {
    success: false,
    error,
  };
  res.status(statusCode).json(response);
}

/**
 * Handle different error types and send appropriate response
 */
export function handleError(res: Response, error: unknown): void {
  // Log the error
  logger.error('Request error', { error: error instanceof Error ? error.message : String(error) });

  // Zod validation error
  if (error instanceof ZodError) {
    const messages = error.errors.map((e) => `${e.path.join('.')}: ${e.message}`);
    sendError(res, 400, messages.join('; '));
    return;
  }

  // Custom application error
  if (error instanceof AppError) {
    sendError(res, error.statusCode, error.message);
    return;
  }

  // Generic error
  const message = error instanceof Error ? error.message : 'Unknown error occurred';
  sendError(res, 500, message);
}

/**
 * Handle async route handlers
 */
export function asyncHandler(
  fn: (req: Request, res: Response) => Promise<void>
) {
  return (req: Request, res: Response, _next: unknown): void => {
    Promise.resolve(fn(req, res)).catch((error) => handleError(res, error));
  };
}
