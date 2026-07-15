'use client';

import { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { auth, avatarApi } from '@/lib/api';
import type { User, Avatar } from '@/lib/types';

export default function ProfilePage() {
  const router = useRouter();
  const [user, setUser] = useState<User | null>(null);
  const [avatar, setAvatar] = useState<Avatar | null>(null);
  const [loading, setLoading] = useState(true);
  const [thumbnailUrl, setThumbnailUrl] = useState('');
  const [actionLoading, setActionLoading] = useState(false);
  const [error, setError] = useState('');

  // Check authentication
  useEffect(() => {
    const currentUser = auth.getUser();
    if (!currentUser) {
      router.push('/login');
      return;
    }
    setUser(currentUser);
  }, [router]);

  // Load avatar data
  const loadAvatar = useCallback(async () => {
    if (!user) return;
    
    try {
      const avatarData = await avatarApi.getAvatar(user.id);
      setAvatar(avatarData);
      setThumbnailUrl(avatarApi.getThumbnailUrl(user.id));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load avatar');
    }
  }, [user]);

  useEffect(() => {
    if (user) {
      loadAvatar().finally(() => setLoading(false));
    }
  }, [user, loadAvatar]);

  // Refresh avatar thumbnail
  const handleRefreshThumbnail = () => {
    if (user) {
      setThumbnailUrl(avatarApi.getThumbnailUrl(user.id) + `&refresh=${Date.now()}`);
    }
  };

  // Switch avatar type
  const handleSwitchAvatarType = async () => {
    if (!user || !avatar) return;
    
    setActionLoading(true);
    setError('');

    try {
      const newType = avatar.playerAvatarType === 'R6' ? 'R15' : 'R6';
      const updatedUser = await avatarApi.switchAvatarType({
        userId: user.id,
        avatarType: newType,
      });
      
      setUser(updatedUser);
      auth.saveAuth({ user: updatedUser, token: auth.getToken() ?? '' });
      
      // Reload avatar
      await loadAvatar();
      handleRefreshThumbnail();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to switch avatar type');
    } finally {
      setActionLoading(false);
    }
  };

  // Logout
  const handleLogout = () => {
    auth.clearAuth();
    router.push('/login');
  };

  if (!user) {
    return null;
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[calc(100vh-200px)]">
        <div className="text-center">
          <div className="spinner border-primary-500 border-t-transparent w-8 h-8 mx-auto mb-4" />
          <p className="text-dark-100">Loading profile...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      {/* Error message */}
      {error && (
        <div className="mb-6 p-4 bg-red-900/30 border border-red-700 rounded-lg text-red-400 text-sm">
          {error}
        </div>
      )}

      <div className="grid md:grid-cols-2 gap-8">
        {/* Avatar Display */}
        <div className="card">
          <h2 className="text-xl font-semibold mb-4">Avatar Preview</h2>
          
          <div className="relative aspect-square bg-dark-500 rounded-lg overflow-hidden mb-4">
            {thumbnailUrl ? (
              <img
                src={thumbnailUrl}
                alt="Avatar Preview"
                className="w-full h-full object-contain"
                onError={() => setThumbnailUrl('')}
              />
            ) : (
              <div className="w-full h-full flex items-center justify-center">
                <div className="spinner border-primary-500 border-t-transparent w-8 h-8" />
              </div>
            )}
          </div>

          <button
            onClick={handleRefreshThumbnail}
            disabled={actionLoading}
            className="btn btn-secondary w-full flex items-center justify-center gap-2"
          >
            {actionLoading ? (
              <div className="spinner w-4 h-4" />
            ) : (
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
              </svg>
            )}
            Refresh Avatar
          </button>
        </div>

        {/* User Info */}
        <div className="space-y-6">
          <div className="card">
            <h2 className="text-xl font-semibold mb-4">Profile</h2>
            
            <div className="space-y-4">
              <div>
                <label className="text-sm text-dark-100">Username</label>
                <p className="text-lg font-medium">{user.username}</p>
              </div>
              
              <div>
                <label className="text-sm text-dark-100">User ID</label>
                <p className="text-lg font-mono">{user.id}</p>
              </div>
              
              <div>
                <label className="text-sm text-dark-100">Avatar Type</label>
                <div className="flex items-center gap-2">
                  <span className="text-lg font-medium">{avatar?.playerAvatarType ?? user.avatarType}</span>
                  <button
                    onClick={handleSwitchAvatarType}
                    disabled={actionLoading}
                    className="btn btn-secondary text-sm px-3 py-1"
                  >
                    Switch to {avatar?.playerAvatarType === 'R6' ? 'R15' : 'R6'}
                  </button>
                </div>
              </div>
              
              <div>
                <label className="text-sm text-dark-100">Joined</label>
                <p className="text-lg">{new Date(user.createdAt).toLocaleDateString()}</p>
              </div>
            </div>
          </div>

          {/* Body Colors */}
          {avatar && (
            <div className="card">
              <h2 className="text-xl font-semibold mb-4">Body Colors</h2>
              
              <div className="grid grid-cols-3 gap-3">
                <BodyColorDisplay name="Head" colorId={avatar.bodyColors.headColorId} />
                <BodyColorDisplay name="Torso" colorId={avatar.bodyColors.torsoColorId} />
                <BodyColorDisplay name="Left Arm" colorId={avatar.bodyColors.leftArmColorId} />
                <BodyColorDisplay name="Right Arm" colorId={avatar.bodyColors.rightArmColorId} />
                <BodyColorDisplay name="Left Leg" colorId={avatar.bodyColors.leftLegColorId} />
                <BodyColorDisplay name="Right Leg" colorId={avatar.bodyColors.rightLegColorId} />
              </div>
            </div>
          )}

          {/* Equipped Assets */}
          {avatar && avatar.assets.length > 0 && (
            <div className="card">
              <h2 className="text-xl font-semibold mb-4">Equipped Assets</h2>
              
              <div className="space-y-2">
                {avatar.assets.map((asset) => (
                  <div key={asset.id} className="flex items-center justify-between p-3 bg-dark-500 rounded-lg">
                    <div>
                      <p className="font-medium">{asset.name}</p>
                      <p className="text-sm text-dark-100">{asset.assetType.name}</p>
                    </div>
                    <span className="text-xs font-mono text-dark-100">#{asset.id}</span>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Logout Button */}
          <button
            onClick={handleLogout}
            className="btn btn-danger w-full"
          >
            Logout
          </button>
        </div>
      </div>
    </div>
  );
}

// Body color display component
function BodyColorDisplay({ name, colorId }: { name: string; colorId: number }) {
  // Roblox brick color mapping
  const colorMap: Record<number, string> = {
    194: '#f2e7c6',
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

  return (
    <div className="text-center">
      <div 
        className="w-full aspect-square rounded-lg mb-1 border border-dark-200"
        style={{ backgroundColor: colorMap[colorId] ?? '#f2e7c6' }}
      />
      <p className="text-xs text-dark-100">{name}</p>
      <p className="text-xs font-mono">{colorId}</p>
    </div>
  );
}
