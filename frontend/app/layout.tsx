import type { Metadata } from 'next';
import './globals.css';

export const metadata: Metadata = {
  title: 'Revival - Roblox Avatar System',
  description: 'Modern Roblox avatar revival system with RCC rendering',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body className="bg-dark-600 text-white min-h-screen">
        <div className="min-h-screen flex flex-col">
          {/* Header */}
          <header className="bg-dark-500 border-b border-dark-200 px-6 py-4">
            <div className="max-w-7xl mx-auto flex items-center justify-between">
              <a href="/" className="text-2xl font-bold text-primary-400 hover:text-primary-300 transition-colors">
                Revival
              </a>
              <nav className="flex items-center gap-4">
                <a href="/login" className="text-dark-100 hover:text-white transition-colors">
                  Login
                </a>
                <a href="/register" className="btn btn-primary">
                  Register
                </a>
              </nav>
            </div>
          </header>

          {/* Main content */}
          <main className="flex-1">
            {children}
          </main>

          {/* Footer */}
          <footer className="bg-dark-500 border-t border-dark-200 px-6 py-4 mt-auto">
            <div className="max-w-7xl mx-auto text-center text-dark-100 text-sm">
              Revival - Roblox Avatar Revival System © 2024
            </div>
          </footer>
        </div>
      </body>
    </html>
  );
}
