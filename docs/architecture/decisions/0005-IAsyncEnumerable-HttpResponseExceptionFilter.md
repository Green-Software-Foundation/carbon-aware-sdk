# 5. WebAPI to use IAsyncEnumerable to stream potentially large responses.

## Status
Denied

## Context
There are currently two endpoints (`emissions/forecasts/batch`, `emissions/average-carbon-intensity/batch`) that require passing a payload as an array of items. This array can have as many items as the user wants, and it can take some time to be processed which creates a delay to the client, which might feel that the service is irresponsive. Also, since these requests return enumerable items, and those are buffered before the client gets it, it impacts the overall memory footprint of the WebApp. Changing the signature of these endpoints to return an `IAsyncEnumerable` collection streams the response and helps to deal with these memory concerns.

## Decision
- Using IAsyncEnumerable return type for a controller helps to stream large content to a client when the request is large. Allowing the client to get a flow of continue content without the need to wait until the entire request is processed. (for instance forecast batch with 50 entries)
- Low memory overhead of the container: Processing large requests and not buffering the response by the controller helps to manage this. Using `IActionResult` results in buffering the response, hence memory can grow pretty large.

Given the fact that the expectation of using batch jobs - forecast and carbon aware intensity - are not that large, using `IActionResult` would be enough.

## Consequences
- Dealing with partial failures leaves the client in the 'dark' given the fact that the error is treated as a HTTP 500 error. It doesn't get propagated to the client with a reason, making it hard to diagnose.
- Writing a custom Middleware to handle partial failures, won't scale since it has to buffer the response to avoid dotnet internal errors `(The response has already started, the error handler will not be executed.)`, removing the whole goal of using `IAsyncEnumerable`.

## Green Impact
Neutral
