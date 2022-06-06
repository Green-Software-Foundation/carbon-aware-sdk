using CarbonAware.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Diagnostics;

namespace CarbonAware.WebApi.Filters;

public class HttpResponseExceptionFilter : IExceptionFilter
{
    private ILogger<HttpResponseExceptionFilter> _logger;
    private static readonly ActivitySource _activitySource = new ActivitySource(nameof(HttpResponseExceptionFilter));

    private static Dictionary<string, int> EXCEPTION_STATUS_CODE_MAP = new Dictionary<string, int>()
    {
        { "ArgumentException", (int)HttpStatusCode.BadRequest },
        { "NotImplementedException", (int)HttpStatusCode.NotImplemented },
    };

    public HttpResponseExceptionFilter(ILogger<HttpResponseExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        using (var activity = _activitySource.StartActivity())
        {

            HttpValidationProblemDetails response;
            if (context.Exception is IHttpResponseException httpResponseException)
            {
                response = new HttpValidationProblemDetails(){
                    Title = httpResponseException.Title,
                    Status = httpResponseException.Status,
                    Detail = httpResponseException.Detail
                };
            } else {
                var exceptionType = context.Exception.GetType().Name;
                int statusCode;
                if (!EXCEPTION_STATUS_CODE_MAP.TryGetValue(exceptionType, out statusCode))
                {
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    activity?.SetStatus(ActivityStatusCode.Error, context.Exception.Message);
                    _logger.LogError(context.Exception, "500 Error: Unhandled exception");
                }
                
                response = new HttpValidationProblemDetails()
                {
                    Title = exceptionType,
                    Status = statusCode,
                    Detail = context.Exception.Message
                };
            }

            var traceId = activity?.Id;
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