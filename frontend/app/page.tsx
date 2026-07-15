import Link from 'next/link';

export default function Home() {
  return (
    <div className="flex flex-col items-center justify-center min-h-[calc(100vh-200px)] px-4">
      {/* Hero Section */}
      <div className="text-center max-w-3xl">
        <h1 className="text-5xl md:text-6xl font-bold mb-6 bg-gradient-to-r from-primary-400 to-secondary-400 bg-clip-text text-transparent">
          Welcome to Revival
        </h1>
        <p className="text-xl text-dark-100 mb-8">
          A modern Roblox avatar revival system with RCC rendering.
          Create, customize, and render your avatars with ease.
        </p>
        
        {/* Features */}
        <div className="grid md:grid-cols-3 gap-6 mb-12">
          <div className="card text-center">
            <div className="text-4xl mb-4">🎮</div>
            <h3 className="text-lg font-semibold mb-2">Avatar Types</h3>
            <p className="text-dark-100 text-sm">
              Support for both R6 and R15 avatar types
            </p>
          </div>
          <div className="card text-center">
            <div className="text-4xl mb-4">🎨</div>
            <h3 className="text-lg font-semibold mb-2">Customization</h3>
            <p className="text-dark-100 text-sm">
              Full body color customization with Roblox palette
            </p>
          </div>
          <div className="card text-center">
            <div className="text-4xl mb-4">📸</div>
            <h3 className="text-lg font-semibold mb-2">RCC Rendering</h3>
            <p className="text-dark-100 text-sm">
              Real-time avatar thumbnails via RCCService
            </p>
          </div>
        </div>

        {/* CTA Buttons */}
        <div className="flex flex-col sm:flex-row gap-4 justify-center">
          <Link href="/register" className="btn btn-primary text-lg px-8 py-3">
            Get Started
          </Link>
          <Link href="/login" className="btn btn-secondary text-lg px-8 py-3">
            Login
          </Link>
        </div>
      </div>

      {/* API Status */}
      <div className="mt-16 w-full max-w-2xl">
        <div className="card">
          <h2 className="text-lg font-semibold mb-4">API Endpoints</h2>
          <div className="space-y-2 text-sm font-mono">
            <div className="flex justify-between">
              <span className="text-dark-100">POST</span>
              <span>/api/auth/register</span>
            </div>
            <div className="flex justify-between">
              <span className="text-dark-100">POST</span>
              <span>/api/auth/login</span>
            </div>
            <div className="flex justify-between">
              <span className="text-dark-100">GET</span>
              <span>/api/avatar/:userId</span>
            </div>
            <div className="flex justify-between">
              <span className="text-dark-100">POST</span>
              <span>/api/avatar/equip</span>
            </div>
            <div className="flex justify-between">
              <span className="text-dark-100">POST</span>
              <span>/api/avatar/unequip</span>
            </div>
            <div className="flex justify-between">
              <span className="text-dark-100">POST</span>
              <span>/api/avatar/thumbnail</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
