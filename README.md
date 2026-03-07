# VehicleVelocity: AI-Driven Inventory Audit Pipeline

A high-performance, real-time microservices architecture designed to automate vehicle intake and quality auditing. Built with scalability and AI-integration at its core.

## 🚀 Key Features
* **Real-Time Streaming:** Leverages **Apache Kafka** to decouple vehicle intake (Producer) from audit processing (Consumer).
* **AI Vision Integration:** Features an extensible `IImageAnalysisService` that flags structural damage (Rust, Dents, Cracks) using image metadata.
* **Fault Tolerance:** Implemented **Polly Retry Policies** with exponential backoff to handle transient database contention.
* **Cloud-Native Stack:** Developed using .NET 8, PostgreSQL, and Docker Compose for seamless environment parity.

## 🛠 Tech Stack
* **Language:** C# / .NET 8
* **Message Broker:** Confluent Kafka
* **Database:** PostgreSQL (via Entity Framework Core)
* **Infrastructure:** Docker & Docker Compose
* **Resiliency:** Polly (Retry/Jitter)

## 🏗 System Architecture
1. **Producer:** Simulates vehicle intake and publishes JSON events to the `inventory-updates` topic.
2. **Kafka:** Acts as the distributed log, ensuring message persistence.
3. **Consumer (The Gatekeeper):** Subscribes to updates, runs a Dual-Audit (Text Logic + AI Vision), and persists results to PostgreSQL.