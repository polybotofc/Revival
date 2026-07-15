import { Request, Response } from 'express';
import { z } from 'zod';
import { avatarService, userService } from '../services/index.js';
import { asyncHandler, sendSuccess } from '../utils/index.js';

/**
 * Validation schemas
 */
export const equipSchema = z.object({
  userId: z.number().int().positive(),
  assetId: z.number().int().positive(),
  assetTypeId: z.number().int().positive(),
});

export const unequipSchema = z.object({
  userId: z.number().int().positive(),
  assetId: z.number().int().positive(),
});

export const bodyColorsSchema = z.object({
  userId: z.number().int().positive(),
  bodyColors: z.object({
    headColorId: z.number().int().min(1).max(208),
    torsoColorId: z.number().int().min(1).max(208),
    leftArmColorId: z.number().int().min(1).max(208),
    rightArmColorId: z.number().int().min(1).max(208),
    leftLegColorId: z.number().int().min(1).max(208),
    rightLegColorId: z.number().int().min(1).max(208),
  }),
});

export const switchAvatarTypeSchema = z.object({
  userId: z.number().int().positive(),
  avatarType: z.enum(['R6', 'R15']),
});

/**
 * AvatarController handles avatar-related endpoints
 * Provides Roblox-compatible avatar API
 */
export class AvatarController {
  /**
   * GET /avatar/:userId
   * Get avatar data for a user
   * Returns Roblox-compatible avatar format
   */
  getAvatar = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const userId = parseInt(req.params['userId'] ?? '0', 10);

    if (!userId || userId <= 0) {
      res.status(400).json({ success: false, error: 'Invalid userId' });
      return;
    }

    const avatar = await avatarService.getAvatar(userId);

    sendSuccess(res, avatar);
  });

  /**
   * POST /avatar/equip
   * Equip an asset to user's avatar
   */
  equipAsset = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const { userId, assetId, assetTypeId } = req.body;

    const avatar = await avatarService.equipAsset({ userId, assetId, assetTypeId });

    sendSuccess(res, {
      avatar,
      message: 'Asset equipped successfully',
    });
  });

  /**
   * POST /avatar/unequip
   * Unequip an asset from user's avatar
   */
  unequipAsset = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const { userId, assetId } = req.body;

    const avatar = await avatarService.unequipAsset(userId, assetId);

    sendSuccess(res, {
      avatar,
      message: 'Asset unequipped successfully',
    });
  });

  /**
   * GET /avatar/:userId/body-colors
   * Get body colors for a user
   */
  getBodyColors = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const userId = parseInt(req.params['userId'] ?? '0', 10);

    if (!userId || userId <= 0) {
      res.status(400).json({ success: false, error: 'Invalid userId' });
      return;
    }

    const bodyColors = await avatarService.getBodyColors(userId);

    sendSuccess(res, { bodyColors });
  });

  /**
   * PATCH /avatar/body-colors
   * Set body colors for a user
   */
  setBodyColors = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const { userId, bodyColors } = req.body;

    const avatar = await avatarService.setBodyColors(userId, bodyColors);

    sendSuccess(res, {
      avatar,
      message: 'Body colors updated successfully',
    });
  });

  /**
   * PATCH /avatar/type
   * Switch avatar type between R6 and R15
   */
  switchAvatarType = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const { userId, avatarType } = req.body;

    const user = await userService.updateAvatarType(userId, avatarType);

    sendSuccess(res, {
      user,
      message: `Avatar type switched to ${avatarType}`,
    });
  });
}

// Singleton instance
export const avatarController = new AvatarController();
