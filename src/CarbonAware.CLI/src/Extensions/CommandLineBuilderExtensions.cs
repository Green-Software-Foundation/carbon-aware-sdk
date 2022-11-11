using CarbonAware.Interfaces;
using System.Collections;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;

namespace CarbonAware.CLI.Extensions;
public static class CommandLineBuilderExtensions
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
        if (exception is IHttpResponseException httpResponseException)
        {
            context.Console.Error.Write($"{httpResponseException.Title}\n");
            context.Console.Error.Write($"{httpResponseException.Status}\n");
            context.Console.Error.Write($"{httpResponseException.Detail}\n");
            exitCode = ExitCode.DataSourceError;
        }
        else if (exception is ArgumentException)
        {
            context.Console.Error.Write($"{exception.Message}\n");
            foreach (DictionaryEntry entry in exception.Data)
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
            context.Console.Error.Write($"{exception.Message}\n");
        }
        context.ExitCode = (int)exitCode;
    }
}