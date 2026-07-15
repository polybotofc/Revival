// User types
export interface User {
  id: number;
  username: string;
  email: string;
  passwordHash: string;
  avatarType: AvatarType;
  createdAt: Date;
  updatedAt: Date;
}

export interface UserCreateInput {
  username: string;
  email: string;
  password: string;
}

export interface UserPublic {
  id: number;
  username: string;
  avatarType: AvatarType;
  createdAt: Date;
}

// Avatar types
export type AvatarType = 'R6' | 'R15';

export interface AvatarBodyColors {
  headColorId: number;
  torsoColorId: number;
  leftArmColorId: number;
  rightArmColorId: number;
  leftLegColorId: number;
  rightLegColorId: number;
}

export interface AvatarAsset {
  id: number;
  userId: number;
  assetId: number;
  assetTypeId: number;
  createdAt: Date;
}

export interface AvatarAssetInput {
  userId: number;
  assetId: number;
  assetTypeId: number;
}

// Roblox-compatible avatar response
export interface AvatarResponse {
  playerAvatarType: AvatarType;
  bodyColors: AvatarBodyColors;
  assets: AvatarAssetResponse[];
}

export interface AvatarAssetResponse {
  id: number;
  assetType: {
    id: number;
    name: string;
  };
  name: string;
}

// API Request/Response types
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface EquipRequest {
  userId: number;
  assetId: number;
  assetTypeId: number;
}

export interface UnequipRequest {
  userId: number;
  assetId: number;
}

export interface ThumbnailRequest {
  userId: number;
}

export interface BodyColorsRequest {
  userId: number;
  bodyColors: AvatarBodyColors;
}

export interface SwitchAvatarTypeRequest {
  userId: number;
  avatarType: AvatarType;
}

// JWT types
export interface JwtPayload {
  userId: number;
  email: string;
  iat?: number;
  exp?: number;
}

export interface AuthenticatedRequest {
  user?: JwtPayload;
}

// API Response types
export interface ApiResponse<T = unknown> {
  success: boolean;
  data?: T;
  error?: string;
  message?: string;
}

// Render types
export interface RenderResult {
  imageData: string; // Base64 encoded PNG/SVG
  success: boolean;
  error?: string;
  isPlaceholder?: boolean;
}

// Asset type mapping
export const ASSET_TYPE_NAMES: Record<number, string> = {
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

// Asset types that should be replaced (only one allowed)
export const SINGLE_INSTANCE_ASSET_TYPES = new Set([2, 3, 4, 5]); // Head, Face, Neck, Shoulder
