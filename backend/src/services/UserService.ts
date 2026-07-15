import bcrypt from 'bcryptjs';
import jwt, { SignOptions } from 'jsonwebtoken';
import { prisma } from '../database/index.js';
import { config } from '../config/index.js';
import { ValidationError, AuthenticationError, NotFoundError, ConflictError } from '../utils/errors.js';
import { logger } from '../utils/logger.js';
import type { UserPublic, JwtPayload, AvatarBodyColors, AvatarType } from '../types/index.js';

interface UserCreateInput {
  username: string;
  email: string;
  password: string;
}

interface UserRecord {
  id: number;
  username: string;
  email: string;
  passwordHash: string;
  avatarType: string;
  createdAt: Date;
  updatedAt: Date;
}

/**
 * UserService handles all user-related business logic
 */
export class UserService {
  /**
   * Create a new user with default avatar configuration
   * Automatically creates R6 avatar with default body colors
   */
  async createUser(input: UserCreateInput): Promise<UserPublic> {
    const { username, email, password } = input;

    // Validate input
    if (!username || username.length < 3) {
      throw new ValidationError('Username must be at least 3 characters');
    }
    if (!email || !email.includes('@')) {
      throw new ValidationError('Invalid email address');
    }
    if (!password || password.length < 6) {
      throw new ValidationError('Password must be at least 6 characters');
    }

    // Check for existing user
    const existingUser = await prisma.user.findFirst({
      where: {
        OR: [{ email }, { username }],
      },
    });

    if (existingUser) {
      if (existingUser.email === email) {
        throw new ConflictError('Email already registered');
      }
      throw new ConflictError('Username already taken');
    }

    // Hash password
    const passwordHash = await bcrypt.hash(password, 12);

    // Create user with default avatar configuration
    const user = await prisma.user.create({
      data: {
        username,
        email,
        passwordHash,
        avatarType: 'R6',
        // Create default R6 avatar body colors
        bodyColors: {
          create: {
            headColorId: config.defaultBodyColors.headColorId,
            torsoColorId: config.defaultBodyColors.torsoColorId,
            leftArmColorId: config.defaultBodyColors.leftArmColorId,
            rightArmColorId: config.defaultBodyColors.rightArmColorId,
            leftLegColorId: config.defaultBodyColors.leftLegColorId,
            rightLegColorId: config.defaultBodyColors.rightLegColorId,
          },
        },
      },
      include: {
        bodyColors: true,
      },
    });

    logger.info('User created', { userId: user.id, username: user.username });

    return this.toPublicUser(user);
  }

  /**
   * Authenticate user and return JWT token
   */
  async login(email: string, password: string): Promise<{ user: UserPublic; token: string }> {
    // Validate input
    if (!email || !password) {
      throw new ValidationError('Email and password are required');
    }

    // Find user
    const user = await prisma.user.findUnique({
      where: { email },
    });

    if (!user) {
      throw new AuthenticationError('Invalid email or password');
    }

    // Verify password
    const isValidPassword = await bcrypt.compare(password, user.passwordHash);
    if (!isValidPassword) {
      throw new AuthenticationError('Invalid email or password');
    }

    // Generate JWT
    const payload: JwtPayload = {
      userId: user.id,
      email: user.email,
    };

    const signOptions: SignOptions = {
      expiresIn: config.jwt.expiresIn as jwt.SignOptions['expiresIn'],
    };

    const token = jwt.sign(payload, config.jwt.secret, signOptions);

    logger.info('User logged in', { userId: user.id });

    return {
      user: this.toPublicUser(user),
      token,
    };
  }

  /**
   * Get user by ID
   */
  async getUserById(userId: number): Promise<UserPublic> {
    const user = await prisma.user.findUnique({
      where: { id: userId },
    });

    if (!user) {
      throw new NotFoundError('User not found');
    }

    return this.toPublicUser(user);
  }

  /**
   * Update user avatar type (R6/R15)
   */
  async updateAvatarType(userId: number, avatarType: AvatarType): Promise<UserPublic> {
    const user = await prisma.user.update({
      where: { id: userId },
      data: { avatarType },
    });

    logger.info('Avatar type updated', { userId, avatarType });

    return this.toPublicUser(user);
  }

  /**
   * Get user's default body colors
   */
  async getBodyColors(userId: number): Promise<AvatarBodyColors> {
    const bodyColors = await prisma.avatarBodyColors.findUnique({
      where: { userId },
    });

    if (!bodyColors) {
      throw new NotFoundError('Body colors not found');
    }

    return {
      headColorId: bodyColors.headColorId,
      torsoColorId: bodyColors.torsoColorId,
      leftArmColorId: bodyColors.leftArmColorId,
      rightArmColorId: bodyColors.rightArmColorId,
      leftLegColorId: bodyColors.leftLegColorId,
      rightLegColorId: bodyColors.rightLegColorId,
    };
  }

  /**
   * Set user's body colors
   */
  async setBodyColors(userId: number, colors: AvatarBodyColors): Promise<AvatarBodyColors> {
    // Validate color IDs (should be between 1 and 208 for Roblox brick colors)
    const validColorIds = Object.values(colors);
    for (const colorId of validColorIds) {
      if (colorId < 1 || colorId > 208) {
        throw new ValidationError('Invalid body color ID. Must be between 1 and 208');
      }
    }

    const bodyColors = await prisma.avatarBodyColors.upsert({
      where: { userId },
      update: colors,
      create: {
        userId,
        ...colors,
      },
    });

    logger.info('Body colors updated', { userId });

    return {
      headColorId: bodyColors.headColorId,
      torsoColorId: bodyColors.torsoColorId,
      leftArmColorId: bodyColors.leftArmColorId,
      rightArmColorId: bodyColors.rightArmColorId,
      leftLegColorId: bodyColors.leftLegColorId,
      rightLegColorId: bodyColors.rightLegColorId,
    };
  }

  /**
   * Verify JWT token and return payload
   */
  verifyToken(token: string): JwtPayload {
    try {
      return jwt.verify(token, config.jwt.secret) as JwtPayload;
    } catch {
      throw new AuthenticationError('Invalid or expired token');
    }
  }

  /**
   * Convert user to public format (without sensitive data)
   */
  private toPublicUser(user: UserRecord): UserPublic {
    return {
      id: user.id,
      username: user.username,
      avatarType: user.avatarType as AvatarType,
      createdAt: user.createdAt,
    };
  }
}

// Singleton instance
export const userService = new UserService();
