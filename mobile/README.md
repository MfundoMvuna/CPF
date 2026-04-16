# CPF Mobile App

React Native mobile app built with Expo and TypeScript for the Community Policing Forum.

## Getting Started

```bash
cd mobile
npm install
npx expo start
```

Scan the QR code with Expo Go on your phone, or press `a` for Android emulator / `i` for iOS simulator.

## Project Structure

```
mobile/
├── App.tsx                        # Entry point with auth state routing
├── src/
│   ├── config.ts                  # API base URL configuration
│   ├── types.ts                   # TypeScript interfaces
│   ├── services/
│   │   ├── api.ts                 # HTTP client with JWT + auto-refresh
│   │   ├── authService.ts         # Login / register / logout
│   │   ├── panicService.ts        # Trigger & fetch panic alerts
│   │   └── postService.ts         # Community feed CRUD
│   ├── stores/
│   │   ├── authStore.ts           # Zustand auth state
│   │   └── jwtDecode.ts           # Minimal JWT decoder
│   ├── components/
│   │   └── PanicButton.tsx        # Large emergency button with GPS
│   ├── screens/
│   │   ├── LoginScreen.tsx
│   │   ├── RegisterScreen.tsx
│   │   ├── HomeScreen.tsx         # Panic button home
│   │   ├── FeedScreen.tsx         # Community posts
│   │   └── ProfileScreen.tsx      # User info + logout
│   └── navigation/
│       ├── AuthNavigator.tsx      # Login ↔ Register flow
│       └── MainNavigator.tsx      # Bottom tabs (Home/Feed/Profile)
```

## Configuration

Edit `src/config.ts` to set the backend API URL. In development, use your LAN IP:

```ts
const API_BASE_URL = 'http://192.168.0.131:5164';
```

## Features

- **JWT Authentication** with SecureStore + automatic token refresh
- **Panic Button** — sends GPS coordinates via Expo Location
- **Community Feed** — view/create posts with pull-to-refresh
- **Profile** — view user info, sign out
