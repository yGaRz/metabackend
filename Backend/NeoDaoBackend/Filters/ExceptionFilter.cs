using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace NeoDaoBackend.Filters;

public class ExceptionFilter : IExceptionFilter {
    private readonly ILogger _logger;

    public ExceptionFilter(ILogger<ExceptionFilter> logger) {
        _logger = logger;
    }

    public void OnException(ExceptionContext context) {
        _logger.LogError(context.Exception, "Uncaught exception detected");
        context.Result = new ContentResult {
            Content = null,
            StatusCode = (int) HttpStatusCode.InternalServerError
        };
    }
}
