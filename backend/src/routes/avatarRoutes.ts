import { Router } from 'express';
import { 
  avatarController, 
  thumbnailController,
  equipSchema, 
  unequipSchema, 
  bodyColorsSchema, 
  switchAvatarTypeSchema,
  thumbnailSchema 
} from '../controllers/index.js';
import { validateBody } from '../middleware/index.js';

const router = Router();

/**
 * GET /avatar/:userId
 * Get avatar data for a user
 * Returns Roblox-compatible avatar JSON format:
 * {
 *   "playerAvatarType": "R6" | "R15",
 *   "bodyColors": {
 *     "headColorId": number,
 *     "torsoColorId": number,
 *     ...
 *   },
 *   "assets": [...]
 * }
 */
router.get('/:userId', avatarController.getAvatar);

/**
 * GET /avatar/:userId/body-colors
 * Get body colors for a user
 */
router.get('/:userId/body-colors', avatarController.getBodyColors);

/**
 * POST /avatar/equip
 * Equip an asset to user's avatar
 * Body: { userId, assetId, assetTypeId }
 * If asset type only allows one (Head, Face, Neck, Shoulder), replaces existing
 */
router.post('/equip', validateBody(equipSchema), avatarController.equipAsset);

/**
 * POST /avatar/unequip
 * Unequip an asset from user's avatar
 * Body: { userId, assetId }
 */
router.post('/unequip', validateBody(unequipSchema), avatarController.unequipAsset);

/**
 * PATCH /avatar/body-colors
 * Set body colors for a user
 * Body: { userId, bodyColors: { headColorId, torsoColorId, ... } }
 */
router.patch('/body-colors', validateBody(bodyColorsSchema), avatarController.setBodyColors);

/**
 * PATCH /avatar/type
 * Switch avatar type between R6 and R15
 * Body: { userId, avatarType: "R6" | "R15" }
 */
router.patch('/type', validateBody(switchAvatarTypeSchema), avatarController.switchAvatarType);

/**
 * POST /avatar/thumbnail
 * Generate avatar thumbnail image
 * Body: { userId }
 * Returns: image/png
 */
router.post('/thumbnail', validateBody(thumbnailSchema), thumbnailController.generateThumbnail);

/**
 * GET /avatar/:userId/thumbnail
 * Alternative thumbnail endpoint with query parameter
 * Query: ?userId=1
 * Returns: image/png (or image/svg+xml as fallback)
 */
router.get('/:userId/thumbnail', thumbnailController.getThumbnail);

export default router;
