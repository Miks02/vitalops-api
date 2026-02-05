using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MixxFit.API.Exceptions.Handlers
{
    public class CancellationExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<CancellationExceptionHandler> _logger;

        public CancellationExceptionHandler(ILogger<CancellationExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not OperationCanceledException operationCanceledException)
                return false;

            _logger.LogInformation("Request has been cancelled by the user");
            return true;

        }
    }
}
