import { fileURLToPath, URL } from 'node:url';

import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import vueJsx from '@vitejs/plugin-vue-jsx';
import vueDevTools from 'vite-plugin-vue-devtools';

const https = process.env.CI
  ? undefined
  : {
    key: fileURLToPath(new URL('./.certs/key.pem', import.meta.url)),
    cert: fileURLToPath(new URL('./.certs/cert.pem', import.meta.url)),
  };

export default defineConfig({
  server: {
    host: true,
    port: parseInt(process.env.PORT || '3001'),
    https: https,
    proxy: {
      '/api': {
        target: process.env.VITE_API_URL,
        changeOrigin: true,
        rewrite: path => path.replace(/^\/api/, ''),
        secure: false,
      },
    },
    watch: {
      usePolling: true,
      interval: 1000,
    },
  },
  plugins: [vue(), vueJsx(), vueDevTools()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
});
