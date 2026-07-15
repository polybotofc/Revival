import { config } from '../config/index.js';
import { logger } from '../utils/logger.js';
import { avatarService } from './AvatarService.js';
import type { AvatarResponse, RenderResult } from '../types/index.js';
import fs from 'fs';
import path from 'path';
import { spawn } from 'child_process';

/**
 * RenderService handles avatar rendering using Roblox Cloud Console (RCC)
 * Generates thumbnails by spawning RCC process and reading output files
 */
export class RenderService {
  private readonly thumbnailDir: string;

  constructor() {
    this.thumbnailDir = config.thumbnail.outputDir;
    // Ensure thumbnail directory exists
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
   * 3. Spawn RCC process to render
   * 4. Read generated PNG from output directory
   * 5. Return PNG image
   */
  async GenerateThumbnail(userId: number): Promise<RenderResult> {
    try {
      // Step 1: Get avatar data
      const avatarData = await avatarService.getAvatar(userId);
      logger.debug('Got avatar data for rendering', { userId });

      // Step 2: Create job file for RCC
      const jobId = `avatar_${userId}_${Date.now()}`;
      const jobFilePath = await this.createJobFile(jobId, userId, avatarData);

      // Step 3: Spawn RCC to process the job
      const outputPath = await this.spawnRCC(jobId, jobFilePath);

      // Step 4: Read the generated PNG
      if (outputPath && fs.existsSync(outputPath)) {
        const imageBuffer = fs.readFileSync(outputPath);
        const base64 = imageBuffer.toString('base64');
        
        // Clean up
        try {
          fs.unlinkSync(jobFilePath);
          fs.unlinkSync(outputPath);
        } catch { /* ignore cleanup errors */ }

        logger.info('Thumbnail generated via RCC', { userId, outputPath });
        return {
          success: true,
          imageData: base64,
        };
      }

      // RCC didn't produce output, use placeholder
      logger.warn('RCC did not produce output, using placeholder', { userId });
      return this.generatePlaceholderThumbnail(userId);

    } catch (error) {
      logger.error('Failed to generate thumbnail via RCC', { userId, error });
      // Fallback to placeholder
      return this.generatePlaceholderThumbnail(userId);
    }
  }

  /**
   * Create RCC job file
   */
  private async createJobFile(jobId: string, userId: number, _avatarData: AvatarResponse): Promise<string> {
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

    const jobFilePath = path.join(this.thumbnailDir, `job_${jobId}.json`);
    fs.writeFileSync(jobFilePath, JSON.stringify(jobConfig, null, 2));
    
    logger.debug('Created RCC job file', { jobId, jobFilePath });
    return jobFilePath;
  }

  /**
   * Spawn RCC process to render thumbnail
   */
  private spawnRCC(jobId: string, jobFilePath: string): Promise<string | null> {
    return new Promise((resolve) => {
      // Check if RCC executable exists
      if (!fs.existsSync(config.rcc.executablePath)) {
        logger.warn('RCC executable not found', { path: config.rcc.executablePath });
        resolve(null);
        return;
      }

      // Spawn RCC process
      const rccArgs = [
        '-console',
        '-verbose',
        '-localtest',
        jobFilePath,
        '-settingsfile',
        'DevSettingsFile.json',
      ];

      logger.info('Spawning RCC process', { 
        executable: config.rcc.executablePath,
        args: rccArgs 
      });

      const rccProcess = spawn(config.rcc.executablePath, rccArgs, {
        cwd: path.dirname(config.rcc.executablePath),
        timeout: 60000,
      });

      let stderr = '';
      let stdout = '';

      rccProcess.stderr?.on('data', (data: Buffer) => {
        stderr += data.toString();
      });

      rccProcess.stdout?.on('data', (data: Buffer) => {
        stdout += data.toString();
        
        // Check for success in output
        if (stdout.includes('ThumbnailGenerator::click() success')) {
          // RCC reports success - look for output file
          const possibleOutputs = [
            path.join(this.thumbnailDir, `thumb_${jobId}.png`),
            path.join(path.dirname(config.rcc.executablePath), `thumb_${jobId}.png`),
            path.join(this.thumbnailDir, 'output.png'),
            path.join(path.dirname(config.rcc.executablePath), 'output.png'),
          ];

          for (const outPath of possibleOutputs) {
            if (fs.existsSync(outPath)) {
              logger.info('Found RCC output', { outputPath: outPath });
              rccProcess.kill();
              resolve(outPath);
              return;
            }
          }
        }
      });

      rccProcess.on('error', (error: Error) => {
        logger.error('RCC process error', { error: error.message });
        resolve(null);
      });

      rccProcess.on('close', (code: number | null) => {
        logger.debug('RCC process exited', { code });
        
        if (code === 0) {
          // Process completed - look for output
          const possibleOutputs = [
            path.join(this.thumbnailDir, `thumb_${jobId}.png`),
            path.join(path.dirname(config.rcc.executablePath), `thumb_${jobId}.png`),
            path.join(this.thumbnailDir, 'output.png'),
          ];

          for (const outPath of possibleOutputs) {
            if (fs.existsSync(outPath)) {
              resolve(outPath);
              return;
            }
          }
        }
        
        resolve(null);
      });

      // Timeout after 60 seconds
      setTimeout(() => {
        rccProcess.kill();
        resolve(null);
      }, 60000);
    });
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
   * Generate improved SVG placeholder for avatar
   */
  private generateAvatarSVG(avatarData: AvatarResponse): string {
    const { bodyColors, playerAvatarType } = avatarData;
    
    // Map color IDs to RGB values
    const colorMap: Record<number, string> = {
      194: '#f2e7c6', // Pearl (skin)
      199: '#ccb294', // Reddish brown
      21: '#aa5500',  // Bright orange
      23: '#aa0000',  // Bright red
      24: '#0010b0',  // Medium blue
      26: '#00aa00',  // Bright green
      28: '#00aa9d',  // Bright yellowish green
      37: '#7e5b44',  // Dark stone grey
      38: '#9c9c9c',  // Medium stone grey
      45: '#cd545b',  // Bright red
      101: '#635e56',  // Dark taupe
      102: '#f5f5f5',  // White
      104: '#ae9068',  // Medium reddish violet
      105: '#756e6a',  // Dark stone grey
      106: '#a3a2a5',  // Dark taupe
      107: '#e5ecdb',  // Warm grey
      108: '#d7a771',  // Reddish brown
      119: '#bfb7a5',  // Medium stone grey
      125: '#f4b77e',  // Bright orange
      135: '#bf9268',  // Reddish brown
      136: '#a27e5e',  // Reddish brown
      137: '#775f47',  // Reddish brown
      141: '#b59d7e',  // Sand
      143: '#996654',  // Brick yellow
      145: '#7c5a41',  // Sand blue
      153: '#cbacc8',  // Bright orange
      157: '#979797',  // Medium stone grey
      208: '#00a2d2', // Bright blue
      1: '#f5f5f5',    // Bright red
      5: '#a9a9a9',    // Medium red
      9: '#f2cb2f',    // Bright yellow
      11: '#f5f5f5',   // Bright orange
      12: '#9c9c9c',   // Dark stone grey
      13: '#f5f5f5',   // Bright violet
      18: '#e4e4e4',   // Nougat
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
        <defs>
          <linearGradient id="bgGrad" x1="0%" y1="0%" x2="0%" y2="100%">
            <stop offset="0%" style="stop-color:#f0f0f0"/>
            <stop offset="100%" style="stop-color:#e0e0e0"/>
          </linearGradient>
        </defs>
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
        <defs>
          <linearGradient id="bgGrad" x1="0%" y1="0%" x2="0%" y2="100%">
            <stop offset="0%" style="stop-color:#f0f0f0"/>
            <stop offset="100%" style="stop-color:#e0e0e0"/>
          </linearGradient>
        </defs>
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
