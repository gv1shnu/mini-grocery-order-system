# Mini Grocery Order System

A small ASP.NET Core Web API for ordering grocery products, built around one question that
turns out to be the only interesting thing about a shop: **what happens when two people buy
the last bag of rice at the same time?**

This started life as a 3-day take-home exercise with a deliberately tight brief — two
endpoints, a mandated Controller/Service/Repository layering, one database transaction.
I've since kept the shape and gone deeper on correctness rather than wider on features.

---

## The part worth reading

Placing an order is a read-then-write: check the stock, then deduct it. That pattern is a
lost-update race waiting to happen. Two requests both read `stock = 10`, both decide there's
plenty, and both write `9`. Two units sold, one unit deducted.

`OrderService` avoids this by wrapping the whole read-then-write in a single transaction:

```csharp
using var transaction = await _context.Database.BeginTransactionAsync();

var product = await _productRepository.GetByIdAsync(request.ProductId);
if (product.Stock < request.Quantity) return /* rejected */;
product.Stock -= request.Quantity;
// ...
await transaction.CommitAsync();
```

The detail that makes this work is easy to miss: **`Microsoft.Data.Sqlite` opens transactions
with `BEGIN IMMEDIATE`**, which acquires SQLite's write lock at `BeginTransactionAsync` and
holds it through commit. The stock read happens *inside* that lock, so order placement
serializes. There is no window for a second reader to see a stale stock level.

This isn't a claim I'm asking you to take on faith — `OrderConcurrencyTests` fires 50
simultaneous orders at a product with 10 in stock and asserts the invariant:

```
200 OK: 10
400 BadRequest: 40
succeeded=10  remainingStock=0  ordersRecorded=10
```

Delete the two transaction lines and the same test reports:

```
200 OK: 50
succeeded=50  remainingStock=9  ordersRecorded=50
```

Fifty orders from ten units of stock, and the stock lands at 9 because every request read 10
and wrote 9. The transaction is load-bearing, and the test is what proves it rather than
assumes it.

### The trade-off

Serializing on the database's write lock is the right call at this size and the wrong one at
scale: every order placement is globally serialized, so throughput is one order at a time
regardless of whether the orders touch the same product. Two directions out of that, neither
of which this project needs:

- **Optimistic concurrency** — mark `Stock` as a concurrency token, let EF add
  `WHERE Stock = @original` to the update, and retry on `DbUpdateConcurrencyException`.
  Contention costs a retry instead of a wait, and orders for *different* products stop
  blocking each other.
- **Row-level locking** on an engine that has it — `SELECT ... FOR UPDATE` on PostgreSQL
  narrows the lock to the one product row instead of the whole database.

SQLite is a single-writer database. The honest answer to "how does this scale?" is "it
doesn't, and that's a deliberate choice for a seeded demo."

---

## Tech stack

ASP.NET Core (.NET 10) · Entity Framework Core · SQLite · xUnit

## Getting started

```bash
git clone https://github.com/gv1shnu/mini-grocery-order-system
cd mini-grocery-order-system
dotnet run --project MiniGrocery
```

Migrations are applied at startup, so a fresh clone comes up with a seeded database and no
extra steps. The API listens on `http://localhost:5111`.

- Frontend: <http://localhost:5111/index.html>
- OpenAPI document: <http://localhost:5111/openapi/v1.json>

```bash
dotnet test
```

## API

```
GET  /api/products       →  200  All products
GET  /api/products/{id}  →  200  One product
                            404  No such product
```

```
POST /api/orders         →  200  Order placed, returns { "orderId": 1 }
                            400  Quantity below 1
                            404  No such product
                            409  Requested quantity exceeds stock
```

```jsonc
// POST /api/orders
{ "productId": 1, "quantity": 2 }
```

Failures return [RFC 9110 Problem Details](https://datatracker.ietf.org/doc/html/rfc9110),
and the four outcomes are distinguishable by status code — a missing product (404) and an
out-of-stock product (409) are genuinely different things, and a client should be able to
tell them apart without parsing prose.

## Project structure

```text
MiniGrocery/
├── Controllers/   → HTTP only: map a service result onto a status code
├── Services/      → Business logic: validation, stock rules, the transaction
├── Repositories/  → Database access via EF Core
├── Models/        → Entities (Product, Order)
├── DTOs/          → Request contracts and their validation attributes
├── Data/          → DbContext and seed data
├── Migrations/    → EF Core schema history
├── wwwroot/       → Deliberately minimal HTML frontend
└── Program.cs     → Startup, DI, migration-on-boot

MiniGrocery.Tests/ → Integration tests over the real app (WebApplicationFactory)
```

## Where the business logic lives

All of it is in `Services/OrderService.cs`. Specifically:

| Rule | Where |
| --- | --- |
| Quantity must be ≥ 1 | `OrderRequestDto` attribute + `OrderService` guard |
| Product must exist | `OrderService` |
| Stock must cover the quantity | `OrderService` |
| Stock deduction and order insert are atomic | `OrderService` transaction |
| Total price = unit price × quantity | `OrderService` |

`OrdersController` contains no rules. It calls the service, receives a `PlaceOrderResult`,
and translates the outcome into a status code — that's its entire job. The service never
mentions HTTP.

## Tests

Eleven integration tests run against the real application through `WebApplicationFactory`,
each class against its own throwaway SQLite file. They cover the concurrency invariant, the
stock boundary (ordering *exactly* the remaining stock must succeed), and each of the four
order outcomes.

Three of them are regression guards for bugs this repo actually had:

- **Negative quantity.** `{"quantity": -5}` passed the `stock < quantity` check, *increased*
  stock by 5, and recorded an order with a total price of −250.
- **`CreatedAt` was never assigned**, so every order was stamped `DateTime.MinValue` — the
  year 1. `OrderService` now takes a `TimeProvider`, which also makes the clock injectable.
- **404 and 409 were the same response.** The service returned `bool`, so "no such product"
  and "not enough stock" both surfaced as `400 Insufficient stock or invalid product.`

## Deliberately not here

Auth, Docker, a message queue, caching, an SPA. This is a seeded demo of one narrow problem;
adding infrastructure it doesn't need would obscure the part that's actually worth reading.
An `Order` also references exactly one product rather than carrying order lines — a real
system would model a basket, but the original brief fixed the schema and the concurrency
question doesn't get more interesting with more rows.

## License

MIT — see [LICENSE](LICENSE).
