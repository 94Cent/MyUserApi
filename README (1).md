# MyUser API

MyUser API is a simple ASP.NET Core Web API for user registration, Login authentication and Product CRUD operations. 
This project demonstrates how to create a user registration and login system with email confirmation and JWT token-based authentication, also performing CRUD operations (GET all products, GET product by ID, POST 
new product, PUT update product, DELETE product) and other operations on an Api Endpoint

## Prerequisites

- .NET 6 SDK or later
- SQL Server or another compatible database
- Visual Studio or another C# IDE
- Git

## Getting Started

### Clone the Repository

```bash
git clone https://github.com/94cent/MyUserApi.git
cd MyUser.API
```
SETTING UP THE DATABASE
Open appsettings.json and update the ConnectionStrings section to point to your database.

"ConnectionStrings": {
  "DefaultConnection": "Server=your_server;Database=your_database;"
}
Apply the database migrations to create the necessary tables.

JWT AUTHENTICATION
Open appsettings.json for the Authentication section with your JWT settings.

RUNNING THE APPLICATION
Build and run the application
The Api should be available at  https://localhost:7063

###API ENDPOINTS

USER REGISTRATION
POST /api/auth/register
Registers a new user
Request body:
{
  "email": "user@example.com",
  "password": "YourPassword123!",
  "firstName": "John",
  "lastName": "Doe"
}

Response:
200 OK: User registered successfully. Please check your email to confirm your registration.
400 Bad Request: Validation error or email already exists.

USER LOGIN
POST /api/auth/login
Authenticates a user and returns a JWT token
Request Body:
{
  "email": "user@example.com",
  "password": "YourPassword123!"
}
Note: email and password must be registered first into database

Response:

200 OK: JWT token.
401 Unauthorized: Invalid credentials.

CRUD
/api/products
for crud operations 

Project Structure
Controllers: Contains the API controllers.
Data: Contains the ApplicationDbContext class for Entity Framework Core.
Models: Contains the data models and DTOs.
Services: Contains the email service implementation.
