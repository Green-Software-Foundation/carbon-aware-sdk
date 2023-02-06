using CarbonAware.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Collections.Generic;
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
                            Detail = context.Exception.Message
                };
                if (isVerboseApi) {
                    response.Errors["stackTrace"] = new string[] { context.Exception.StackTrace! };
                }
            }
        }

        var traceId = activity?.Id;
        if (traceId != null)
        {
            response.Extensions["traceId"] = traceId;
        }

        foreach (DictionaryEntry entry in context.Exception.Data)
        {
            if (entry.Value is string[] messages && entry.Key is string key){
                response.Errors[key] = messages;
            }
        }

        context.Result = new ObjectResult(response)
        {
            StatusCode = response.Status
        };
        _logger.LogError(context.Exception, "Exception: {exception}", context.Exception.Message);
        context.ExceptionHandled = true;
    }
}
