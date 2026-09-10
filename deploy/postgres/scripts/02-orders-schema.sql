-- =============================================================================
-- Database: orderflow_orders (Orders Microservice)
-- =============================================================================
\c orderflow_orders;

CREATE TABLE IF NOT EXISTS orders (
    id UUID PRIMARY KEY,
    customer_id VARCHAR(100) NOT NULL,
    total_amount NUMERIC(10,2) NOT NULL,
    status TEXT NOT NULL CHECK (status IN ('Pending','Reserving','Charging','Confirmed','Cancelled')),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS order_lines (
    id BIGSERIAL PRIMARY KEY,
    order_id UUID NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    sku VARCHAR(50) NOT NULL,
    quantity INT NOT NULL,
    unit_price NUMERIC(10,2) NOT NULL
);

CREATE TABLE IF NOT EXISTS order_saga_state (
    order_id UUID PRIMARY KEY REFERENCES orders(id) ON DELETE CASCADE,
    reservation_completed BOOLEAN NOT NULL DEFAULT FALSE,
    payment_completed BOOLEAN NOT NULL DEFAULT FALSE,
    last_processed_event_id UUID NULL
);

CREATE TABLE IF NOT EXISTS outbox_messages (
    id BIGSERIAL PRIMARY KEY,
    event_id UUID NOT NULL UNIQUE,
    topic VARCHAR(255) NOT NULL,
    payload JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    published_at TIMESTAMPTZ NULL
);

CREATE TABLE IF NOT EXISTS inbox_messages (
    event_id UUID PRIMARY KEY,
    processed_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
