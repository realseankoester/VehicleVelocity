# VehicleVelocity - GateKeeper Service

An event-driven auditing system that monitors vehicle intake telemetry via Kafka and performs automated quality assessments using a heuristic AI engine.

## 🚀 System Architecture
- **Producer:** Simulates vehicle intake data and publishes to Kafka.
- **Consumer (GateKeeper):** Processes event streams, performs multi-stage audits, and persists results.
- **Data Store:** PostgreSQL with Snake Case naming conventions.
- **Resilience:** Implements Polly retry policies for database persistence.
- **Observability:** Integrated with Grafana for real-time audit visualization.

## 🛠️ Key Features
- **Automated Risk Scoring:** Evaluates vehicles based on mileage, structural integrity, and cosmetic notes.
- **Dual-Phase Deployment:** - **Phase 1 (Shadow):** Logs AI insights without affecting the pipeline.
    - **Phase 2 (Assisted):** Flags high-priority vehicles for manual specialist review.
- **Persistence Logic:** Efficient "Upsert" logic using Entity Framework Core.
- **Structured Logging:** JSON-formatted logs via Serilog for audit trail tracking.

## 🚦 Getting Started

1. **Environment Setup:**
   - Ensure Docker is running with Kafka and PostgreSQL.
   - Configure `.env` with `DB_PASSWORD`.

2. **Database Migration:**
   ```bash
   cd VehicleVelocity.Common
   dotnet ef database update --startup-project ../VehicleVelocity.Consumer