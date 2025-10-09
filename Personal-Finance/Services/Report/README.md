# 🧾 Report Service

## 📘 Overview
**Report Service** is a microservice that asynchronously consumes messages from **RabbitMQ**, processes transaction data, and generates **PDF transaction reports** for users.  
Each generated report is automatically saved locally in the `reports/` directory and can be accessed or downloaded.

The service is built using **.NET 8**, integrates with **RabbitMQ** via **MassTransit**, and follows **Clean Architecture** principles.

---


### Layers
- **API** – Hosts MassTransit, configures consumers, and starts the service.  
- **Application** – Contains handlers and interfaces (e.g., `GenerateTransactionsReportHandler`).  
- **Domain** – Defines core entities (e.g., `Transaction`).  
- **Infrastructure** – Implements RabbitMQ consumers and PDF generation logic.  
- **EventBus.Messages** – Shared event definitions used for inter-service communication.

---

## 🚀 How It Works

1. The service listens to a RabbitMQ queue named **`transactionsreport-queue`**.  
2. It consumes messages of type `TransactionsReportEvent`.  
3. When a message is received, it extracts and processes the transaction list.  
4. A formatted **PDF report** is generated from this data.  
5. The generated file is saved in the local directory `/app/reports` (mapped to `./reports` on the host).

---


## 🧾 Example Message (JSON Payload)

To test the service manually using **RabbitMQ Management UI**:

1. Open the RabbitMQ dashboard at [http://localhost:15672](http://localhost:15672)  
   (username: `guest`, password: `guest`)
2. Go to **Queues → transactionsreport-queue → Publish message**
3. Paste the following JSON payload into the *Payload* field:

```json
{
  "EmailAddress": "john@example.com",
  "UserId": "1",
  "UserName": "John Doe",
  "TransactionItems": [
    {
      "Amount": 250.75,
      "WalletName": "Main Wallet",
      "TransactionType": 0,
      "Description": "Salary",
      "Date": "2025-10-08T10:00:00Z",
      "Currency": "USD",
      "CategoryName": "Income"
    },
    {
      "Amount": 50.00,
      "WalletName": "Main Wallet",
      "TransactionType": 1,
      "Description": "Groceries",
      "Date": "2025-10-08T18:00:00Z",
      "Currency": "USD",
      "CategoryName": "Food"
    }
  ]
}
```

4. Click Publish Message
 
5. The Report Service will consume the message, generate a PDF, and save it to:

`./reports/<unique-guid>.pdf`

You can then open the generated report directly from the `reports/` folder.


