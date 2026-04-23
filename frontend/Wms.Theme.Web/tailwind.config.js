/** @type {import('tailwindcss').Config} */
export default {
  darkMode: 'class',
  content: [
    "./Pages/**/*.{html,cshtml}",
    "./Views/**/*.{html,cshtml}",
    "./wwwroot/**/*.html"
  ],
  safelist: [
    // Dynamic status badges / utilities referenced in code blocks
    'bg-yellow-100', 'text-yellow-800', 'bg-green-100', 'text-green-800',
    'bg-red-100', 'text-red-800', 'bg-gray-100', 'text-gray-800',
    // Common utilities to avoid accidental purge during early scaffolding
    'bg-white', 'rounded-lg', 'shadow', 'shadow-md', 'px-6', 'py-2',
    'text-gray-900', 'text-gray-700', 'hover:bg-gray-50', 'hover:text-blue-900',
    'bg-blue-600', 'hover:bg-blue-700', 'text-white'
  ],
  theme: {
    extend: {
      colors: {
        primary: '#1f2937',
        secondary: '#6b7280',
      },
      fontFamily: {
        sans: ['Inter', 'sans-serif'],
      },
    },
  },
  plugins: [],
}
