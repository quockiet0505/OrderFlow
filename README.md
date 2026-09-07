# OrderFlow

Order fulfilment microservices coding challenge.

## Services

- Orders
- Inventory
- Payments
- Blazor WebAssembly Client

## Architecture

Each backend service follows Clean Architecture:

- Domain
- Application
- Infrastructure
- API

## Messaging

Apache Pulsar is used for communication between services.

The Saga is choreographed:

Orders
-> Inventory
-> Payments
-> Orders

## Patterns

- Choreographed Saga
- Transactional Outbox
- Inbox / Deduplication
- CQRS
- Repository / Unit of Work
- Dead Letter Queue

## Database

Each service owns its own database.

- orderflow_orders
- orderflow_inventory
- orderflow_payments

## Stock consumption

After successful payment, Inventory permanently consumes the reserved stock.

## Ports

- Client: 5000
- Orders: 5001
- Inventory: 5002
- Payments: 5003
