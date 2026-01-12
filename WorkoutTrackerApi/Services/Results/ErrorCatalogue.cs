namespace WorkoutTrackerApi.Services.Results;

public sealed class Error
{
    public string Code { get; }
    public string Description { get; }

    public Error(string code, string description)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be null or whitespace", nameof(code));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or whitespace", nameof(description));

        Code = code;
        Description = description;
    }
    
    public static class General
    {
        public static Error IdentityError(string message = "Error occurred while doing an identity operation")
            => new("General.IdentityError", message);
        
        public static Error NotFound(string message = "The requested resource was not found")
            => new("General.NotFound", message);

        public static Error InternalServerError(string message = "Internal server error")
            => new("General.InternalServerError", message);

        public static Error UnknownError(string message = "An unknown error occurred")
            => new("General.UnknownError", message);
    }
    
    public static class Resource
    {
        public static Error NotFound(string resourceName, string identifier = "")
        {
            string message = string.IsNullOrEmpty(identifier)
                ? $"{resourceName} was not found"
                : $"{resourceName} with identifier '{identifier}' was not found";

            return new Error("Resource.NotFound", message);
        }

        public static Error AlreadyExists(string resourceName, string identifier = "")
        {
            string message = string.IsNullOrEmpty(identifier)
                ? $"{resourceName} already exists"
                : $"{resourceName} with identifier '{identifier}' already exists";

            return new Error("Resource.AlreadyExists", message);
        }
    }

    public static class Validation
    {
        public static Error InvalidInput(string message = "The provided input is invalid")
            => new("Validation.InvalidInput", message);
        
        public static Error MissingRequiredField(string fieldName)
            => new("Validation.MissingRequiredField", $"The required field '{fieldName}' is missing");
        
        public static Error OutOfRange(string paramName, string message = "")
        {
            string fullMessage = string.IsNullOrWhiteSpace(message)
                ? $"The value for '{paramName}' is out of the allowed range"
                : message;
            return new Error("Validation.OutOfRange", fullMessage);
        }
    }
    
    public static class Auth
    {
        public static Error RegistrationFailed(string message = "Unexpected error happened during registration")
            => new("Auth.RegistrationFailed", message);
        
        public static Error LoginFailed(string message = "Unexpected error happened during login")
            => new("Auth.LoginFailed", message);
        
        public static Error PasswordError(string message = "Error occurred while trying to assign password to the user")
            => new("Auth.InvalidCredentials", message);

        public static Error AccountLocked(string message = "Account is locked")
            => new("Auth.AccountLocked", message);

        public static Error JwtError(string message = "Error happened while trying to assign refresh token to the user")
            => new("Auth.JwtError", message);

        public static Error ExpiredToken(string message = "Refresh token has expired")
            => new("Auth.ExpiredToken", message);
    }
    
    public static class User
    {
        public static Error EmailAlreadyExists(string email = "")
        {

            string message = string.IsNullOrWhiteSpace(email)
                ? "Email is taken"
                : $"Email '{email}' is taken";
            
            return new Error("User.EmailAlreadyExists", message);
        }

        public static Error UsernameAlreadyExists(string username = "")
        {
            string message = string.IsNullOrWhiteSpace(username)
                ? "Username is taken"
                : $"Email is {username}";

            return new Error("User.UsernameAlreadyExists", message);
        }

        public static Error NotFound(string identifier = "")
        {
            string message = string.IsNullOrWhiteSpace(identifier)
                ? "User not found"
                : $"User with identifier '{identifier}' is not found";

            return new Error("User.NotFound", message);
        }
    }
    public static class Database
    {
        public static Error ConnectionFailed(string message = "Failed to connect to the database")
            => new("Database.ConnectionFailed", message);

        public static Error OperationFailed(string message = "Database operation failed")
            => new("Database.OperationFailed", message);

        public static Error SaveChangesFailed(string message = "Failed to save changes to the database")
            => new("Database.SaveChangesFailed", message);

        public static Error ConcurrencyError(string message = "Concurrency conflict detected")
            => new("Database.ConcurrencyError", message);

        public static Error TransactionFailed(string message = "Database transaction failed")
            => new("Database.TransactionFailed", message);

        public static Error ConstraintViolation(string constraintName = "")
        {
            string message = string.IsNullOrWhiteSpace(constraintName)
                ? "Database constraint violation"
                : $"Database constraint '{constraintName}' violated";

            return new Error("Database.ConstraintViolation", message);
        }

        public static Error TimeoutError(string message = "Database operation timed out")
            => new("Database.TimeoutError", message);
    }
}