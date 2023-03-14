using CarbonAware.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Diagnostics;
using System.Net;

namespace CarbonAware.WebApi.Filters;

public class HttpResponseExceptionFilter : IExceptionFilter
{
    private ILogger<HttpResponseExceptionFilter> _logger;
    private IOptionsMonitor<CarbonAwareVariablesConfiguration> _options;

    private static Dictionary<string, int> EXCEPTION_STATUS_CODE_MAP = new Dictionary<string, int>()
    {
        { "ArgumentException", (int)HttpStatusCode.BadRequest },
        { "NotImplementedException", (int)HttpStatusCode.NotImplemented },
        { "InvalidOperationException", (int)HttpStatusCode.BadRequest },
    };

    public HttpResponseExceptionFilter(ILogger<HttpResponseExceptionFilter> logger, IOptionsMonitor<CarbonAwareVariablesConfiguration> options)
    {
        _logger = logger;
        _options = options;
    }

    public void OnException(ExceptionContext context)
    {
        var activity = Activity.Current;

        HttpValidationProblemDetails response;
        var contextException = GetRelevantException(context);
        if (contextException is IHttpResponseException httpResponseException)
        {
            response = new HttpValidationProblemDetails(){
                Title = httpResponseException.Title,
                Status = httpResponseException.Status,
                Detail = httpResponseException.Detail
            };
        } else {
            var exceptionType = contextException.GetType().Name;
            int statusCode;
            if (!EXCEPTION_STATUS_CODE_MAP.TryGetValue(exceptionType, out statusCode))
            {
                statusCode = (int)HttpStatusCode.InternalServerError;
                activity?.SetStatus(ActivityStatusCode.Error, contextException.Message);
            }
            var isVerboseApi = _options.CurrentValue.VerboseApi;
       
            if (statusCode == (int)HttpStatusCode.InternalServerError && !isVerboseApi)
            {
                 response = new HttpValidationProblemDetails() {
                                Title = HttpStatusCode.InternalServerError.ToString(),
                                Status = statusCode,
                    };
            }
            else
            {
                response = new HttpValidationProblemDetails() {
                            Title = exceptionType,
                            Status = statusCode,
                            Detail = contextException.Message
                };
                if (isVerboseApi) {
                    response.Errors["stackTrace"] = new string[] { contextException.StackTrace! };
                }
            }
        }

        var traceId = activity?.Id;
        if (traceId != null)
        {
            response.Extensions["traceId"] = traceId;
        }

        foreach (DictionaryEntry entry in contextException.Data)
        {
            if (entry.Value is string[] messages && entry.Key is string key){
                response.Errors[key] = messages;
            }
        }

        context.Result = new ObjectResult(response)
        {
            StatusCode = response.Status
        };
        _logger.LogError(contextException, "Exception: {exception}", contextException.Message);
        context.ExceptionHandled = true;
    }

    private static Exception GetRelevantException(ExceptionContext context)
    {
        // Give priority to the inner exception since it contains the exception root cause.
        if (context.Exception.InnerException is not null)
        {
            return context.Exception.InnerException;
        }
        return context.Exception;
    }
}
