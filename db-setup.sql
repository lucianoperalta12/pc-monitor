-- ============================================================
-- PC Monitor V1 — Setup de base de datos
-- Ejecutar en el VPS como superusuario de PostgreSQL
-- ============================================================

-- 1. Crear base de datos (si no existe)
-- Ejecutar esta línea por separado conectado a la base "postgres":
-- CREATE DATABASE pcmonitor;

-- 2. Conectarse a la base pcmonitor y ejecutar el resto:
-- \c pcmonitor

-- 3. Crear schema
CREATE SCHEMA IF NOT EXISTS pc_monitor;

-- 4. Tabla machines
CREATE TABLE IF NOT EXISTS pc_monitor.machines (
    id           SERIAL PRIMARY KEY,
    machine_id   VARCHAR(100) NOT NULL,
    name         VARCHAR(200),
    last_seen_at TIMESTAMPTZ  NOT NULL,
    CONSTRAINT uq_machines_machine_id UNIQUE (machine_id)
);

-- 5. Tabla sessions
CREATE TABLE IF NOT EXISTS pc_monitor.sessions (
    id          SERIAL PRIMARY KEY,
    machine_id  VARCHAR(100) NOT NULL,
    started_at  TIMESTAMPTZ  NOT NULL,
    ended_at    TIMESTAMPTZ  NULL    -- NULL = sesión abierta
);

CREATE INDEX IF NOT EXISTS idx_sessions_machine_open
    ON pc_monitor.sessions (machine_id, ended_at);

-- 6. (Opcional) Usuario dedicado con permisos mínimos
-- CREATE USER pcmonitor_app WITH PASSWORD 'CAMBIAR_PASSWORD';
-- GRANT USAGE ON SCHEMA pc_monitor TO pcmonitor_app;
-- GRANT SELECT, INSERT, UPDATE ON pc_monitor.machines TO pcmonitor_app;
-- GRANT SELECT, INSERT, UPDATE ON pc_monitor.sessions TO pcmonitor_app;
-- GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA pc_monitor TO pcmonitor_app;
