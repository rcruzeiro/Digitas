using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Digitas.Api.Web.Extensions;

public static class HttpOutput
{
    public static string DefaultType => "https://httpstatuses.com";

    public static ProblemDetails CreateProblemDetail(string title, int statusCode, Uri instance, string detail, Exception? ex)
    {
        if (string.IsNullOrEmpty(title)) throw new ArgumentNullException(nameof(title));

        if (statusCode < 100 || statusCode > 599) throw new ArgumentException("Invalid status code.", nameof(statusCode));

        if (instance is null) throw new ArgumentNullException(nameof(instance));

        if (instance.IsAbsoluteUri) throw new ArgumentException("Problem detail with invalid relative URI.", nameof(instance));

        if (string.IsNullOrEmpty(detail)) throw new ArgumentNullException(nameof(title));

        var problem = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Instance = instance.OriginalString,
            Status = statusCode,
            Type = $"{DefaultType}/{statusCode}"
        };

        if (ex is not null && ex is ValidationException validationException)
        {
            problem.Extensions.Add("validation", validationException.ValidationResult);

            if (ex.InnerException is not null)
            {
                problem.Extensions.Add("diagnostic", ex.InnerException.Message);
            }
        }

        return problem;
    }

    public static ProblemDetails CreateProblemDetail(string title, int statusCode, string? instance, string detail, Exception? ex)
    {
        if (string.IsNullOrEmpty(instance)) throw new ArgumentNullException(nameof(instance));

        if (!Uri.IsWellFormedUriString(instance, UriKind.Relative)) throw new ArgumentException("Problem detail with invalid relative URI.", nameof(instance));

        return CreateProblemDetail(title, statusCode, new Uri(instance, UriKind.Relative), detail, ex);
    }

    public static ProblemDetails CreateProblemDetail(string title, int statusCode, Uri instance, string detail)
        => CreateProblemDetail(title, statusCode, instance, detail, default);

    public static ProblemDetails CreateProblemDetail(string title, int statusCode, string? instance, string detail)
        => CreateProblemDetail(title, statusCode, instance, detail, default);
}
