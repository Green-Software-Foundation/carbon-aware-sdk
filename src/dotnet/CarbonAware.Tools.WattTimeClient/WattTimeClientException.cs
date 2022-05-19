using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.Tools.WattTimeClient
{
    public class WattTimeClientException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="WattTimeClientException"/> class.
        /// </summary>
        /// <param name="message">The error message supplied.</param>
        /// <param name="response">The response object generating this exception.</param>
        public WattTimeClientException(string message, HttpResponseMessage? response) : base(message)
        {
            this.Response = response;
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
        public HttpStatusCode? HttpStatusCode => this.Response?.StatusCode;

        /// <summary>
        /// Gets the amount of time to wait before tryiung a new request, if the error is retryable.
        /// </summary>
        public RetryConditionHeaderValue? RetryAfter => this.Response?.Headers.RetryAfter;

        /// <summary>
        /// Gets the response returned from the WattTime call.
        /// </summary>
        public HttpResponseMessage? Response { get; }

        /// <summary>
        /// Gets the content returned from the WattTime call.
        /// </summary>
        public string? Content => this.Response?.Content.ToString();
        
    }
}
