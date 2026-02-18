using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Seems.Api.Filters;

public class ApiExceptionFilter(ILogger<ApiExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case ValidationException validationException:
                var errors = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                context.Result = new BadRequestObjectResult(new
                {
                    status = 400,
                    message = "Validation failed.",
                    errors,
                });
                context.ExceptionHandled = true;
                break;

            case UnauthorizedAccessException:
                context.Result = new UnauthorizedObjectResult(new
                {
                    status = 401,
                    message = context.Exception.Message,
                });
                context.ExceptionHandled = true;
                break;

            case KeyNotFoundException:
                context.Result = new NotFoundObjectResult(new
                {
                    status = 404,
                    message = context.Exception.Message,
                });
                context.ExceptionHandled = true;
                break;

            default:
                logger.LogError(context.Exception, "Unhandled exception");
                context.Result = new ObjectResult(new
                {
                    status = 500,
                    message = "An unexpected error occurred.",
                })
                {
                    StatusCode = 500,
                };
                context.ExceptionHandled = true;
                break;
        }
    }
}
