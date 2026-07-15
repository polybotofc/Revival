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
   * Creates the job file in D:\New folder to avoid path issues with spaces
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

    // Create job file in D:\New folder to avoid path issues
    const rccDir = path.dirname(config.rcc.executablePath);
    const jobId = `thumb_${userId}_${Date.now()}`;
    const jobFilePath = path.join(rccDir, `job_${jobId}.json`);
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

      const rccDir = path.dirname(config.rcc.executablePath);
      logger.info('Spawning RCC process', { 
        executable: config.rcc.executablePath,
        jobFile: jobFilePath,
        cwd: rccDir
      });

      // Build command - use spawn directly without shell
      const exePath = config.rcc.executablePath;
      
      // Use spawn without shell - Node.js handles Windows paths natively
      const rccProcess = spawn(exePath, [
        '-console',
        '-verbose',
        '-localtest',
        jobFilePath,
        '-settingsfile',
        'DevSettingsFile.json',
      ], {
        cwd: rccDir,
        windowsHide: false,
      });

      let stdout = '';
      let stderr = '';

      rccProcess.stdout?.on('data', (data: Buffer) => {
        const text = data.toString();
        stdout += text;
        
        if (text.includes('ThumbnailGenerator::click() success')) {
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
        
        // Clean up job file
        try {
          fs.unlinkSync(jobFilePath);
        } catch { /* ignore cleanup errors */ }
        
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
    // Pattern: Capture Base64 between THUMBNAIL_START and THUMBNAIL_END
    const startMarker = 'THUMBNAIL_START';
    const endMarker = 'THUMBNAIL_END';
    
    const startIdx = output.indexOf(startMarker);
    if (startIdx === -1) {
      return null;
    }
    
    const endIdx = output.indexOf(endMarker, startIdx + startMarker.length);
    if (endIdx === -1) {
      return null;
    }
    
    // Extract Base64 string between markers
    const base64 = output.substring(startIdx + startMarker.length, endIdx).trim();
    
    // Validate it's not empty and looks like Base64
    if (base64.length < 1000 || !/^[A-Za-z0-9+/=]+$/.test(base64)) {
      return null;
    }
    
    // Validate PNG magic bytes
    if (this.isValidBase64Png(base64)) {
      return base64;
    }
    
    return null;
  }

  /**
   * Validate if Base64 string is likely a PNG image
   */
  private isValidBase64Png(base64: string): boolean {
    try {
      const decoded = Buffer.from(base64, 'base64');
      
      // Must be at least 1000 chars to be a real PNG thumbnail
      if (decoded.length < 1000) {
        return false;
      }
      
      // Check PNG magic bytes: 89 50 4E 47 (iVBOR in Base64)
      if (decoded.length > 4) {
        const isPng = decoded[0] === 0x89 && 
                      decoded[1] === 0x50 && 
                      decoded[2] === 0x4E && 
                      decoded[3] === 0x47;
        return isPng;
      }
      
      return false;
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
