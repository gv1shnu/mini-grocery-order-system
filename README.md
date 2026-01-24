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

---

## Features
- View all products
- View product by ID
- Order placement with stock validation
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
	├── DTOs/          → Request data transfer objects
	├── Repositories/  → Database access (EF Core)
	├── wwwroot/       → Basic static HTML frontend
	├── Services/      → Business logic & transactions
	├── Properties/    → Local launch configuration
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
GET  /api/products
GET  /api/products/{id}
POST /api/orders
```

### Frontend (Basic)

Open in browser
```
http://localhost:5111/index.html
```

## Features

### Backend
- Project setup and clean structure
- Database models (Product, Order)
- EF Core configuration and migrations
- Seed initial product data
- Products API (GET /products)
- Order placement API (POST /orders)
- Order business logic with stock validation
- Transaction handling for orders

### Frontend (Basic)
- Implemented using a simple static HTML page
- Lists products using the Products API
- Allows placing orders using the Orders API
- Displays success or failure messages
- No UI or design focus as per requirements
