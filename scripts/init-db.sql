-- CronCaps PostgreSQL Initialization Script
-- This script creates the initial database structure

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_stat_statements";

-- Create schemas
CREATE SCHEMA IF NOT EXISTS croncaps;
CREATE SCHEMA IF NOT EXISTS hangfire;
CREATE SCHEMA IF NOT EXISTS audit;

-- Set search path
ALTER DATABASE croncaps SET search_path = croncaps, public;

-- Grant permissions
GRANT ALL PRIVILEGES ON DATABASE croncaps TO croncaps_user;
GRANT ALL PRIVILEGES ON SCHEMA croncaps TO croncaps_user;
GRANT ALL PRIVILEGES ON SCHEMA hangfire TO croncaps_user;
GRANT ALL PRIVILEGES ON SCHEMA audit TO croncaps_user;

-- Create custom types
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'job_status') THEN
        CREATE TYPE croncaps.job_status AS ENUM (
            'active',
            'inactive',
            'paused',
            'error',
            'deleted'
        );
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'execution_status') THEN
        CREATE TYPE croncaps.execution_status AS ENUM (
            'pending',
            'running',
            'completed',
            'failed',
            'cancelled',
            'timeout'
        );
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'user_role') THEN
        CREATE TYPE croncaps.user_role AS ENUM (
            'admin',
            'manager',
            'user',
            'viewer'
        );
    END IF;
END$$;

-- Create audit function
CREATE OR REPLACE FUNCTION audit.audit_function() RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        NEW.created_at = NOW();
        NEW.updated_at = NOW();
        RETURN NEW;
    ELSIF TG_OP = 'UPDATE' THEN
        NEW.updated_at = NOW();
        NEW.version = OLD.version + 1;
        RETURN NEW;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Create audit trigger helper
CREATE OR REPLACE FUNCTION audit.create_audit_trigger(table_name text, schema_name text DEFAULT 'croncaps')
RETURNS void AS $$
BEGIN
    EXECUTE format('CREATE TRIGGER audit_trigger_%I_%I
                    BEFORE INSERT OR UPDATE ON %I.%I
                    FOR EACH ROW EXECUTE FUNCTION audit.audit_function();',
                    schema_name, table_name, schema_name, table_name);
END;
$$ LANGUAGE plpgsql;

-- Log initialization
DO $$
BEGIN
    RAISE NOTICE 'CronCaps database initialized successfully at %', NOW();
END$$;