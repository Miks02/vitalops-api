namespace WorkoutTrackerApi.Services.Results;

public sealed class Error
{
    public string Code { get; }
    public string Description { get; }

    private Error(string code, string description)
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
    
    public static class Auth
    {
        public static Error PasswordError(string message = "Error occurred while trying to assign password to the user")
            => new("Auth.InvalidCredentials", message);

        public static Error AccountLocked(string message = "Account is locked")
            => new("Auth.AccountLocked", message);
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
}