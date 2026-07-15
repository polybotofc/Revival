import { config } from '../config/index.js';
import { logger } from '../utils/logger.js';
import { avatarService } from './AvatarService.js';
import type { AvatarResponse, RenderResult } from '../types/index.js';
import http from 'http';

/**
 * RenderService handles avatar rendering using Roblox Cloud Console (RCC)
 * Uses SOAP/HTTP API to communicate with RCC Service
 */
export class RenderService {
  /**
   * Generate avatar thumbnail
   * 
   * Flow:
   * 1. Get avatar JSON from AvatarService
   * 2. Send SOAP request to RCC Service API
   * 3. Parse response for Base64 PNG
   * 4. Return PNG image
   */
  async GenerateThumbnail(userId: number): Promise<RenderResult> {
    try {
      // Step 1: Get avatar data
      const avatarData = await avatarService.getAvatar(userId);
      logger.debug('Got avatar data for rendering', { userId });

      // Step 2: Send request to RCC Service
      const result = await this.requestThumbnailFromRCC(avatarData, userId);

      if (result) {
        logger.info('Thumbnail generated via RCC', { userId });
        return {
          success: true,
          imageData: result,
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
   * Request thumbnail from RCC Service via HTTP SOAP
   */
  private requestThumbnailFromRCC(_avatarData: AvatarResponse, userId: number): Promise<string | null> {
    return new Promise((resolve) => {
      const rccHost = config.rcc.host;
      const rccPort = config.rcc.port;
      const baseUrl = `http://${rccHost}:${config.port}`;
      const avatarUrl = `${baseUrl}/api/avatar/${userId}`;

      // Create SOAP request for thumbnail
      const soapEnvelope = this.createThumbnailSoapRequest(baseUrl, avatarUrl, userId);

      logger.info('Sending thumbnail request to RCC Service', { 
        host: rccHost, 
        port: rccPort 
      });

      const options = {
        hostname: rccHost,
        port: rccPort,
        path: '/RCCService.asmx',
        method: 'POST',
        headers: {
          'Content-Type': 'text/xml; charset=utf-8',
          'SOAPAction': 'http://roblox.com/RCCService/ExecuteJob',
          'Content-Length': Buffer.byteLength(soapEnvelope),
        },
        timeout: 60000,
      };

      const req = http.request(options, (res) => {
        const chunks: Buffer[] = [];
        
        res.on('data', (chunk: Buffer) => {
          chunks.push(chunk);
        });

        res.on('end', () => {
          const response = Buffer.concat(chunks).toString('utf8');
          logger.debug('RCC response received', { 
            statusCode: res.statusCode,
            responseLength: response.length 
          });

          // Parse SOAP response to extract Base64 image
          const base64Image = this.parseSoapResponse(response);
          
          if (base64Image) {
            resolve(base64Image);
          } else {
            // Try alternative response formats
            const altResult = this.parseAlternativeResponse(response);
            resolve(altResult);
          }
        });
      });

      req.on('error', (error) => {
        logger.error('RCC request failed', { error: error.message });
        resolve(null);
      });

      req.on('timeout', () => {
        req.destroy();
        logger.error('RCC request timed out');
        resolve(null);
      });

      req.write(soapEnvelope);
      req.end();
    });
  }

  /**
   * Create SOAP envelope for thumbnail request
   */
  private createThumbnailSoapRequest(baseUrl: string, avatarUrl: string, userId: number): string {
    const jobId = `thumb_${userId}_${Date.now()}`;

    return `<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
               xmlns:xsd="http://www.w3.org/2001/XMLSchema" 
               xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/"
               xmlns:rbx="http://roblox.com/RCCService">
  <soap:Body>
    <rbx:ExecuteJob>
      <rbx:job>
        <rbx:id>${jobId}</rbx:id>
        <rbx:type>Thumbnail</rbx:type>
        <rbx:expireTime>2025-12-31T23:59:59Z</rbx:expireTime>
        <rbx:input>
          <rbx:thumbnailType>Avatar</rbx:thumbnailType>
          <rbx:targetId>0</rbx:targetId>
          <rbx:thumbnailSize>
            <rbx:width>${config.thumbnail.size}</rbx:width>
            <rbx:height>${config.thumbnail.size}</rbx:height>
          </rbx:thumbnailSize>
          <rbx:format>PNG</rbx:format>
          <rbx:avatarUrl>${avatarUrl}</rbx:avatarUrl>
        </rbx:input>
        <rbx:endpoint>${baseUrl}</rbx:endpoint>
      </rbx:job>
    </rbx:ExecuteJob>
  </soap:Body>
</soap:Envelope>`;
  }

  /**
   * Parse SOAP response to extract Base64 image
   */
  private parseSoapResponse(response: string): string | null {
    const patterns = [
      /<Result>([A-Za-z0-9+/=]+)<\/Result>/i,
      /<data>([A-Za-z0-9+/=]+)<\/data>/i,
      /<thumbnail>([A-Za-z0-9+/=]+)<\/thumbnail>/i,
      /<image>([A-Za-z0-9+/=]+)<\/image>/i,
      /<png>([A-Za-z0-9+/=]+)<\/png>/i,
      /<Base64Image>([^<]+)<\/Base64Image>/i,
      /<Output>([^<]+)<\/Output>/i,
    ];

    for (const pattern of patterns) {
      const match = response.match(pattern);
      if (match && match[1]) {
        return match[1];
      }
    }

    return null;
  }

  /**
   * Try alternative response parsing
   */
  private parseAlternativeResponse(response: string): string | null {
    const trimmed = response.trim();
    
    if (/^[A-Za-z0-9+/=]{100,}$/.test(trimmed)) {
      return trimmed;
    }

    try {
      const json = JSON.parse(trimmed);
      if (json.imageData || json.thumbnail || json.data || json.result) {
        return json.imageData || json.thumbnail || json.data || json.result;
      }
    } catch {
      // Not JSON
    }

    return null;
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
