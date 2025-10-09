# 🧾 Budget.API — API & gRPC Documentation


## 🧱 Controllers and Endpoints

### 1. CategoriesController
**Route:** `/api/categories`

#### `GET /api/categories?userId={guid}`
Fetches all categories associated with a specific user.

**Query Parameters**
| Name | Type | Required | Description |
|------|------|-----------|-------------|
| `userId` | `Guid` | ✅ | Identifier of the user whose categories are retrieved. |

**Response**
- `200 OK` – Returns a list of category DTOs belonging to the user.
- `404 Not Found` – If no categories exist.

---

### 2. WalletsController
**Route:** `/api/wallets`

#### `POST /api/wallets`
Creates a new wallet for a user.

**Request Body**
```json
{
  "userId": "string (guid)",
  "name": "string",
  "currency": "string",
  "balance": 0.0
}
```

**Response**
- `200 OK` – Returns the created wallet entity.
- `400 Bad Request` – Validation error or missing fields.

---

### 3. TransactionController
**Route:** `/api/transaction`

#### `POST /api/transaction`
Creates a new transaction record.

**Request Body**
```json
{
  "walletId": "string (guid)",
  "amount": 120.5,
  "description": "Grocery shopping",
  "transactionType": "Expense",
  "categoryId": "string (guid)",
  "date": "2025-10-06T12:00:00Z"
}
```

**Response**
- `200 OK` – Transaction successfully created.
- `400 Bad Request` – Invalid or missing data.

---

### 4. RecurringTransactionsController
**Route:** `/api/recurringtransactions`

#### `POST /api/recurringtransactions`
Creates a new recurring transaction.

**Request Body**
```json
{
  "walletId": "string (guid)",
  "amount": 50,
  "transactionType": "Income",
  "frequency": "Monthly",
  "startDate": "2025-10-01T00:00:00Z",
  "description": "Salary"
}
```

**Response**
- `200 OK` – Returns created recurring transaction details.
- `400 Bad Request` – Invalid frequency or transaction type.

---

### 5. SpendingLimitsController
**Route:** `/api/spendinglimits`

#### `POST /api/spendinglimits`
Creates or updates a spending limit for a category or wallet.

**Request Body**
```json
{
  "userId": "string (guid)",
  "categoryId": "string (guid)",
  "limitAmount": 500.0,
  "currency": "USD"
}
```

**Response**
- `200 OK` – Returns the limit created or updated.
- `400 Bad Request` – Invalid input or missing data.

---

### 6. QueriesController
**Route:** `/api`

#### `GET /api/wallets/{walletId}/transactions`
Fetches all transactions belonging to a specific wallet.

**Path Parameters**
| Name | Type | Required | Description |
|------|------|-----------|-------------|
| `walletId` | `Guid` | ✅ | Identifier of the wallet. |

**Optional Query Parameters**
| Name | Type | Description |
|------|------|-------------|
| `from` | `DateTime` | Start date of the query range. |
| `to` | `DateTime` | End date of the query range. |
| `currency` | `string` | Currency filter. |

**Response**
- `200 OK` – List of transactions.
- `404 Not Found` – Wallet or transactions not found.

## 🧾 Example Request Flow
1. User creates a wallet → `/api/wallets`
2. Adds transactions → `/api/transaction`
3. Creates recurring transactions → `/api/recurringtransactions`
4. Sets spending limits → `/api/spendinglimits`
5. Queries transactions → `/api/wallets/{id}/transactions`
6. Views categorized spending → `/api/categories?userId={id}`

---

## ✅ Summary
| Controller | Methods | Description |
|-------------|----------|-------------|
| CategoriesController | `GET` | Retrieve user categories |
| WalletsController | `POST` | Create wallet |
| TransactionController | `POST` | Create transaction |
| RecurringTransactionsController | `POST` | Create recurring transaction |
| SpendingLimitsController | `POST` | Create spending limit |
| QueriesController | `GET` | Fetch wallet transactions |
