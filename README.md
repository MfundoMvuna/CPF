# CPF — Community Policing Forum

A full-stack application for community policing coordination, featuring a .NET 8 REST API backend and a React Native (Expo) mobile app.

## Project Structure

```
CPF/
├── src/                        # Backend (.NET 8)
│   ├── CPF.API/                # Minimal API — runs on Kestrel or AWS Lambda
│   ├── CPF.Application/        # DTOs, interfaces (service contracts)
│   ├── CPF.Domain/             # Entities & enums
│   └── CPF.Infrastructure/     # EF Core, JWT, AWS SNS, Yoco payments
├── mobile/                     # Mobile app (Expo / React Native)
├── template.yaml               # AWS SAM deployment template
├── samconfig.toml               # SAM CLI configuration
└── CPF.sln                     # Solution file
```

## Features

| Module | Description |
|--------|-------------|
| **Auth** | Register, login, JWT access/refresh tokens |
| **Panic Alerts** | Trigger GPS-located panic alerts, view active alerts, SNS notifications |
| **Community Feed** | Create and browse community posts (paginated) |
| **Shifts** | View shifts, check-in/check-out with GPS |
| **Payments** | Yoco payment integration with webhook processing |

## API Endpoints

| Method | Path | Auth | Description |
|--------|------|:----:|-------------|
| GET | `/api/health` | | Health check |
| POST | `/api/auth/register` | | Register a new user |
| POST | `/api/auth/login` | | Login |
| POST | `/api/auth/refresh` | | Refresh JWT tokens |
| POST | `/api/panic/trigger` | ✔ | Trigger a panic alert |
| GET | `/api/panic/active` | ✔ | Get active panic alerts |
| POST | `/api/payments/create` | ✔ | Create a Yoco payment |
| POST | `/api/payments/webhook` | | Yoco webhook callback |
| GET | `/api/shifts` | ✔ | List user shifts |
| POST | `/api/shifts/{id}/checkin` | ✔ | Check in to a shift |
| POST | `/api/shifts/{id}/checkout` | ✔ | Check out of a shift |
| GET | `/api/posts` | ✔ | List community posts |
| POST | `/api/posts` | ✔ | Create a community post |

## Tech Stack

### Backend
- **.NET 8** — Minimal APIs
- **Entity Framework Core** — PostgreSQL (Npgsql) / In-Memory for dev
- **JWT** — Authentication & authorization
- **AWS Lambda** — Serverless hosting via `Amazon.Lambda.AspNetCoreServer.Hosting`
- **AWS SNS** — Push notifications for panic alerts
- **Yoco** — Payment processing

### Mobile
- **React Native 0.76** with **Expo SDK 52**
- **React Navigation 7** — Stack & bottom-tab navigation
- **Zustand** — State management
- **Expo Location** — GPS for panic alerts & shift check-ins
- **Expo Secure Store** — Token storage

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (LTS)
- [AWS SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html) (for deployment)

### Run the API locally

```bash
cd src/CPF.API
dotnet run
```

The API starts at `http://localhost:5164` with an in-memory database by default.

### Run the mobile app

```bash
cd mobile
npm install --legacy-peer-deps
npm run web       # browser
npm run android   # Android emulator
npm run ios       # iOS simulator
```

### Deploy to AWS

```bash
sam build
sam deploy --parameter-overrides \
  PostgreSQLConnection="Host=...;Database=cpf_db;..." \
  JwtSecret="your-secret-min-32-chars"
```

See [template.yaml](template.yaml) for all configurable parameters.

## Configuration

### Backend (`appsettings.json`)

| Key | Description |
|-----|-------------|
| `ConnectionStrings:PostgreSQL` | PostgreSQL connection string |
| `Jwt:Secret` | JWT signing key (min 32 characters) |
| `Jwt:Issuer` / `Jwt:Audience` | Token issuer & audience |
| `Yoco:SecretKey` | Yoco API secret key |
| `Yoco:WebhookSecret` | Yoco webhook signature secret |

### Mobile (`src/config.ts`)

Update `API_BASE_URL` to point to your running API or deployed API Gateway URL.

## License

Private — All rights reserved.
