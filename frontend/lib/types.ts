// User types
export interface User {
  id: number;
  username: string;
  avatarType: 'R6' | 'R15';
  createdAt: string;
}

// Avatar types
export interface AvatarBodyColors {
  headColorId: number;
  torsoColorId: number;
  leftArmColorId: number;
  rightArmColorId: number;
  leftLegColorId: number;
  rightLegColorId: number;
}

export interface AvatarAssetResponse {
  id: number;
  assetType: {
    id: number;
    name: string;
  };
  name: string;
}

export interface Avatar {
  playerAvatarType: 'R6' | 'R15';
  bodyColors: AvatarBodyColors;
  assets: AvatarAssetResponse[];
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
  avatarType: 'R6' | 'R15';
}

// API Response types
export interface ApiResponse<T = unknown> {
  success: boolean;
  data?: T;
  error?: string;
  message?: string;
}

export interface AuthResponse {
  user: User;
  token: string;
  message?: string;
}

export interface AvatarResponse {
  playerAvatarType: 'R6' | 'R15';
  bodyColors: AvatarBodyColors;
  assets: AvatarAssetResponse[];
}
