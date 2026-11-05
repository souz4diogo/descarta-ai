/// <reference types="vitest/config" />
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/testes/setup.ts'],
    css: false,
    // Valores fixos e fictícios: os testes não dependem do .env de quem roda.
    env: {
      VITE_API_URL: 'http://api.teste/v1',
      VITE_CONTATO_EMAIL: 'contato@teste.dev',
      VITE_NSI_CNPJ: '00.000.000/0001-00',
    },
  },
})
