CREATE TABLE IF NOT EXISTS receipts (
    id UUID PRIMARY KEY,
    store_name TEXT NULL,
    purchased_at TIMESTAMPTZ NULL,
    total NUMERIC NULL,
    status TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL
);

CREATE TABLE IF NOT EXISTS receipt_photos (
    id UUID PRIMARY KEY,
    receipt_id UUID NOT NULL REFERENCES receipts (id) ON DELETE CASCADE,
    storage_path TEXT NOT NULL,
    uploaded_at TIMESTAMPTZ NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_receipt_photos_receipt_id ON receipt_photos (receipt_id);

CREATE TABLE IF NOT EXISTS receipt_line_items (
    id UUID PRIMARY KEY,
    receipt_id UUID NOT NULL REFERENCES receipts (id) ON DELETE CASCADE,
    raw_text TEXT NOT NULL,
    description TEXT NOT NULL,
    category TEXT NOT NULL,
    quantity NUMERIC NOT NULL,
    unit_price NUMERIC NOT NULL,
    line_total NUMERIC NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_receipt_line_items_receipt_id ON receipt_line_items (receipt_id);
