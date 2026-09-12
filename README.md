# Realtime Kanban Board

A Kanban board with real-time collaboration. When a user moves, creates, or edits a card, every other user viewing the same board will see the update instantly via SignalR, with no page refresh required.

> **Project status: in progress.** Backend (Board → Column → Card CRUD with authorization, JWT auth) and a read-only React frontend (login, board list, board detail view showing columns and cards) are implemented and working. Card drag-and-drop, board member invitations, update/delete endpoints, and the SignalR real-time layer are still being built. See [Roadmap] below for the current state.

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
- Create and retrieve boards, scoped to the authenticated user (`BoardMember` membership required)
- Create and retrieve columns within a board
- Create cards within a column, with title and description
- Move a card between columns (or reorder within a column), with consistent ordering enforced by the `Board` aggregate
- Read-only React frontend: login, board list, and board detail view (columns + cards)

**Planned (MVP):**
- Live updates for all connected users viewing the same board (SignalR)

### Stretch goals

- Invite other users to a specific board
- Comments on cards
- Basic notifications (e.g. "X moved card Y")
- Unit tests (xUnit) for backend logic
- CI pipeline via GitHub Actions (build + test on push)

## Architecture

**Planned end state:**
```
React (TypeScript) ──HTTP──▶ ASP.NET Core Web API ──▶ PostgreSQL
        │                          (EF Core)
        │
        └──WebSocket──▶ SignalR Hub ──▶ broadcasts updates
                          to all clients in the same board group
```

Board mutations (create/move/edit) will go through the REST API, persist to PostgreSQL via EF Core, and then broadcast to all clients connected to that board's SignalR group. SignalR was chosen over polling to avoid unnecessary request overhead and to get genuinely instant updates across clients.

**Currently implemented:** REST API with EF Core + PostgreSQL, JWT authentication via ASP.NET Core Identity, and Board/Column endpoints. `Board` is modeled as an **Aggregate Root**: `Column` and `Card` are only ever created or modified through `Board` (e.g. `board.AddColumn(...)`), never directly via their own repositories. This keeps invariants, such as every board having exactly one owner, or card ordering staying consistent, enforced in one place rather than scattered across services. The SignalR layer and the frontend are not yet built.

Backend solution structure:
```
backend/
├── src/
│   ├── Kanban.Api/            → Controllers, Program.cs, (SignalR hubs later)
│   ├── Kanban.Application/    → Use cases, service interfaces, DTOs — depends only on Domain
│   ├── Kanban.Domain/         → Entities (Board, Column, Card, User, BoardMember), no external dependencies
│   └── Kanban.Infrastructure/ → EF Core DbContext, Identity, implementations of Application interfaces
└── tests/
    └── Kanban.Domain.Tests/   → xUnit (planned)
```

## Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/) 8 or later
- [Node.js](https://nodejs.org/) (LTS)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Setup

```bash
git clone https://github.com/<your-username>/realtime-kanban.git
cd realtime-kanban

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

## Technical Decisions

- **PostgreSQL over SQL Server**: chosen to demonstrate stack flexibility beyond my prior SQL Server experience, and because it's easier to host for free when deploying a portfolio project.
- **Guid primary keys**: used across all entities for consistency and to avoid coupling to any single database's auto-increment mechanics.
- **`ApplicationUser` (Infrastructure) kept separate from the domain `User` entity**: a deliberate departure from letting ASP.NET Core Identity's `IdentityUser` be the domain entity directly. This keeps `Kanban.Domain` free of framework dependencies and testable in isolation, at the cost of some mapping code (e.g. registration creates both an `ApplicationUser` and a domain `User` with the same Id) and losing automatic EF Core navigation between them.
- **Dedicated `Kanban.Application` layer**: application logic (e.g. `AuthService`) lives here as interfaces + implementations, depending only on `Domain`. `Infrastructure` implements those interfaces (`IIdentityService`, `IUserRepository`), keeping the dependency direction pointing inward: `Api → Application → Domain`, with `Infrastructure` also depending on `Application` rather than the other way around. This makes application logic testable without a database or ASP.NET Identity.
- **Composite-key-eligible `BoardMember` given its own `Id` instead**: `(UserId, BoardId)` is already a natural unique key, but a surrogate `Id` was chosen for simplicity and consistency with the rest of the model, backed by a unique index on `(UserId, BoardId)` to preserve the actual constraint.
- **`Board` as an Aggregate Root**: `Column` and `Card` have no public setters and no standalone repositories; all mutation happens through `Board` (`AddColumn`, `AddCard`, `MoveCard`), using `internal` methods on the child entities so only `Kanban.Domain` code can bypass the root. This was introduced specifically because card-move operations need to keep `Order` consistent within a column, which is easier to guarantee in one place than to re-validate in every service that touches cards.
- **dnd-kit over react-beautiful-dnd** *(planned)*: actively maintained and has better TypeScript support.
- **SignalR groups per board** *(planned)*: updates will be broadcast only to clients viewing the relevant board, rather than globally, so the design scales to multiple concurrent boards.

## What I Learned

To be added

## Roadmap / Not Yet Implemented

- [x] Data model + PostgreSQL database + migrations
- [x] User registration and login (JWT)
- [x] Board create/read, scoped to authenticated user
- [x] Column create/read, nested under a board
- [x] Board refactored into an Aggregate Root (Column/Card only mutable via Board)
- [x] Card create/read and move-between-columns 
- [x] Read-only React frontend (login, board list, board detail view)
- [ ] SignalR hub for real-time updates (backend implemented, not yet wired to frontend/tested)
- [ ] Drag-and-drop for cards (dnd-kit)
- [ ] Board member invitations (add non-owner members)
- [ ] Board / Column / Card update & delete
- [ ] Stretch goals (see above)

This section is kept up to date as the project progresses, rather than implying the project is further along than it is.

## License

MIT
