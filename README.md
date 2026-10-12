![CI](https://github.com/thetruesaftblandaren/kanban-board/actions/workflows/ci.yml/badge.svg)

# Realtime Kanban Board

A Kanban board with real-time collaboration. When a user moves, creates, or edits a card, every other user viewing the same board will see the update instantly via SignalR, with no page refresh required.

> **Project status: in progress.** Backend (Board → Column → Card CRUD with authorization, JWT auth), real-time updates via SignalR, and a React frontend with drag-and-drop (login, board list, board detail view) are implemented and working. Board member invitations are still being built. See [Roadmap](#roadmap--not-yet-implemented) below for the current state.

## Why this project

Built as a portfolio project to demonstrate fullstack development skills as a junior developer, with a deliberate focus on real-time communication (SignalR), clean architecture, and a technology stack broader than what I've used professionally so far.

## Tech Stack

**Backend:** C# / ASP.NET Core Web API, Entity Framework Core, SignalR  
**Frontend:** React, TypeScript, dnd-kit  
**Database:** PostgreSQL  
**Auth:** JWT-based authentication (ASP.NET Core Identity)  
**Infrastructure:** Docker Compose

## Features

**Implemented:**
- User registration and login (JWT-based, via ASP.NET Core Identity)
- Data model and PostgreSQL database with migrations (Board, Column, Card, User, BoardMember)
- `Board` modeled as an Aggregate Root: Column/Card are only ever created or mutated through Board
- Full Board/Column/Card create, read, update, and delete, scoped to the authenticated user (`BoardMember` membership required)
- Move a card between columns (or reorder within a column), with consistent ordering enforced by the `Board` aggregate
- Drag and drop a column to change order
- React frontend: login, register, logout, board list, board detail view, and create/edit/delete forms for boards, columns, and cards
- Drag-and-drop for cards and columns (dnd-kit). The UI updates when the server's SignalR broadcast arrives, so every client, including the one dragging, goes through the same path.
- Real-time updates via SignalR: card and column changes (create, update, delete, move) and board rename/delete are broadcast live to everyone viewing the same board. On the board page, SignalR is the single source of truth for UI state: components only call the REST API and never update state themselves, which avoids duplicate-update bugs.
- Sessions stay alive while the user is active: short-lived access tokens (15 min) are renewed automatically using rotating refresh tokens stored server-side as hashes. Logout revokes the refresh token.
- Error handling: Validation in the domain layer, surfaced as error messages in the UI
- Unit tests (xUnit) for domain logic
- CI pipeline via GitHub Actions (build + test on push)

### Stretch goals

- Invite other users to a specific board
- Comments on cards
- Basic notifications (e.g. "X moved card Y")

## Architecture

**Planned end state:**
```
React (TypeScript) ──HTTP──▶ ASP.NET Core Web API ──▶ PostgreSQL
        │                          (EF Core)
        │
        └──WebSocket──▶ SignalR Hub ──▶ broadcasts updates
                          to all clients in the same board group
```

Board mutations (create/move/edit) go through the REST API, persist to PostgreSQL via EF Core, and then broadcast to all clients connected to that board's SignalR group. SignalR was chosen over polling to avoid unnecessary request overhead and to get genuinely instant updates across clients.

**Currently implemented:** REST API with EF Core + PostgreSQL, JWT authentication via ASP.NET Core Identity, and Board/Column endpoints. `Board` is modeled as an **Aggregate Root**: `Column` and `Card` are only ever created or modified through `Board` (e.g. `board.AddColumn(...)`), never directly via their own repositories. This keeps invariants, such as every board having exactly one owner, or card ordering staying consistent, enforced in one place rather than scattered across services.

Backend solution structure:
```
backend/
├── src/
│   ├── Kanban.Api/            → Controllers, Program.cs
│   ├── Kanban.Application/    → Use cases, service interfaces, DTOs — depends only on Domain
│   ├── Kanban.Domain/         → Entities (Board, Column, Card, User, BoardMember), no external dependencies
│   └── Kanban.Infrastructure/ → EF Core DbContext, Identity, implementations of Application interfaces, SignalR hub and notification service
└── tests/
    └── Kanban.Domain.Tests/   → xUnit tests for the `Board` aggregate
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Node.js](https://nodejs.org/) (LTS)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Setup

```bash
git clone https://github.com/thetruesaftblandaren/kanban-board.git
cd kanban-board

# Copy environment template and fill in values (used by docker-compose)
cp .env.example .env

# Start PostgreSQL
docker-compose up -d
```

### Backend secrets

This project uses .NET user-secrets for local development, so no database password or JWT key is stored in the repo. After cloning:

```bash
cd backend/src/Kanban.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=kanban;Username=kanban;Password=<your-local-password>"
dotnet user-secrets set "Jwt:Key" "<your-generated-key, at least 32 random characters>"
```

### Run the backend

```bash
cd backend/src/Kanban.Api
dotnet ef database update
dotnet run
```

### Run the frontend

Requires the backend to be running (see above) for API calls to work.

```bash
cd frontend
npm install
npm run dev
```

Frontend will be available at `http://localhost:5173`.

### Run the tests

```bash
dotnet test backend
```

## Technical Decisions

- **PostgreSQL over SQL Server**: chosen to demonstrate stack flexibility beyond my prior SQL Server experience, and because it's easier to host for free when deploying a portfolio project.
- **Guid primary keys**: used across all entities for consistency and to avoid coupling to any single database's auto-increment mechanics.
- **`ApplicationUser` (Infrastructure) kept separate from the domain `User` entity**: a deliberate departure from letting ASP.NET Core Identity's `IdentityUser` be the domain entity directly. This keeps `Kanban.Domain` free of framework dependencies and testable in isolation, at the cost of some mapping code (e.g. registration creates both an `ApplicationUser` and a domain `User` with the same Id) and losing automatic EF Core navigation between them.
- **Dedicated `Kanban.Application` layer**: application logic (e.g. `AuthService`) lives here as interfaces + implementations, depending only on `Domain`. `Infrastructure` implements those interfaces (`IIdentityService`, `IUserRepository`), keeping the dependency direction pointing inward: `Api → Application → Domain`, with `Infrastructure` also depending on `Application` rather than the other way around. This makes application logic testable without a database or ASP.NET Identity.
- **Composite-key-eligible `BoardMember` given its own `Id` instead**: `(UserId, BoardId)` is already a natural unique key, but a surrogate `Id` was chosen for simplicity and consistency with the rest of the model, backed by a unique index on `(UserId, BoardId)` to preserve the actual constraint.
- **`Board` as an Aggregate Root**: `Column` and `Card` have no public setters and no standalone repositories; all mutation happens through `Board` (`AddColumn`, `AddCard`, `MoveCard`), using `internal` methods on the child entities so only `Kanban.Domain` code can bypass the root. This was introduced specifically because card-move operations need to keep `Order` consistent within a column, which is easier to guarantee in one place than to re-validate in every service that touches cards.
- **dnd-kit over react-beautiful-dnd**: actively maintained and has better TypeScript support. Cards and columns use `useSortable`, with a vertical `SortableContext` per column and a horizontal one around the columns.
- **SignalR groups per board**: each board has its own group (`board-{id}`). Clients join when they open a board and leave when they navigate away, so updates only reach the people viewing that board.
- **Access and refresh tokens**: the access token is a JWT that lives for 15 minutes, and an axios interceptor renews it automatically. Refresh tokens are random strings stored only as SHA-256 hashes, valid for 7 days, single-use (rotated on every renewal), and revoked on logout. Because each renewal extends the session, an active user is never logged out. They are kept in `localStorage` for simplicity; `httpOnly` cookies would protect better against XSS but add CORS complexity and constrain hosting.
- **`INotificationService` abstraction**: the interface lives in `Kanban.Application` and the SignalR implementation (`SignalRNotificationService`, `BoardHub`) in `Kanban.Infrastructure`, so the application layer has no dependency on SignalR.
- **`Column` sorts its cards by `Order` before mutating them**: EF Core returns child collections in no guaranteed order, so removing and inserting by index put cards in the wrong place until the list was sorted by `Order` first. `Board` uses the same approach for columns.

## What I Learned

To be added

## Roadmap / Not Yet Implemented

- [x] Data model + PostgreSQL database + migrations
- [x] User registration and login (JWT)
- [x] Board refactored into an Aggregate Root (Column/Card only mutable via Board)
- [x] Board create, read, update, and delete, scoped to authenticated user
- [x] Column create, read, update, and delete
- [x] Card create, read, update, and delete
- [x] Move a card between columns (or reorder within a column)
- [x] Change column order by dragging and dropping
- [x] React frontend: login, board/column/card creation and editing, board detail view with drag-and-drop
- [x] SignalR real-time updates for all card operations (create, update, delete, move), single source of truth for card state
- [x] Error handling
- [x] Register new user
- [x] Logout
- [x] Session renewal with refresh tokens
- [x] Unit tests (implemented functionality only)
- [x] CI pipeline
- [ ] Styling
- [ ] Card categories (bug / feature / task)
- [ ] Board member invitations (add non-owner members)
- [ ] Assign cards to board members (requires invitations)

This section is kept up to date as the project progresses, rather than implying the project is further along than it is.

## Known Limitations

- Roles (`Owner`, `Editor`, `Viewer`) are stored, but only membership is enforced. A `Viewer` would currently be able to edit.
- The Delete button on a board is shown to every member. The server only allows the owner to delete, so others get an error message.
- Expired and revoked refresh tokens are not cleaned out of the database.
- The unit tests cover domain logic only. They cannot catch bugs that depend on the order EF Core loads data in, which would need an integration test against a real database.

## License

MIT
