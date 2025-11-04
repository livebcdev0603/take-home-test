## Running the Backend

### Prerequisites
- .NET 6.0 SDK
- SQL Server (or use Docker Compose)

### Using Docker Compose (Recommended)

Navigate to the `src` folder and run:
```sh
docker-compose up -d
```

This will start:
- SQL Server on port 1433
- API on http://localhost:5000

### Manual Setup

1. Update the connection string in `Startup.cs` or set it via environment variable:
   ```
   ConnectionStrings__DefaultConnection=Server=localhost;Database=LoanManagementDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;
   ```

2. Build the backend:
   ```sh
   dotnet build
   ```

3. Run all tests:
   ```sh
   dotnet test
   ```

4. Start the API:
   ```sh
   cd Fundo.Applications.WebApi
   dotnet run
   ```

The API will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001

### API Endpoints

- `GET /loans` - List all loans
- `GET /loans/{id}` - Get loan by ID
- `POST /loans` - Create a new loan
- `POST /loans/{id}/payment` - Make a payment on a loan

### Database

The database will be automatically created on first run with seed data containing 3 sample loans.

## Notes  

Feel free to modify the code as needed, but try to **respect and extend the current architecture**, as this is intended to be a replica of the Fundo codebase.
