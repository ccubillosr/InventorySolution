# Inventory Management System

## Overview
This project is an Inventory Management System that consists of a backend API built with ASP.NET Core and a frontend UI developed using Blazor. The system allows users to manage products, including creating, reading, updating, and deleting product entries.

## Project Structure
The solution is organized into three main projects:

1. **InventoryApi**: The backend API that handles all product-related operations.
   - **Controllers**: Contains the API controllers for handling HTTP requests.
   - **Data**: Manages the database context and migrations.
   - **Models**: Defines the product entity.
   - **DTOs**: Contains Data Transfer Objects for product data.
   - **Repositories**: Implements data access logic.
   - **Services**: Contains business logic for product management.
   - **Middleware**: Handles global exceptions.

2. **InventoryBlazorUI**: The frontend UI built with Blazor.
   - **Pages**: Contains Blazor components for product management.
   - **Shared**: Contains shared components like layout and navigation.
   - **Services**: Implements API client for making requests to the backend.
   - **Models**: Defines data models used in the UI.

3. **InventoryApi.Tests**: Contains unit tests for the API and services.

## Getting Started

### Prerequisites
- .NET SDK
- SQL Server

### Setup
1. Clone the repository:
   ```
   git clone <repository-url>
   cd InventorySolution
   ```

2. Create the SQL Server database:
   - Use the provided `appsettings.json` in the `InventoryApi` project to configure the connection string.
   - Run the migrations to set up the database schema.

3. Run the API:
   - Navigate to the `InventoryApi` project directory and run:
     ```
     dotnet run
     ```

4. Run the Blazor UI:
   - Navigate to the `InventoryBlazorUI` project directory and run:
     ```
     dotnet run
     ```

### API Endpoints
The API provides the following endpoints for managing products:
- `GET /api/products`: Retrieve a list of products.
- `GET /api/products/{id}`: Retrieve a product by ID.
- `POST /api/products`: Create a new product.
- `PUT /api/products/{id}`: Update an existing product.
- `DELETE /api/products/{id}`: Delete a product.

### Security
This project follows OWASP security guidelines to ensure secure coding practices across all layers of the application.

## Testing
Unit tests are provided in the `InventoryApi.Tests` project. To run the tests, navigate to the project directory and execute:
```
dotnet test
```

## Contributing
Contributions are welcome! Please submit a pull request or open an issue for any enhancements or bug fixes.

## License
This project is licensed under the MIT License. See the LICENSE file for more details.