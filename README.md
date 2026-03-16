Vehicle Velocity - GateKeeper & Inventory Ecosystem
An end-to-end, event-driven auditing and discovery system. This ecosystem monitors vehicle intake via Kafka, performs heuristic quality assessments (GateKeeper), and exposes a high-performance query layer via GraphQL (Inventory API).

🚀 System Architecture
Producer: Simulates real-time vehicle intake telemetry.

Consumer (GateKeeper): Processes event streams, performs multi-stage heuristic audits, and persists results.

Inventory API: A high-concurrency GraphQL gateway for data discovery and filtering.

Data Store: PostgreSQL with Snake Case naming conventions and EF Core.

Resilience: Implements Polly retry policies for robust database persistence and IDbContextFactory for thread-safe GraphQL execution.

Observability: Integrated with Grafana for real-time audit visualization and Serilog for structured JSON logging.

🛠️ Key Features
Dual-Phase Deployment Strategy:

Phase 1 (Shadow): Logs insights without affecting the pipeline to validate heuristic accuracy.

Phase 2 (Assisted): Flags high-priority vehicles for manual specialist review based on risk scores.

Automated Risk Scoring: Evaluates vehicles based on mileage, structural integrity, and cosmetic notes using 5+ specialized unit tests.

Advanced Discovery (GraphQL): Implements server-side filtering and sorting, allowing clients to fetch optimized payloads (e.g., VIN-only discovery) to prevent over-fetching.

Persistence Logic: High-efficiency "Upsert" logic via Entity Framework Core.

🧪 Quality & Testing
The core audit engine is protected by a suite of xUnit tests that validate:

Mileage threshold logic and risk-weighting algorithms.

Sentiment analysis on vehicle history notes.

Safe initialization of the DbContext across different project lifecycles.

🚦 Getting Started
Infrastructure: Run docker-compose up (Kafka & Postgres).

Environment: Configure .env with DB_PASSWORD.

Run Inventory API: cd InventoryAPI && dotnet run

Query: Access Banana Cake Pop at /graphql to explore the inventory.