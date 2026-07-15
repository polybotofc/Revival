import type { Config } from 'tailwindcss';

const config: Config = {
  content: [
    './pages/**/*.{js,ts,jsx,tsx,mdx}',
    './components/**/*.{js,ts,jsx,tsx,mdx}',
    './app/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  theme: {
    extend: {
      colors: {
        // Roblox-inspired color palette
        primary: {
          50: '#f0f4ff',
          100: '#e0e9ff',
          200: '#c7d9ff',
          300: '#a3c2ff',
          400: '#78a3ff',
          500: '#5580ff',
          600: '#3861ff',
          700: '#2847e8',
          800: '#2239b8',
          900: '#1e318a',
        },
        secondary: {
          50: '#fff8f0',
          100: '#ffefdb',
          200: '#ffdab3',
          300: '#ffbe7a',
          400: '#ff9a3d',
          500: '#ff7b0f',
          600: '#f55d00',
          700: '#cc4500',
          800: '#a33806',
          900: '#83330b',
        },
        dark: {
          100: '#44444f',
          200: '#38383f',
          300: '#2d2d36',
          400: '#26262e',
          500: '#1f1f27',
          600: '#1a1a21',
          700: '#15151c',
          800: '#111116',
          900: '#0d0d11',
        },
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
        mono: ['JetBrains Mono', 'monospace'],
      },
    },
  },
  plugins: [],
};

export default config;
