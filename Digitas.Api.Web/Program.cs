using System.Text.Json;
using System.Text.Json.Serialization;
using Digitas.Api.Web;
using Digitas.Api.Web.Extensions;
using Digitas.Api.Web.Middlewares;
using Digitas.Core;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOptions();
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services
    .AddResponseCaching()
    .AddHealthChecks();

// custom middlewares
builder.Services.TryAddSingleton<GlobalMiddleware>();

// api versioning
builder.Services
    .AddApiVersioning(setup =>
    {
        setup.DefaultApiVersion = new ApiVersion(1, 0);
        setup.AssumeDefaultVersionWhenUnspecified = true;
        setup.ReportApiVersions = true;
        setup.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                                                          new HeaderApiVersionReader("x-api-version"),
                                                          new MediaTypeApiVersionReader("x-api-version"));
    });

builder.Services
    .AddVersionedApiExplorer(setup =>
    {
        setup.GroupNameFormat = "'v'VVV";
        setup.SubstituteApiVersionInUrl = true;
    });

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

// configure core application
builder.Services.AddCore(configure =>
{
    var settings = new ApiSettings();
    configuration.GetSection("Settings").Bind(settings);

    configure.ConnectionString = settings.Mongo.ConnectionString;
    configure.Database = settings.Mongo.Database;
});

builder.Host
    .ConfigureAppConfiguration((context, configuration) =>
    {
        configuration
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
    });

var app = builder.Build();
var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.Reverse())
    {
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
    }
});

app
    .UseStaticFiles()
    .UseResponseCaching()
    .UseRouting()
    .UseMiddleware<GlobalMiddleware>()
    .UseCors(setup =>
    {
        setup.AllowAnyHeader();
        setup.AllowAnyMethod();
        setup.AllowAnyOrigin();
    })
    .UseRequestLocalization(options =>
    {
        options.DefaultRequestCulture = new RequestCulture("pt-BR");
        options.AddSupportedCultures("pt-BR");
        options.SetDefaultCulture("pt-BR");
    })
    .UseHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = p => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    })
    .UseHttpsRedirection()
    .UseEndpoints(endpoints =>
    {
        // we can write some minimal endpoint here if necessary

        endpoints.MapControllers();
    });

app.Run();
