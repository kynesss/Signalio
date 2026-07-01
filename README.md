# Signalio

Real-time chat application built with Blazor WebAssembly, ASP.NET Core and SignalR. Signalio lets users register, sign in and exchange messages instantly, with a live list of who is currently online.

> Portfolio project focused on learning SignalR, real-time communication patterns and Blazor WebAssembly authentication.

## Highlights

- Real-time messaging broadcast to all connected clients over a SignalR Hub.
- JWT authentication issued on register and login, restored from localStorage on refresh.
- Custom AuthenticationStateProvider parsing JWT claims into a ClaimsPrincipal.
- Bearer token attached automatically to every HttpClient request via a DelegatingHandler.
- WebSocket authentication through a query-string access token.
- Live online-user tracking with join/leave notifications.
- Message history persisted to PostgreSQL and served over REST.
- Hosted Blazor WebAssembly setup where the server serves the client.

## Tech Stack

### Backend

- .NET 10 / ASP.NET Core
- SignalR (WebSocket-based real-time communication)
- ASP.NET Core Identity
- JWT bearer authentication
- Entity Framework Core + PostgreSQL (Npgsql)
- Minimal API endpoints

### Frontend

- Blazor WebAssembly (.NET 10)
- Custom TokenAuthenticationStateProvider
- HubConnection (Microsoft.AspNetCore.SignalR.Client)

## Architecture

The solution is split into three projects. The server hosts the API, the SignalR Hub and the compiled WebAssembly client, while contracts shared between both sides live in `Signalio.Shared`.

```text
Signalio.sln
  Signalio.Server/    ASP.NET Core host, SignalR ChatHub, auth and message endpoints
  Signalio/           Blazor WebAssembly client, auth state, chat page
  Signalio.Shared/    shared DTOs (MessageDto, LoginRequest, RegisterRequest, AuthResponse)
```

## Core Features

### Authentication

- Register and login through minimal API endpoints.
- JWT issued on both register and login.
- Token stored in localStorage and restored on page refresh.
- Custom AuthenticationStateProvider parsing JWT claims into a ClaimsPrincipal.
- AuthorizationMessageHandler automatically attaching the Bearer token to every HttpClient request.
- `[Authorize]` route guard with redirect to login.

### Chat

- Real-time message broadcast to all connected clients via the SignalR Hub.
- Message history loaded on connect through `GET /api/messages` (last 50).
- Online users list updated in real time on connect and disconnect.
- Notifications when a user joins or leaves the chat.
- Messages persisted to PostgreSQL.

## Getting Started

### Prerequisites

- .NET 10 SDK
- PostgreSQL or Docker

### 1. Clone the repository

```bash
git clone https://github.com/kynesss/Signalio.git
cd Signalio
```

### 2. Configure the server

Set the connection string and JWT options in `Signalio.Server/appsettings.json`:

- `ConnectionStrings:Default`
- `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiryMinutes`

Or start a database with Docker:

```bash
docker run --name signalio-db -e POSTGRES_PASSWORD=yourpassword -p 5432:5432 -d postgres
```

### 3. Apply migrations

```bash
dotnet ef database update --project Signalio.Server
```

### 4. Run the application

```bash
dotnet run --project Signalio.Server
```

The client is served by the server in hosted mode, so there is nothing to run separately.

### 5. Open the app

Open the browser at the URL shown in the console.

## API Areas

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/messages?take=50`

## What This Project Demonstrates

- Blazor WebAssembly over Blazor Server. The client is a standalone entity connecting to the Hub over HTTP/WebSocket, giving a clear picture of how SignalR works instead of hiding the connection inside a Blazor circuit.
- JWT over cookie authentication. Simpler with WebAssembly (no SameSite or CORS cookie issues), with the token passed via the `Authorization: Bearer` header for REST and via the query string (`?access_token=`) for WebSocket connections, since browsers cannot set custom headers on the WebSocket handshake.
- A custom TokenAuthenticationStateProvider instead of a library, for full control over JWT claim parsing and the Blazor auth state lifecycle.
- An AuthorizationMessageHandler (DelegatingHandler) that centralizes Bearer token attachment for all outgoing HttpClient requests without repeating it per call.
- OnlineUserService as a singleton backed by a ConcurrentDictionary. Online-user state is shared across all SignalR connections, each on its own thread, and the ConcurrentDictionary keeps that access thread-safe.
- Message history over REST (`GET /api/messages`) rather than the Hub. The Hub is responsible only for real-time events, while history retrieval stays a classic request/response, an intentional separation of concerns.
- An OnMessageReceived hook in JwtBearerEvents that extracts the token from the query string specifically for the `/chathub` path, keeping REST endpoints on a proper Authorization header.

## Roadmap

- Chat rooms using SignalR Groups.
- Typing indicator.
- Refresh token support.
- Public demo deployment.
- Screenshot and demo GIF in this README.
