import { config } from '../config/index.js';
import { logger } from '../utils/logger.js';
import { avatarService } from './AvatarService.js';
import { InternalError } from '../utils/errors.js';
import type { AvatarResponse, RenderResult } from '../types/index.js';

/**
 * Roblox Lua template for rendering avatars
 * This template will be used by RCC to render the avatar
 */
const AVATAR_LUA_TEMPLATE = `
local Players = game:GetService("Players")
local UserId = tonumber(ARGS.UserId) or 1
local AvatarType = ARGS.AvatarType or "R6"
local Size = tonumber(ARGS.Size) or 420
local CameraType = ARGS.CameraType or "Portrait"

-- Load avatar data from JSON
local AvatarData = nil
if ARGS.AvatarJSON and ARGS.AvatarJSON ~= "" then
    local success, result = pcall(function()
        return game:GetService("HttpService"):JSONDecode(ARGS.AvatarJSON)
    end)
    if success then
        AvatarData = result
    end
end

-- Body color IDs
local bodyColors = AvatarData and AvatarData.bodyColors or {
    headColorId = 194,
    torsoColorId = 194,
    leftArmColorId = 194,
    rightArmColorId = 194,
    leftLegColorId = 194,
    rightLegColorId = 194
}

-- Create humanoid description based on avatar type
local humanoidDescription = Players:GetHumanoidDescriptionFromBodyPartColors(
    Enum.HumanoidRigType[AvatarType] or Enum.HumanoidRigType.R6,
    Color3.fromRGB(194, 194, 194), -- Default color
    Color3.fromRGB(194, 194, 194),
    Color3.fromRGB(194, 194, 194),
    Color3.fromRGB(194, 194, 194),
    Color3.fromRGB(194, 194, 194)
)

-- Apply body colors
local function applyBodyColor(colorId)
    -- Roblox brick color mapping (simplified)
    local colors = {
        [194] = Color3.fromRGB(242, 231, 198), -- Pearl
        [199] = Color3.fromRGB(204, 178, 148), -- Reddish brown
        [21] = Color3.fromRGB(170, 85, 0), -- Bright orange
        [23] = Color3.fromRGB(170, 0, 0), -- Bright red
        [24] = Color3.fromRGB(0, 16, 176), -- Medium blue
        [26] = Color3.fromRGB(0, 170, 0), -- Bright green
        [28] = Color3.fromRGB(0, 170, 157), -- Bright yellowish green
        [37] = Color3.fromRGB(126, 91, 68), -- Dark stone grey
        [38] = Color3.fromRGB(156, 156, 156), -- Medium stone grey
        [45] = Color3.fromRGB(205, 84, 75), -- Bright red
        [101] = Color3.fromRGB(99, 94, 86), -- Dark taupe
        [102] = Color3.fromRGB(245, 245, 245), -- White
        [104] = Color3.fromRGB(174, 144, 104), -- Medium reddish violet
        [105] = Color3.fromRGB(117, 114, 106), -- Dark stone grey
        [106] = Color3.fromRGB(163, 162, 165), -- Dark taupe
        [107] = Color3.fromRGB(229, 228, 219), -- Warm grey
        [108] = Color3.fromRGB(215, 167, 113), -- Reddish brown
        [119] = Color3.fromRGB(191, 183, 165), -- Medium stone grey
        [125] = Color3.fromRGB(244, 183, 126), -- Bright orange
        [135] = Color3.fromRGB(191, 146, 104), -- Reddish brown
        [136] = Color3.fromRGB(162, 126, 94), -- Reddish brown
        [137] = Color3.fromRGB(119, 95, 71), -- Reddish brown
        [141] = Color3.fromRGB(181, 157, 126), -- Sand
        [143] = Color3.fromRGB(153, 102, 84), -- Brick yellow
        [145] = Color3.fromRGB(124, 90, 65), -- Sand blue
        [153] = Color3.fromRGB(203, 172, 140), -- Bright orange
        [157] = Color3.fromRGB(151, 151, 151), -- Medium stone grey
        [194] = Color3.fromRGB(242, 231, 198), -- Pearl
        [199] = Color3.fromRGB(204, 178, 148), -- Reddish brown
        [208] = Color3.fromRGB(0, 162, 210), -- Bright blue
    }
    return colors[colorId] or Color3.fromRGB(194, 194, 194)
end

-- Create the character
local player = Players:GetPlayerByUserId(UserId)
if not player then
    player = Players:CreatePlayer(UserId, "TempPlayer")
end

local character = Players:CreateHumanoidModelFromDescription(
    humanoidDescription,
    Enum.HumanoidRigType[AvatarType] or Enum.HumanoidRigType.R6
)

-- Set body colors for R6
if AvatarType == "R6" then
    local torso = character:FindFirstChild("Torso")
    if torso then
        torso.BrickColor = BrickColor.new(applyBodyColor(bodyColors.torsoColorId))
        local head = character:FindFirstChild("Head")
        if head then
            head.BrickColor = BrickColor.new(applyBodyColor(bodyColors.headColorId))
        end
        local leftArm = character:FindFirstChild("Left Arm")
        if leftArm then
            leftArm.BrickColor = BrickColor.new(applyBodyColor(bodyColors.leftArmColorId))
        end
        local rightArm = character:FindFirstChild("Right Arm")
        if rightArm then
            rightArm.BrickColor = BrickColor.new(applyBodyColor(bodyColors.rightArmColorId))
        end
        local leftLeg = character:FindFirstChild("Left Leg")
        if leftLeg then
            leftLeg.BrickColor = BrickColor.new(applyBodyColor(bodyColors.leftLegColorId))
        end
        local rightLeg = character:FindFirstChild("Right Leg")
        if rightLeg then
            rightLeg.BrickColor = BrickColor.new(applyBodyColor(bodyColors.rightLegColorId))
        end
    end
end

-- Equip assets (simplified - would need actual asset loading in production)
if AvatarData and AvatarData.assets then
    for _, asset in ipairs(AvatarData.assets) do
        -- In production, load actual assets from Roblox
        -- This is a placeholder for asset rendering
    end
end

-- Position camera for thumbnail
local camera = workspace.CurrentCamera
character.Parent = workspace

-- Camera positioning based on type
if CameraType == "Portrait" then
    camera.CameraType = Enum.CameraType.Fixed
    camera.CFrame = CFrame.new(character.PrimaryPart.Position + Vector3.new(0, 1.5, 5), character.PrimaryPart.Position + Vector3.new(0, 1.5, 0))
elseif CameraType == "Full" then
    camera.CameraType = Enum.CameraType.Fixed
    camera.CFrame = CFrame.new(character.PrimaryPart.Position + Vector3.new(0, 2, 8), character.PrimaryPart.Position + Vector3.new(0, 1, 0))
else
    camera.CameraType = Enum.CameraType.Fixed
    camera.CFrame = CFrame.new(character.PrimaryPart.Position + Vector3.new(0, 1.5, 5), character.PrimaryPart.Position + Vector3.new(0, 1.5, 0))
end

-- Render
wait(0.5)

local thumbnail = camera:CaptureThumbnail()
local imageData = thumbnail:EncodeToPNG()

-- Return result
return {
    success = true,
    imageData = imageData
}
`;

/**
 * RenderService handles avatar rendering using Roblox Cloud Console (RCC)
 * Generates thumbnails of avatars using RCCService SOAP API
 */
export class RenderService {
  private readonly rccUrl: string;

  constructor() {
    this.rccUrl = config.rcc.url;
  }

  /**
   * Generate avatar thumbnail
   * 
   * Flow:
   * 1. Get avatar JSON from AvatarService
   * 2. Create SOAP XML for RCC
   * 3. Send to RCC
   * 4. Receive Base64 PNG
   * 5. Return PNG image
   */
  async GenerateThumbnail(userId: number): Promise<RenderResult> {
    try {
      // Step 1: Get avatar data
      const avatarData = await avatarService.getAvatar(userId);
      logger.debug('Got avatar data for rendering', { userId });

      // Step 2: Generate SOAP XML
      const soapXml = this.generateSoapXml(userId, avatarData);

      // Step 3: Send to RCC
      const response = await this.sendToRCC(soapXml);

      // Step 4: Parse response
      const result = this.parseRCCResponse(response);

      return result;
    } catch (error) {
      logger.error('Failed to generate thumbnail', { userId, error });
      return {
        success: false,
        imageData: '',
        error: error instanceof Error ? error.message : 'Unknown error',
      };
    }
  }

  /**
   * Generate placeholder thumbnail when RCC is unavailable
   * Creates a simple SVG-based avatar representation
   */
  async generatePlaceholderThumbnail(userId: number): Promise<RenderResult> {
    try {
      const avatarData = await avatarService.getAvatar(userId);
      
      // Generate SVG placeholder
      const svg = this.generateAvatarSVG(avatarData);
      const base64 = Buffer.from(svg).toString('base64');

      return {
        success: true,
        imageData: base64,
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
   * Generate SOAP XML for RCC request
   */
  private generateSoapXml(userId: number, avatarData: AvatarResponse): string {
    const avatarJson = JSON.stringify(avatarData).replace(/"/g, '&quot;');

    return `<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
                xmlns:xsd="http://www.w3.org/2001/XMLSchema" 
                xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <ExecuteScriptRequest xmlns="http://tempuri.org/">
      <scriptName>RenderAvatar</script>
      <scriptContent><![CDATA[${AVATAR_LUA_TEMPLATE}]]></scriptContent>
      <args>
        <UserId>${userId}</UserId>
        <AvatarType>${avatarData.playerAvatarType}</AvatarType>
        <AvatarJSON>${avatarJson}</AvatarJSON>
        <Size>${config.thumbnail.size}</Size>
        <CameraType>${config.thumbnail.cameraType}</CameraType>
      </args>
    </ExecuteScriptRequest>
  </soap:Body>
</soap:Envelope>`;
  }

  /**
   * Send SOAP request to RCC
   */
  private async sendToRCC(soapXml: string): Promise<string> {
    try {
      const response = await fetch(`${this.rccUrl}/RCCService.asmx`, {
        method: 'POST',
        headers: {
          'Content-Type': 'text/xml; charset=utf-8',
          'SOAPAction': 'http://tempuri.org/ExecuteScript',
        },
        body: soapXml,
      });

      if (!response.ok) {
        throw new InternalError(`RCC request failed: ${response.status}`);
      }

      return await response.text();
    } catch (error) {
      logger.warn('RCC unavailable, using placeholder', { error });
      throw new InternalError('RCC service unavailable');
    }
  }

  /**
   * Parse RCC SOAP response
   */
  private parseRCCResponse(xmlResponse: string): RenderResult {
    // Extract Base64 image data from SOAP response
    // In a real implementation, parse the XML properly
    const base64Match = xmlResponse.match(/<Base64Image>([^<]+)<\/Base64Image>/i);
    
    if (base64Match && base64Match[1]) {
      return {
        success: true,
        imageData: base64Match[1],
      };
    }

    return {
      success: false,
      imageData: '',
      error: 'Failed to parse RCC response',
    };
  }

  /**
   * Generate SVG placeholder for avatar
   * Used when RCC is unavailable
   */
  private generateAvatarSVG(avatarData: AvatarResponse): string {
    const { bodyColors, playerAvatarType } = avatarData;
    
    // Map color IDs to RGB values
    const colorMap: Record<number, string> = {
      194: '#f2e7c6', // Pearl
      199: '#ccb294',
      21: '#aa5500',
      23: '#aa0000',
      24: '#0010b0',
      26: '#00aa00',
      28: '#00aa9d',
      37: '#7e5b44',
      38: '#9c9c9c',
      45: '#cd545b',
      101: '#635e56',
      102: '#f5f5f5',
      104: '#ae9068',
      105: '#756e6a',
      106: '#a3a2a5',
      107: '#e5ecdb',
      108: '#d7a771',
      119: '#bfb7a5',
      125: '#f4b77e',
      135: '#bf9268',
      136: '#a27e5e',
      137: '#775f47',
      141: '#b59d7e',
      143: '#996654',
      145: '#7c5a41',
      153: '#cbacc8',
      157: '#979797',
      208: '#00a2d2',
    };

    const getColor = (id: number): string => colorMap[id] ?? '#f2e7c6';

    if (playerAvatarType === 'R15') {
      // R15 body parts
      return `<svg xmlns="http://www.w3.org/2000/svg" width="420" height="420" viewBox="0 0 420 420">
        <rect width="420" height="420" fill="#e8e8e8"/>
        <circle cx="210" cy="80" r="50" fill="${getColor(bodyColors.headColorId)}"/>
        <rect x="175" y="130" width="70" height="100" fill="${getColor(bodyColors.torsoColorId)}"/>
        <rect x="110" y="140" width="65" height="80" fill="${getColor(bodyColors.leftArmColorId)}"/>
        <rect x="245" y="140" width="65" height="80" fill="${getColor(bodyColors.rightArmColorId)}"/>
        <rect x="180" y="230" width="28" height="100" fill="${getColor(bodyColors.leftLegColorId)}"/>
        <rect x="212" y="230" width="28" height="100" fill="${getColor(bodyColors.rightLegColorId)}"/>
        <text x="210" y="380" text-anchor="middle" fill="#666" font-family="Arial" font-size="14">R15 Avatar</text>
      </svg>`;
    } else {
      // R6 body parts
      return `<svg xmlns="http://www.w3.org/2000/svg" width="420" height="420" viewBox="0 0 420 420">
        <rect width="420" height="420" fill="#e8e8e8"/>
        <circle cx="210" cy="70" r="45" fill="${getColor(bodyColors.headColorId)}"/>
        <rect x="170" y="115" width="80" height="60" fill="${getColor(bodyColors.torsoColorId)}"/>
        <rect x="90" y="120" width="80" height="50" fill="${getColor(bodyColors.leftArmColorId)}"/>
        <rect x="250" y="120" width="80" height="50" fill="${getColor(bodyColors.rightArmColorId)}"/>
        <rect x="175" y="175" width="35" height="110" fill="${getColor(bodyColors.leftLegColorId)}"/>
        <rect x="210" y="175" width="35" height="110" fill="${getColor(bodyColors.rightLegColorId)}"/>
        <text x="210" y="350" text-anchor="middle" fill="#666" font-family="Arial" font-size="14">R6 Avatar</text>
      </svg>`;
    }
  }
}

// Singleton instance
export const renderService = new RenderService();
