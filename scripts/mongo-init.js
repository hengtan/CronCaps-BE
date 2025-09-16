// CronCaps MongoDB Initialization Script
// This script creates the initial MongoDB structure for logs and analytics

print('Starting CronCaps MongoDB initialization...');

// Switch to the CronCaps logs database
db = db.getSiblingDB('croncaps_logs');

// Create collections with validation schemas
db.createCollection('execution_logs', {
    validator: {
        $jsonSchema: {
            bsonType: 'object',
            title: 'Execution Log Schema',
            required: ['job_id', 'execution_id', 'status', 'started_at'],
            properties: {
                _id: {
                    bsonType: 'objectId'
                },
                job_id: {
                    bsonType: 'string',
                    description: 'UUID of the job from PostgreSQL'
                },
                execution_id: {
                    bsonType: 'string',
                    description: 'Unique execution identifier'
                },
                status: {
                    enum: ['pending', 'running', 'completed', 'failed', 'cancelled', 'timeout'],
                    description: 'Execution status'
                },
                started_at: {
                    bsonType: 'date',
                    description: 'When the execution started'
                },
                finished_at: {
                    bsonType: ['date', 'null'],
                    description: 'When the execution finished'
                },
                duration_ms: {
                    bsonType: ['long', 'null'],
                    minimum: 0,
                    description: 'Execution duration in milliseconds'
                },
                output: {
                    bsonType: ['string', 'null'],
                    description: 'Execution output/result'
                },
                error_message: {
                    bsonType: ['string', 'null'],
                    description: 'Error message if failed'
                },
                metadata: {
                    bsonType: 'object',
                    description: 'Additional metadata'
                }
            }
        }
    }
});

db.createCollection('system_metrics', {
    validator: {
        $jsonSchema: {
            bsonType: 'object',
            title: 'System Metrics Schema',
            required: ['timestamp', 'metric_type', 'value'],
            properties: {
                _id: {
                    bsonType: 'objectId'
                },
                timestamp: {
                    bsonType: 'date',
                    description: 'When the metric was recorded'
                },
                metric_type: {
                    enum: ['cpu_usage', 'memory_usage', 'disk_usage', 'active_jobs', 'queued_jobs', 'failed_jobs'],
                    description: 'Type of metric'
                },
                value: {
                    bsonType: 'double',
                    description: 'Metric value'
                },
                tags: {
                    bsonType: 'object',
                    description: 'Additional tags for the metric'
                }
            }
        }
    }
});

db.createCollection('audit_logs', {
    validator: {
        $jsonSchema: {
            bsonType: 'object',
            title: 'Audit Log Schema',
            required: ['timestamp', 'action', 'user_id', 'resource_type'],
            properties: {
                _id: {
                    bsonType: 'objectId'
                },
                timestamp: {
                    bsonType: 'date',
                    description: 'When the action occurred'
                },
                action: {
                    enum: ['create', 'read', 'update', 'delete', 'execute', 'pause', 'resume'],
                    description: 'Action performed'
                },
                user_id: {
                    bsonType: 'string',
                    description: 'UUID of the user who performed the action'
                },
                resource_type: {
                    enum: ['job', 'user', 'system', 'auth'],
                    description: 'Type of resource affected'
                },
                resource_id: {
                    bsonType: ['string', 'null'],
                    description: 'ID of the affected resource'
                },
                details: {
                    bsonType: 'object',
                    description: 'Additional details about the action'
                },
                ip_address: {
                    bsonType: ['string', 'null'],
                    description: 'IP address of the user'
                },
                user_agent: {
                    bsonType: ['string', 'null'],
                    description: 'User agent string'
                }
            }
        }
    }
});

// Create indexes for better performance
print('Creating indexes...');

// Execution logs indexes
db.execution_logs.createIndex({ 'job_id': 1, 'started_at': -1 });
db.execution_logs.createIndex({ 'status': 1, 'started_at': -1 });
db.execution_logs.createIndex({ 'execution_id': 1 }, { unique: true });
db.execution_logs.createIndex({ 'started_at': -1 });

// System metrics indexes
db.system_metrics.createIndex({ 'timestamp': -1, 'metric_type': 1 });
db.system_metrics.createIndex({ 'metric_type': 1, 'timestamp': -1 });

// Audit logs indexes
db.audit_logs.createIndex({ 'timestamp': -1 });
db.audit_logs.createIndex({ 'user_id': 1, 'timestamp': -1 });
db.audit_logs.createIndex({ 'resource_type': 1, 'resource_id': 1, 'timestamp': -1 });
db.audit_logs.createIndex({ 'action': 1, 'timestamp': -1 });

// Create TTL indexes for automatic cleanup (optional)
// Remove execution logs older than 90 days
db.execution_logs.createIndex({ 'started_at': 1 }, { expireAfterSeconds: 7776000 });

// Remove system metrics older than 30 days
db.system_metrics.createIndex({ 'timestamp': 1 }, { expireAfterSeconds: 2592000 });

// Remove audit logs older than 1 year
db.audit_logs.createIndex({ 'timestamp': 1 }, { expireAfterSeconds: 31536000 });

// Create user with read/write permissions
db.createUser({
    user: 'croncaps_app',
    pwd: 'croncaps_dev_password',
    roles: [
        {
            role: 'readWrite',
            db: 'croncaps_logs'
        }
    ]
});

// Insert sample data for development
print('Inserting sample data...');

// Sample execution log
db.execution_logs.insertOne({
    job_id: 'sample-job-id-123',
    execution_id: 'exec-' + new Date().getTime(),
    status: 'completed',
    started_at: new Date(Date.now() - 60000),
    finished_at: new Date(),
    duration_ms: 60000,
    output: 'Job completed successfully',
    error_message: null,
    metadata: {
        server: 'dev-server',
        version: '1.0.0'
    }
});

// Sample system metric
db.system_metrics.insertOne({
    timestamp: new Date(),
    metric_type: 'active_jobs',
    value: 5,
    tags: {
        server: 'dev-server',
        environment: 'development'
    }
});

// Sample audit log
db.audit_logs.insertOne({
    timestamp: new Date(),
    action: 'create',
    user_id: 'sample-user-id-123',
    resource_type: 'job',
    resource_id: 'sample-job-id-123',
    details: {
        job_name: 'Sample Job',
        cron_expression: '0 */5 * * * *'
    },
    ip_address: '127.0.0.1',
    user_agent: 'CronCaps/1.0.0'
});

print('CronCaps MongoDB initialization completed successfully!');
print('Collections created: execution_logs, system_metrics, audit_logs');
print('Indexes and sample data inserted.');