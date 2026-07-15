import { prisma } from '../database/index.js';
import { NotFoundError, ValidationError } from '../utils/errors.js';
import { logger } from '../utils/logger.js';
import { userService } from './UserService.js';
import type {
  AvatarResponse,
  AvatarBodyColors,
  AvatarAssetResponse,
  AvatarAssetInput,
} from '../types/index.js';

/**
 * AvatarService handles all avatar-related business logic
 * Provides methods for managing avatar assets and body colors
 */
export class AvatarService {
  /**
   * Get complete avatar data for a user
   * Compatible with Roblox's modern avatar API format
   */
  async getAvatar(userId: number): Promise<AvatarResponse> {
    // Get user with avatar type
    const user = await prisma.user.findUnique({
      where: { id: userId },
      include: {
        avatarAssets: true,
        bodyColors: true,
      },
    });

    if (!user) {
      throw new NotFoundError('User not found');
    }

    // Get body colors (with defaults if not set)
    const bodyColors = user.bodyColors
      ? {
          headColorId: user.bodyColors.headColorId,
          torsoColorId: user.bodyColors.torsoColorId,
          leftArmColorId: user.bodyColors.leftArmColorId,
          rightArmColorId: user.bodyColors.rightArmColorId,
          leftLegColorId: user.bodyColors.leftLegColorId,
          rightLegColorId: user.bodyColors.rightLegColorId,
        }
      : await userService.getBodyColors(userId);

    // Format assets in Roblox-compatible format
    const assets = user.avatarAssets.map((asset) => this.formatAssetResponse(asset));

    return {
      playerAvatarType: user.avatarType as 'R6' | 'R15',
      bodyColors,
      assets,
    };
  }

  /**
   * Equip an asset to the user's avatar
   * If asset type only allows one instance (e.g., Head, Face), replace existing
   * If asset type allows multiple (e.g., Hat), add to existing
   */
  async equipAsset(input: AvatarAssetInput): Promise<AvatarResponse> {
    const { userId, assetId, assetTypeId } = input;

    // Validate input
    if (!userId || !assetId || !assetTypeId) {
      throw new ValidationError('userId, assetId, and assetTypeId are required');
    }

    // Check if user exists
    const user = await prisma.user.findUnique({
      where: { id: userId },
    });

    if (!user) {
      throw new NotFoundError('User not found');
    }

    // Asset types that should be replaced (only one instance allowed)
    const SINGLE_INSTANCE_TYPES = new Set([2, 3, 4, 5]); // Head, Face, Neck, Shoulder

    if (SINGLE_INSTANCE_TYPES.has(assetTypeId)) {
      // Remove existing asset of this type
      await prisma.avatarAsset.deleteMany({
        where: {
          userId,
          assetTypeId,
        },
      });
      logger.debug('Removed existing asset of type', { assetTypeId });
    }

    // Check if asset already equipped
    const existingAsset = await prisma.avatarAsset.findFirst({
      where: {
        userId,
        assetId,
        assetTypeId,
      },
    });

    if (existingAsset) {
      logger.debug('Asset already equipped', { assetId });
      // Asset already equipped, just return current avatar
      return this.getAvatar(userId);
    }

    // Equip the new asset
    await prisma.avatarAsset.create({
      data: {
        userId,
        assetId,
        assetTypeId,
      },
    });

    logger.info('Asset equipped', { userId, assetId, assetTypeId });

    // Return updated avatar
    return this.getAvatar(userId);
  }

  /**
   * Unequip an asset from the user's avatar
   */
  async unequipAsset(userId: number, assetId: number): Promise<AvatarResponse> {
    // Check if user exists
    const user = await prisma.user.findUnique({
      where: { id: userId },
    });

    if (!user) {
      throw new NotFoundError('User not found');
    }

    // Remove the asset
    const result = await prisma.avatarAsset.deleteMany({
      where: {
        userId,
        assetId,
      },
    });

    if (result.count === 0) {
      throw new NotFoundError('Asset not found on avatar');
    }

    logger.info('Asset unequipped', { userId, assetId });

    // Return updated avatar
    return this.getAvatar(userId);
  }

  /**
   * Get body colors for a user
   */
  async getBodyColors(userId: number): Promise<AvatarBodyColors> {
    const avatar = await this.getAvatar(userId);
    return avatar.bodyColors;
  }

  /**
   * Set body colors for a user
   */
  async setBodyColors(userId: number, colors: AvatarBodyColors): Promise<AvatarResponse> {
    // Delegate to userService
    await userService.setBodyColors(userId, colors);
    return this.getAvatar(userId);
  }

  /**
   * Format asset for Roblox-compatible response
   * In a real system, asset names would come from an asset catalog database
   */
  private formatAssetResponse(asset: { assetId: number; assetTypeId: number }): AvatarAssetResponse {
    const typeName = this.getAssetTypeName(asset.assetTypeId);
    return {
      id: asset.assetId,
      assetType: {
        id: asset.assetTypeId,
        name: typeName,
      },
      name: `Asset ${asset.assetId}`,
    };
  }

  /**
   * Get asset type name from ID
   * Maps to Roblox's asset type system
   */
  private getAssetTypeName(assetTypeId: number): string {
    const assetTypes: Record<number, string> = {
      1: 'Hair',
      2: 'Head',
      3: 'Face',
      4: 'Neck',
      5: 'Shoulder',
      6: 'Front',
      7: 'Back',
      8: 'Tool',
      9: 'FrontHair',
      10: 'BackHair',
      11: 'SideHair',
      12: 'Accessory',
      13: 'TShirt',
      14: 'Shirt',
      15: 'Pants',
      16: 'Jacket',
      17: 'Sweater',
      18: 'Shorts',
      19: 'LeftShoe',
      20: 'RightShoe',
      21: ' Dress',
      22: 'Badge',
      23: 'DynamicHead',
    };

    return assetTypes[assetTypeId] ?? 'Unknown';
  }
}

// Singleton instance
export const avatarService = new AvatarService();
