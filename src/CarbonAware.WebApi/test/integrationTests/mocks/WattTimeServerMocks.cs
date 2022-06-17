using System.Net;
using System.Text.Json;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using System.Net.Mime;
using CarbonAware.Tools.WattTimeClient.Model;
using CarbonAware.Tools.WattTimeClient.Constants;

namespace CarbonAware.Tools.WattTimeClient
{
    /// <summary>
    /// A utilities static class for Watt Time
    /// </summary>
    public static class WattTimeServerMocks
    {
        private static readonly DateTimeOffset testDataPointOffset = new(2022, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly string testBA = "TEST_BA";
        private static readonly GridEmissionDataPoint defaultDataPoint = new()
        {
            BalancingAuthorityAbbreviation = testBA,
            Datatype = "dt",
            Frequency = 300,
            Market = "mkt",
            PointTime = testDataPointOffset,
            Value = 999.99F,
            Version = "1.0"
        };

        private static readonly List<GridEmissionDataPoint> defaultDataList = new() { defaultDataPoint };

        private static readonly List<Forecast> defaultForecastList = new()
        {
            new Forecast()
            {
                GeneratedAt = testDataPointOffset,
                ForecastData = new List<GridEmissionDataPoint>()
                    {
                        new GridEmissionDataPoint()
                        {
                            BalancingAuthorityAbbreviation = testBA,
                            PointTime = testDataPointOffset,
                            Value = 999.99F,
                            Version = "1.0"
                        }
                    }
            }
        };

        private static readonly BalancingAuthority defaultBalancingAuthority = new()
        {
            Id = 12345,
            Abbreviation = testBA,
            Name = "Test Balancing Authority"
        };

        private static readonly LoginResult defaultLoginResult = new() { Token = "myDefaultToken123" };

        /// <summary>
        /// Getter for the default data point used in the 
        /// </summary>
        public static GridEmissionDataPoint GetDefaultEmissionsDataPoint() => new()
        {
            BalancingAuthorityAbbreviation = defaultDataPoint.BalancingAuthorityAbbreviation,
            Datatype = defaultDataPoint.Datatype,
            Frequency = defaultDataPoint.Frequency,
            Market = defaultDataPoint.Market,
            PointTime = defaultDataPoint.PointTime,
            Value = defaultDataPoint.Value,
            Version = defaultDataPoint.Version
        };

        /// <summary>
        /// Setup the mock server for watttime calls
        /// </summary>
        public static void WattTimeServerSetupMocks(this WireMockServer server, List<GridEmissionDataPoint>? dataMock = null, List<Forecast>? forecastMock = null, BalancingAuthority? baMock = null, LoginResult? loginMock = null)
        {
            SetupBaMock(server, baMock);
            SetupLoginMock(server, loginMock);
        }

        /// <summary>
        /// Helper function for setting up server response given a get request.
        /// </summary>
        /// <param name="server">Wire mock server to add mock to.</param>
        /// <param name="path">String path server should respond to.</param>
        /// <param name="statusCode">Status code server should respond with.</param>
        /// <param name="contentType">Content type server should return.</param>
        /// <param name="body">Response body from the request.</param>
        private static void SetupResponseGivenGetRequest(this WireMockServer server, string path, string body, HttpStatusCode statusCode = HttpStatusCode.OK, string contentType = MediaTypeNames.Application.Json)
        {
            server
                .Given(Request.Create().WithPath("/" + path).UsingGet())
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(statusCode)
                        .WithHeader("Content-Type", contentType)
                        .WithBody(body)
            );
        }

        /// <summary>
        /// Setup data calls on mock server
        /// </summary>
        /// <param name="server">Wire mock server to setup for data path. </param>
        /// <param name="content"> [Optional] List of grid emissions data points to return in the mock. </param>
        /// <remarks> If no content is passed, server mocks a static datapoint. </remarks>
        public static void SetupDataMock(this WireMockServer server, List<GridEmissionDataPoint>? content = null)
        {
            server.SetupResponseGivenGetRequest(Paths.Data, JsonSerializer.Serialize(content ?? defaultDataList));
        }

        /// <summary>
        /// Setup forecast calls on mock server
        /// </summary>
        /// <param name="server">Wire mock server to setup for forecast path. </param>
        /// <param name="content"> [Optional] List of forecasts to return in the mock. </param>
        /// <remarks> If no content is passed, server mocks a static forecast list with a single forecast. </remarks>
        public static void SetupForecastMock(this WireMockServer server, List<Forecast>? content = null) =>
            server.SetupResponseGivenGetRequest(Paths.Forecast, JsonSerializer.Serialize(content ?? defaultForecastList));

        /// <summary>
        /// Setup balancing authority calls on mock server
        /// </summary>
        /// <param name="server">Wire mock server to setup for ba path. </param>
        /// <param name="content"> [Optional] List of forecasts to return in the mock. </param>
        /// <remarks> If no content is passed, server mocks a static balancing authority. </remarks>
        private static void SetupBaMock(this WireMockServer server, BalancingAuthority? content = null) =>
            server.SetupResponseGivenGetRequest(Paths.BalancingAuthorityFromLocation, JsonSerializer.Serialize(content ?? defaultBalancingAuthority));

        /// <summary>
        /// Setup logins calls on mock server
        /// </summary>
        /// <param name="server">Wire mock server to setup for login path. </param>
        /// <param name="content"> [Optional] List of forecasts to return in the mock. </param>
        /// <remarks> If no content is passed, server mocks a static login result. </remarks>
        private static void SetupLoginMock(this WireMockServer server, LoginResult? content = null) =>
            server.SetupResponseGivenGetRequest(Paths.Login, JsonSerializer.Serialize(content ?? defaultLoginResult));
    }
}
