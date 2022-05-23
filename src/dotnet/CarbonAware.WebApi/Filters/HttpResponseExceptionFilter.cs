using CarbonAware.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Diagnostics;

namespace CarbonAware.WebApi.Filters;

public class HttpResponseExceptionFilter : IExceptionFilter
{
    private ILogger<HttpResponseExceptionFilter> _logger;
    private ActivitySource _activitySource;

    public HttpResponseExceptionFilter(ILogger<HttpResponseExceptionFilter> logger, ActivitySource activitySource)
    {
        _logger = logger;
        _activitySource = activitySource;
    }

    public void OnException(ExceptionContext context)
    {
        using (var activity = _activitySource.StartActivity(nameof(HttpResponseExceptionFilter)))
        {

            HttpValidationProblemDetails response;
            if (context.Exception is IHttpResponseException httpResponseException)
            {
                response = new HttpValidationProblemDetails(){
                    Title = httpResponseException.Title,
                    Status = httpResponseException.Status,
                    Detail = httpResponseException.Detail
                };
            } else if (context.Exception is ArgumentException argumentException) {
                response = new HttpValidationProblemDetails(){
                    Title = argumentException.GetType().Name,
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = argumentException.Message
                };
            } else {
                response = new HttpValidationProblemDetails(){
                    Title = context.Exception.GetType().Name,
                    Status = (int)HttpStatusCode.InternalServerError,
                    Detail = context.Exception.Message
                };
                _logger.LogError(context.Exception, "500 Error: Unhandled exception");
            }

            var traceId = Activity.Current?.Id;
            if (traceId != null)
            {
                response.Extensions["traceId"] = traceId;
            }

            context.Result = new ObjectResult(response)
            {
                StatusCode = response.Status
            };

            context.ExceptionHandled = true;
        }
    }
}