using CarbonAware.Interfaces;
using System;

namespace CarbonAware.DataSources.ElectricityMapsFree.Client;

internal class ElectricityMapsFreeClientHttpException : Exception, IHttpResponseException
{
    /// <summary>
    /// Creates a new instance of the <see cref="ElectricityMapsFreeClientHttpException"/> class.
    /// </summary>
    /// <param name="message">The error message supplied.</param>
    /// <param name="response">The response object generating this exception.</param>
    public ElectricityMapsFreeClientHttpException(string message, HttpResponseMessage response) : base(message)
    {
        this.Response = response;
        this.Status = (int)response.StatusCode;
        this.Title = nameof(ElectricityMapsFreeClientHttpException);
        this.Detail = message;
    }

    /// <summary>
    /// Gets the status code for the exception.  See remarks for the status codes that can be returned.
    /// </summary>
    /// <remarks>
    /// 400:  Returned when missing arguments (no country code passed or lat/lon don't map to a known country code)
    /// 401:  Returned when trying to access a path or location that isn't authorized for the token.
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
    /// Gets the response returned from the Electricity Maps Free call.
    /// </summary>
    public HttpResponseMessage? Response { get; }
}
