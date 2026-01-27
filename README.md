# VitalOps API

VitalOps API is a backend Web API written in C# targeting .NET 10.  
The project is organized as a single project inside a single solution and follows a horizontal folder structure (Controllers, Services, Validators, Models, DTOs, etc.).

## Summary
Minimal, service-oriented Web API that keeps controllers thin and places business logic in services. Validation is handled by a dedicated validators layer and DTOs are used for external communication.

## Folder structure (horizontal)
- Controllers — HTTP endpoints, routing and request/response mapping.
- Services — business logic and orchestration.
- Validators — input validation for DTOs using FluentValidation.
- Models — domain entities and internal types.
- DTOs — request/response objects exposed to clients.
- Configurations — DI and application setup helpers.
- Extension - for extensions methods like ResultExtensions that can be seen in the repo, etc...
- Exceptions - for exception handlers
- Mappers - Custom model/dto mappers (i don't like mapping libraries so i make mappers manually)
- Data - Persistence layer, db context and configuration classes are here
- Filters - API Filters like ProblemDetailsFilter that transforms the request when an error happens and returns a ProblemDetails to the client

Keep controllers thin: map and validate input, call services, return appropriate HTTP responses.

## Result pattern / error handling
This project uses a result pattern for handling business logic as opposed to throwing exceptions everywhere,
exceptions are reserved only for "exceptional stuff" like database errors, network errors and others.
Business errors are handled using the Result and Result<T> classes along with the ErrorCatalogue class

## Authentication / Authorization
This project uses custom authentication based on JWT access tokens plus refresh tokens.

## Configuration (example)
Minimal JWT settings in appsettings.json:
```json
{
  "Jwt": {
    "Issuer": "VitalOps",
    "Audience": "VitalOpsClients",
    "Secret": "REPLACE_WITH_A_STRONG_SECRET",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 30
  }
}
```

## Logging and Api responses
- This project uses ILogger<T> interface for structured logging inside every service class.
- Error return type is standardized across the API using the ProblemDetails class that provides a standardized error message

## Validation and DTOs
- Validate incoming DTOs in the Validators layer before calling Services.
- Map between DTOs and Models in a single place (controller or mapping helper) to keep Services model-focused.
