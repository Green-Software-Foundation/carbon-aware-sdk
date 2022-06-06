using CarbonAware.Interfaces;
using System;

namespace CarbonAware.Tools.WattTimeClient;

public class WattTimeClientHttpException : Exception, IHttpResponseException
{
    /// <summary>
    /// Creates a new instance of the <see cref="WattTimeClientHttpException"/> class.
    /// </summary>
    /// <param name="message">The error message supplied.</param>
    /// <param name="response">The response object generating this exception.</param>
    public WattTimeClientHttpException(string message, HttpResponseMessage response) : base(message)
    {
        this.Response = response;
        this.Status = (int)response.StatusCode;
        this.Title = nameof(WattTimeClientHttpException);
        this.Detail = message;
    }

    /// <summary>
    /// Gets the status code for the exception.  See remarks for the status codes that can be returned.
    /// </summary>
    /// <remarks>
    /// 400:  Returned when the lattitude/longitude provided aren't associated with a known balancing authority.
    /// 401:  Returned when no authorization header is passed.  You should not expect to receive this status code.
    /// 403:  Returned when an invalid username or password is used for login.  Please check your configuration and verify your account when this error is received.
    /// 429:  Returned when the number of requests has exceeded the WattTime rate limit, currently at 3,000 per rolling 5 minute window.  For current limits, see https://www.watttime.org/api-documentation/#restrictions
    /// </remarks>
    public int? Status { get; }

    /// <summary>
    /// A short, human-readable summary of the problem type. It SHOULD NOT change from occurrence to occurrence
    /// of the problem, except for purposes of localization(e.g., using proactive content negotiation;
    /// see[RFC7231], Section 3.4).
    /// </summary>
    public string? Title { get; }

    /// <summary>
    /// A human-readable explanation specific to this occurrence of the problem.
    /// </summary>
    public string? Detail { get; }

    /// <summary>
    /// Gets the response returned from the WattTime call.
    /// </summary>
    public HttpResponseMessage? Response { get; }
}
