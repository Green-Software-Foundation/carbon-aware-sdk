using CarbonAware.Interfaces;
using System.Collections;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;

namespace CarbonAware.CLI.Extensions;
internal static class CommandLineBuilderExtensions
{
    public enum ExitCode
    {
        Success = 0,
        Failure = 1,
        InvalidArguments = 2,
        DataSourceError = 3,
    }

    public static CommandLineBuilder UseCarbonAwareExceptionHandler(this CommandLineBuilder builder)
    {
        return builder.UseExceptionHandler(ExceptionHandler);
    }

    private static void ExceptionHandler(Exception exception, InvocationContext context)
    {
        var exitCode = ExitCode.Failure;
        var relevantException = GetRelevantException(exception);
        if (relevantException is IHttpResponseException httpResponseException)
        {
            context.Console.Error.Write($"{httpResponseException.Title}\n");
            context.Console.Error.Write($"{httpResponseException.Status}\n");
            context.Console.Error.Write($"{httpResponseException.Detail}\n");
            exitCode = ExitCode.DataSourceError;
        }
        else if (relevantException is ArgumentException)
        {
            context.Console.Error.Write($"{relevantException.Message}\n");
            foreach (DictionaryEntry entry in relevantException.Data)
            {
                if (entry.Value is string[] messages && entry.Key is string key)
                {
                    context.Console.Error.Write($"{key}: ");
                    foreach (var message in messages)
                    {
                        context.Console.Error.Write($"{message}\n");
                    }
                }
            }
            exitCode = ExitCode.InvalidArguments;
        }
        else
        {
            context.Console.Error.Write($"{relevantException.Message}\n");
            context.Console.Error.Write($"{relevantException.InnerException?.Message}\n");
        }
        context.ExitCode = (int)exitCode;
    }

    private static Exception GetRelevantException(Exception context)
    {
        // Give priority to the inner exception since it contains the exception root cause.
        if (context.InnerException is not null)
        {
            return context.InnerException;
        }
        return context;
    }
}
