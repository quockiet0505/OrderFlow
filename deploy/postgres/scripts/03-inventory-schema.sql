-- =============================================================================
-- Database: orderflow_inventory (Inventory Microservice)
-- =============================================================================
\c orderflow_inventory;

CREATE TABLE IF NOT EXISTS stock_items (
    sku VARCHAR(50) PRIMARY KEY,
    quantity_on_hand INT NOT NULL,
    quantity_reserved INT NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS reservations (
    id BIGSERIAL PRIMARY KEY,
    order_id UUID NOT NULL,
    sku VARCHAR(50) NOT NULL,
    quantity INT NOT NULL,
    status TEXT NOT NULL CHECK (status IN ('Active','Released','Consumed')),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT unique_order_sku UNIQUE (order_id, sku)
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

-- Seed stock items
INSERT INTO stock_items (sku, quantity_on_hand, quantity_reserved)
VALUES
    ('WIDGET-01', 10, 0),
    ('WIDGET-02', 5, 0),
    ('WIDGET-03', 20, 0)
ON CONFLICT (sku) DO NOTHING;
