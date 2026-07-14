import axios, { AxiosInstance, AxiosError } from 'axios';
import type {
  ApiResponse,
  AuthResponse,
  Avatar,
  User,
  AvatarBodyColors,
  RegisterRequest,
  LoginRequest,
  EquipRequest,
  UnequipRequest,
  ThumbnailRequest,
  BodyColorsRequest,
  SwitchAvatarTypeRequest,
} from './types';

const API_BASE_URL = process.env['NEXT_PUBLIC_API_URL'] ?? 'http://localhost:3001/api';

/**
 * Create axios instance with default config
 */
function createApiClient(): AxiosInstance {
  const client = axios.create({
    baseURL: API_BASE_URL,
    headers: {
      'Content-Type': 'application/json',
    },
    timeout: 10000,
  });

  // Request interceptor to add auth token
  client.interceptors.request.use(
    (config) => {
      const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null;
      if (token) {
        config.headers['Authorization'] = `Bearer ${token}`;
      }
      return config;
    },
    (error) => Promise.reject(error)
  );

  // Response interceptor to handle errors
  client.interceptors.response.use(
    (response) => response,
    (error: AxiosError<ApiResponse>) => {
      if (error.response?.data?.error) {
        throw new Error(error.response.data.error);
      }
      if (error.response?.status === 401) {
        // Clear token and redirect to login
        if (typeof window !== 'undefined') {
          localStorage.removeItem('token');
          localStorage.removeItem('user');
        }
      }
      throw error;
    }
  );

  return client;
}

const api = createApiClient();

/**
 * Authentication API
 */
export const authApi = {
  /**
   * Register a new user
   */
  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const response = await api.post<ApiResponse<{ user: User }>>('/auth/register', data);
    if (!response.data.success) {
      throw new Error(response.data.error ?? 'Registration failed');
    }
    return {
      user: response.data.data!.user,
      token: '', // No token on registration
      message: response.data.message,
    };
  },

  /**
   * Login user
   */
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await api.post<ApiResponse<AuthResponse>>('/auth/login', data);
    if (!response.data.success) {
      throw new Error(response.data.error ?? 'Login failed');
    }
    return response.data.data!;
  },

  /**
   * Get current user
   */
  getCurrentUser: async (): Promise<User> => {
    const response = await api.get<ApiResponse<{ user: User }>>('/auth/me');
    if (!response.data.success) {
      throw new Error(response.data.error ?? 'Failed to get user');
    }
    return response.data.data!.user;
  },
};

/**
 * Avatar API
 */
export const avatarApi = {
  /**
   * Get avatar data for a user
   */
  getAvatar: async (userId: number): Promise<Avatar> => {
    const response = await api.get<ApiResponse<Avatar>>(`/avatar/${userId}`);
    if (!response.data.success) {
      throw new Error(response.data.error ?? 'Failed to get avatar');
    }
    return response.data.data!;
  },

  /**
   * Equip an asset
   */
  equipAsset: async (data: EquipRequest): Promise<Avatar> => {
    const response = await api.post<ApiResponse<{ avatar: Avatar }>>('/avatar/equip', data);
    if (!response.data.success) {
      throw new Error(response.data.error ?? 'Failed to equip asset');
    }
    return response.data.data!.avatar;
  },

  /**
   * Unequip an asset
   */
  unequipAsset: async (data: UnequipRequest): Promise<Avatar> => {
    const response = await api.post<ApiResponse<{ avatar: Avatar }>>('/avatar/unequip', data);
    if (!response.data.success) {
      throw new Error(response.data.error ?? 'Failed to unequip asset');
    }
    return response.data.data!.avatar;
  },

  /**
   * Get body colors
   */
  getBodyColors: async (userId: number): Promise<AvatarBodyColors> => {
    const response = await api.get<ApiResponse<{ bodyColors: AvatarBodyColors }>>(`/avatar/${userId}/body-colors`);
    if (!response.data.success) {
      throw new Error(response.data.error ?? 'Failed to get body colors');
    }
    return response.data.data!.bodyColors;
  },

  /**
   * Set body colors
   */
  setBodyColors: async (data: BodyColorsRequest): Promise<Avatar> => {
    const response = await api.patch<ApiResponse<{ avatar: Avatar }>>('/avatar/body-colors', data);
    if (!response.data.success) {
      throw new Error(response.data.error ?? 'Failed to set body colors');
    }
    return response.data.data!.avatar;
  },

  /**
   * Switch avatar type
   */
  switchAvatarType: async (data: SwitchAvatarTypeRequest): Promise<User> => {
    const response = await api.patch<ApiResponse<{ user: User }>>('/avatar/type', data);
    if (!response.data.success) {
      throw new Error(response.data.error ?? 'Failed to switch avatar type');
    }
    return response.data.data!.user;
  },

  /**
   * Get thumbnail URL
   */
  getThumbnailUrl: (userId: number): string => {
    return `${API_BASE_URL}/avatar/thumbnail?userId=${userId}&t=${Date.now()}`;
  },

  /**
   * Generate thumbnail via POST
   */
  generateThumbnail: async (data: ThumbnailRequest): Promise<Blob> => {
    const response = await api.post(`${API_BASE_URL}/avatar/thumbnail`, data, {
      responseType: 'blob',
    });
    return response.data;
  },
};

/**
 * Auth helpers
 */
export const auth = {
  /**
   * Save auth data to localStorage
   */
  saveAuth: (data: AuthResponse): void => {
    if (typeof window !== 'undefined') {
      localStorage.setItem('token', data.token);
      localStorage.setItem('user', JSON.stringify(data.user));
    }
  },

  /**
   * Get token from localStorage
   */
  getToken: (): string | null => {
    if (typeof window !== 'undefined') {
      return localStorage.getItem('token');
    }
    return null;
  },

  /**
   * Get user from localStorage
   */
  getUser: (): User | null => {
    if (typeof window !== 'undefined') {
      const userStr = localStorage.getItem('user');
      if (userStr) {
        try {
          return JSON.parse(userStr);
        } catch {
          return null;
        }
      }
    }
    return null;
  },

  /**
   * Clear auth data
   */
  clearAuth: (): void => {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
    }
  },

  /**
   * Check if user is authenticated
   */
  isAuthenticated: (): boolean => {
    return auth.getToken() !== null;
  },
};

export { api };
