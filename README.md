# Mini Grocery Order System

A simple ASP.NET Core Web API for managing grocery products and orders.
The project demonstrates clean separation of concerns using controllers, repositories,
and Entity Framework Core with SQLite.

---

## Backend Tech Stack
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- C#
- Git

---

## Features
- View all products
- View product by ID
- Order management (coming next)
- Database persistence using EF Core
- Clean separation of concerns (Controllers, Repositories, Services)

---

## Project Structure
```text
	MiniGrocery/
	├── Controllers/   → HTTP endpoints (request/response only)
	├── Data/          → DbContext & database configuration
	├── Migrations/    → Database schema changes
	├── Models/        → Entity definitions (tables)
	├── Repositories/  → Database access (EF Core)
	├── Services/      → Business logic & transactions
	├── Frontend/      → Basic UI (Ionic + Angular)
	├── Program.cs     → Application startup & DI setup
```

---

## Getting Started

### 1. Clone the repository
```bash
git clone <repo-url>
cd mini-grocery-order-system
```

### 2. Run the application
```bash
dotnet run
```

### 3. Test API Endpoints
```
GET /api/products
GET /api/products/{id}
```

## TODO

### Backend
- [x] Project setup and clean structure
- [x] Database models (Product, Order)
- [x] EF Core configuration and migrations
- [x] Seed initial product data
- [x] Products API (GET /products)
- [ ] Order placement API (POST /orders)
- [ ] Order business logic with stock validation
- [ ] Transaction handling for orders

### Frontend
- [ ] Initialize Ionic + Angular project
- [ ] Display product list
- [ ] Place order from UI
- [ ] Show success / failure messages
