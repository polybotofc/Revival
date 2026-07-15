import { config } from '../config/index.js';
import { logger } from '../utils/logger.js';
import { avatarService } from './AvatarService.js';
import type { AvatarResponse, RenderResult } from '../types/index.js';
import { spawn } from 'child_process';
import fs from 'fs';
import path from 'path';

/**
 * RenderService handles avatar rendering using Roblox Cloud Console (RCC)
 * Spawns RCC process and captures Base64 PNG from console output
 */
export class RenderService {
  private readonly thumbnailDir: string;

  constructor() {
    this.thumbnailDir = config.thumbnail.outputDir;
    if (!fs.existsSync(this.thumbnailDir)) {
      fs.mkdirSync(this.thumbnailDir, { recursive: true });
    }
  }

  /**
   * Generate avatar thumbnail
   * 
   * Flow:
   * 1. Get avatar JSON from AvatarService
   * 2. Create job file for RCC
   * 3. Spawn RCC process
   * 4. Capture Base64 PNG from stdout
   * 5. Return PNG image
   */
  async GenerateThumbnail(userId: number): Promise<RenderResult> {
    try {
      // Validate user exists
      await avatarService.getAvatar(userId);
      logger.debug('Got avatar data for rendering', { userId });

      const jobFilePath = await this.createJobFile(userId);
      const result = await this.spawnRCCAndCapture(userId, jobFilePath);

      try {
        fs.unlinkSync(jobFilePath);
      } catch { /* ignore cleanup errors */ }

      if (result) {
        logger.info('Thumbnail generated via RCC', { userId });
        return {
          success: true,
          imageData: result,
        };
      }

      logger.warn('RCC did not produce output, using placeholder', { userId });
      return this.generatePlaceholderThumbnail(userId);

    } catch (error) {
      logger.error('Failed to generate thumbnail via RCC', { userId, error });
      return this.generatePlaceholderThumbnail(userId);
    }
  }

  /**
   * Create RCC job file
   */
  private async createJobFile(userId: number): Promise<string> {
    const jobConfig = [{
      Mode: 'Thumbnail',
      Settings: {
        Type: 'Avatar',
        Arguments: [
          `http://127.0.0.1:${config.port}`,
          `http://127.0.0.1:${config.port}/api/avatar/${userId}`,
          'PNG',
          config.thumbnail.size,
          config.thumbnail.size,
        ],
      },
      Arguments: {},
    }];

    const jobId = `avatar_${userId}_${Date.now()}`;
    const jobFilePath = path.resolve(this.thumbnailDir, `job_${jobId}.json`);
    fs.writeFileSync(jobFilePath, JSON.stringify(jobConfig, null, 2));
    
    logger.debug('Created RCC job file', { jobId, jobFilePath });
    return jobFilePath;
  }

  /**
   * Spawn RCC process and capture Base64 PNG from stdout
   */
  private spawnRCCAndCapture(_userId: number, jobFilePath: string): Promise<string | null> {
    return new Promise((resolve) => {
      if (!fs.existsSync(config.rcc.executablePath)) {
        logger.warn('RCC executable not found', { path: config.rcc.executablePath });
        resolve(null);
        return;
      }

      logger.info('Spawning RCC process', { 
        executable: config.rcc.executablePath,
        jobFile: jobFilePath
      });

      const rccProcess = spawn(config.rcc.executablePath, [
        '-console',
        '-verbose',
        '-localtest',
        jobFilePath,
        '-settingsfile',
        'DevSettingsFile.json',
      ], {
        cwd: path.dirname(config.rcc.executablePath),
      });

      let stdout = '';
      let stderr = '';

      rccProcess.stdout?.on('data', (data: Buffer) => {
        const text = data.toString();
        stdout += text;
        
        if (stdout.includes('ThumbnailGenerator::click() success')) {
          logger.info('RCC reported success');
        }
      });

      rccProcess.stderr?.on('data', (data: Buffer) => {
        stderr += data.toString();
      });

      rccProcess.on('error', (error: Error) => {
        logger.error('RCC process error', { error: error.message });
        resolve(null);
      });

      rccProcess.on('close', (code: number | null) => {
        logger.debug('RCC process exited', { code });
        
        const base64Image = this.parseBase64FromOutput(stdout);
        
        if (base64Image) {
          logger.info('Found Base64 PNG in RCC output', { length: base64Image.length });
          resolve(base64Image);
        } else {
          if (stderr) {
            logger.warn('RCC stderr', { stderr: stderr.substring(0, 500) });
          }
          resolve(null);
        }
      });

      setTimeout(() => {
        rccProcess.kill();
        resolve(null);
      }, 60000);
    });
  }

  /**
   * Parse Base64 PNG from RCC stdout
   */
  private parseBase64FromOutput(output: string): string | null {
    const patterns = [
      /return\s+"([A-Za-z0-9+/=]{500,})"/,
      /THUMBNAIL:([A-Za-z0-9+/=]{500,})/,
      /([A-Za-z0-9+/=]{5000,})/,
    ];

    for (const pattern of patterns) {
      const match = output.match(pattern);
      if (match && match[1]) {
        const potentialBase64 = match[1];
        if (this.isValidBase64Png(potentialBase64)) {
          return potentialBase64;
        }
      }
    }

    return null;
  }

  /**
   * Validate if Base64 string is likely a PNG image
   */
  private isValidBase64Png(base64: string): boolean {
    try {
      const decoded = Buffer.from(base64, 'base64');
      if (decoded.length > 4) {
        const isPng = decoded[0] === 0x89 && 
                      decoded[1] === 0x50 && 
                      decoded[2] === 0x4E && 
                      decoded[3] === 0x47;
        if (isPng) {
          return true;
        }
      }
      
      if (base64.length < 1000) {
        return false;
      }
      
      return true;
    } catch {
      return false;
    }
  }

  /**
   * Generate placeholder thumbnail when RCC is unavailable
   */
  async generatePlaceholderThumbnail(userId: number): Promise<RenderResult> {
    try {
      const avatarData = await avatarService.getAvatar(userId);
      const svg = this.generateAvatarSVG(avatarData);
      const base64 = Buffer.from(svg).toString('base64');

      logger.info('Using SVG placeholder for thumbnail', { userId });
      return {
        success: true,
        imageData: base64,
        isPlaceholder: true,
      };
    } catch (error) {
      return {
        success: false,
        imageData: '',
        error: error instanceof Error ? error.message : 'Unknown error',
      };
    }
  }

  /**
   * Generate SVG placeholder for avatar
   */
  private generateAvatarSVG(avatarData: AvatarResponse): string {
    const { bodyColors, playerAvatarType } = avatarData;
    
    const colorMap: Record<number, string> = {
      194: '#f2e7c6', 199: '#ccb294', 21: '#aa5500', 23: '#aa0000',
      24: '#0010b0', 26: '#00aa00', 28: '#00aa9d', 37: '#7e5b44',
      38: '#9c9c9c', 45: '#cd545b', 101: '#635e56', 102: '#f5f5f5',
      104: '#ae9068', 105: '#756e6a', 106: '#a3a2a5', 107: '#e5ecdb',
      108: '#d7a771', 119: '#bfb7a5', 125: '#f4b77e', 135: '#bf9268',
      136: '#a27e5e', 137: '#775f47', 141: '#b59d7e', 143: '#996654',
      145: '#7c5a41', 153: '#cbacc8', 157: '#979797', 208: '#00a2d2',
      1: '#f5f5f5', 5: '#a9a9a9', 9: '#f2cb2f', 11: '#f5f5f5',
      12: '#9c9c9c', 13: '#f5f5f5', 18: '#e4e4e4',
    };

    const getColor = (id: number): string => colorMap[id] ?? '#f2e7c6';
    const skinColor = getColor(bodyColors.headColorId);
    const torsoColor = getColor(bodyColors.torsoColorId);
    const leftArmColor = getColor(bodyColors.leftArmColorId);
    const rightArmColor = getColor(bodyColors.rightArmColorId);
    const leftLegColor = getColor(bodyColors.leftLegColorId);
    const rightLegColor = getColor(bodyColors.rightLegColorId);

    if (playerAvatarType === 'R15') {
      return `<svg xmlns="http://www.w3.org/2000/svg" width="420" height="420" viewBox="0 0 420 420">
        <defs><linearGradient id="bgGrad" x1="0%" y1="0%" x2="0%" y2="100%">
          <stop offset="0%" style="stop-color:#f0f0f0"/><stop offset="100%" style="stop-color:#e0e0e0"/>
        </linearGradient></defs>
        <rect width="420" height="420" fill="url(#bgGrad)"/>
        <circle cx="210" cy="75" r="50" fill="${skinColor}" stroke="#333" stroke-width="1"/>
        <rect x="160" y="125" width="100" height="110" rx="10" fill="${torsoColor}" stroke="#333" stroke-width="1"/>
        <rect x="105" y="130" width="55" height="90" rx="8" fill="${leftArmColor}" stroke="#333" stroke-width="1"/>
        <rect x="260" y="130" width="55" height="90" rx="8" fill="${rightArmColor}" stroke="#333" stroke-width="1"/>
        <rect x="168" y="235" width="40" height="120" rx="8" fill="${leftLegColor}" stroke="#333" stroke-width="1"/>
        <rect x="212" y="235" width="40" height="120" rx="8" fill="${rightLegColor}" stroke="#333" stroke-width="1"/>
        <text x="210" y="400" text-anchor="middle" fill="#666" font-family="Arial" font-size="16" font-weight="bold">R15 Avatar</text>
      </svg>`;
    } else {
      return `<svg xmlns="http://www.w3.org/2000/svg" width="420" height="420" viewBox="0 0 420 420">
        <defs><linearGradient id="bgGrad" x1="0%" y1="0%" x2="0%" y2="100%">
          <stop offset="0%" style="stop-color:#f0f0f0"/><stop offset="100%" style="stop-color:#e0e0e0"/>
        </linearGradient></defs>
        <rect width="420" height="420" fill="url(#bgGrad)"/>
        <circle cx="210" cy="65" r="45" fill="${skinColor}" stroke="#333" stroke-width="1"/>
        <rect x="165" y="110" width="90" height="70" rx="5" fill="${torsoColor}" stroke="#333" stroke-width="1"/>
        <rect x="85" y="115" width="80" height="55" rx="8" fill="${leftArmColor}" stroke="#333" stroke-width="1"/>
        <rect x="255" y="115" width="80" height="55" rx="8" fill="${rightArmColor}" stroke="#333" stroke-width="1"/>
        <rect x="168" y="180" width="38" height="120" rx="8" fill="${leftLegColor}" stroke="#333" stroke-width="1"/>
        <rect x="214" y="180" width="38" height="120" rx="8" fill="${rightLegColor}" stroke="#333" stroke-width="1"/>
        <text x="210" y="350" text-anchor="middle" fill="#666" font-family="Arial" font-size="16" font-weight="bold">R6 Avatar</text>
      </svg>`;
    }
  }
}

// Singleton instance
export const renderService = new RenderService();
