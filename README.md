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
git clone https://github.com/gv1shnu/mini-grocery-order-system
cd mini-grocery-order-system
```

### 2. Run the application
```bash
dotnet run
```


### 3. Frontend (Basic)

Open in browser
```
http://localhost:5111/index.html
```


## API explanation
```
GET  /api/products       →  Retrieve all available products
GET  /api/products/{id}  →  Retrieve a specific product by ID

```
```
POST /api/orders         →  Place an order for a product
```
Request body
```{
  "productId": 1,
  "quantity": 2
}
```

## Responsibility by layer

### 1. Controllers
- Handle HTTP requests and responses
- Perform basic request validation
- Delegate processing to services only

### 2. Services
- Validate stock availability
- Handle order placement logic
- Manage transactions

### 3. Repositories
- Encapsulate database access
- Perform CRUD operations only


