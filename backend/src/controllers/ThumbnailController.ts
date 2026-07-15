import { Request, Response } from 'express';
import { z } from 'zod';
import { renderService } from '../services/index.js';
import { asyncHandler } from '../utils/index.js';
import { ValidationError } from '../utils/errors.js';

/**
 * Validation schemas
 */
export const thumbnailSchema = z.object({
  userId: z.number().int().positive(),
});

/**
 * ThumbnailController handles avatar thumbnail generation
 */
export class ThumbnailController {
  /**
   * POST /avatar/thumbnail
   * Generate and return avatar thumbnail image
   * Returns image/png content type
   */
  generateThumbnail = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const userId = req.body['userId'];

    if (!userId || typeof userId !== 'number' || userId <= 0) {
      throw new ValidationError('Invalid userId');
    }

    // Try to generate thumbnail using RCC
    let result = await renderService.GenerateThumbnail(userId);

    // If RCC fails, generate placeholder
    if (!result.success || !result.imageData) {
      result = await renderService.generatePlaceholderThumbnail(userId);
    }

    if (!result.success || !result.imageData) {
      res.status(500).json({
        success: false,
        error: result.error ?? 'Failed to generate thumbnail',
      });
      return;
    }

    // Check if it's SVG or PNG
    if (result.imageData.startsWith('<svg')) {
      // Return SVG as image
      res.setHeader('Content-Type', 'image/svg+xml');
      res.send(Buffer.from(result.imageData));
    } else {
      // Return Base64 decoded PNG
      res.setHeader('Content-Type', 'image/png');
      try {
        const imageBuffer = Buffer.from(result.imageData, 'base64');
        res.send(imageBuffer);
      } catch {
        // If base64 decode fails, send as SVG
        res.setHeader('Content-Type', 'image/svg+xml');
        const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="420" height="420">
          <rect width="420" height="420" fill="#e8e8e8"/>
          <circle cx="210" cy="70" r="45" fill="#f2e7c6"/>
          <rect x="170" y="115" width="80" height="60" fill="#f2e7c6"/>
          <rect x="90" y="120" width="80" height="50" fill="#f2e7c6"/>
          <rect x="250" y="120" width="80" height="50" fill="#f2e7c6"/>
          <rect x="175" y="175" width="35" height="110" fill="#f2e7c6"/>
          <rect x="210" y="175" width="35" height="110" fill="#f2e7c6"/>
        </svg>`;
        res.send(svg);
      }
    }
  });

  /**
   * GET /avatar/:userId/thumbnail
   * Alternative endpoint using GET with query parameter
   */
  getThumbnail = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const userId = parseInt(req.query['userId'] as string ?? '0', 10);

    if (!userId || userId <= 0) {
      throw new ValidationError('Invalid userId');
    }

    // Try to generate thumbnail using RCC
    let result = await renderService.GenerateThumbnail(userId);

    // If RCC fails, generate placeholder
    if (!result.success || !result.imageData) {
      result = await renderService.generatePlaceholderThumbnail(userId);
    }

    if (!result.success || !result.imageData) {
      res.status(500).json({
        success: false,
        error: result.error ?? 'Failed to generate thumbnail',
      });
      return;
    }

    // Return SVG as image
    if (result.imageData.startsWith('<svg')) {
      res.setHeader('Content-Type', 'image/svg+xml');
      res.send(Buffer.from(result.imageData));
    } else {
      res.setHeader('Content-Type', 'image/png');
      try {
        const imageBuffer = Buffer.from(result.imageData, 'base64');
        res.send(imageBuffer);
      } catch {
        res.status(500).json({
          success: false,
          error: 'Failed to decode thumbnail',
        });
      }
    }
  });
}

// Singleton instance
export const thumbnailController = new ThumbnailController();
