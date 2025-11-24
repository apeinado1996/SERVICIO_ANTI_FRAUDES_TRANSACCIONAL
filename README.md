# Transactional Anti-Fraud (.NET 8, SQL Server, Kafka, Docker)

## 📁 Estructura del Proyecto

```text
TRANSACTIONAL_ANTIFRAUD/
│  docker-compose.yaml
│  Dockerfile                 # Tests de Transaction (transaction-tests)
│  DockerfileAntifraud        # Tests de Antifraud (antifraud-tests)
│
├─Transaction_Service/
│  ├─Transaction.Api/
│  ├─Transaction.Core/
│  ├─Transaction.Infrastructure/
│  └─Transaction.Tests/
│
└─Antifraud_Service/
   ├─Antifraud.Api/
   ├─Antifraud.Core/
   ├─Antifraud.Infrastructure/
   └─Antifraud.Tests/
```

# Pasos para la instalación

- **Construir las imágenes:** `docker compose build`
- **Levantar los contenedores:** `docker compose up -d`
- **Kafka: Crear Tópicos Utilizados:** `docker exec -it kafka bash`
- **Crear el tópico transactions:**  `kafka-topics --bootstrap-server kafka:9092 --create --topic transactions --partitions 1 --replication-factor 1`
- **Crear el tópico transactions-validated:** `kafka-topics --bootstrap-server kafka:9092 --create --topic transactions-validated --partitions 1 --replication-factor 1`
- **Verificar que los tópicos existen:** `kafka-topics --bootstrap-server kafka:9092 --list`
- **Base de Datos SQL Server**
- **Ejecutar tests de Transaction:** `docker compose run --rm transaction-tests`
- **Ejecutar tests de Antifraud:** `docker compose run --rm antifraud-tests`

# Se envía Colección de Postman para ejecutar los endpoints del microservicio de Transactions
