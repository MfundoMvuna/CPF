// API configuration
// Use your machine's LAN IP so the phone/emulator can reach the backend
const API_BASE_URL = __DEV__
  ? 'http://192.168.0.131:5164'
  : 'https://your-production-api.com';

export const Config = {
  API_BASE_URL,
} as const;
