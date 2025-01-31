using Cadastro.API.Interfaces;
using Cadastro.API.Services;
using Cadastro.Configuracoes;
using Cadastro.Data.Repositories;
using Cadastro.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Trace;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Text.Json.Serialization;
using Prometheus;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Prometheus.Client;
using Prometheus.HttpClientMetrics;
using Prometheus.DotNetRuntime;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
                .AddJsonOptions(opt =>
                                {
                                    opt.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
                                    opt.JsonSerializerOptions.UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement;
                                });

builder.Services.AddApiVersioning(x =>
{
    x.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    x.AssumeDefaultVersionWhenUnspecified = true;
    x.ReportApiVersions = true;
    x.ApiVersionReader = new UrlSegmentApiVersionReader();
});

builder.Services.AddVersionedApiExplorer(p =>
{
    p.GroupNameFormat = "'v'VVV";
    p.SubstituteApiVersionInUrl = true;
    p.AddApiVersionParametersWhenVersionNeutral = true;
    p.AssumeDefaultVersionWhenUnspecified = true;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                        (sender, certificate, chain, sslPolicyErrors) => true;

builder.Services.AddAPICustomAuthorizationConfig(builder.Configuration);

builder.Services.AddAPICustomAuthenticationConfig(builder.Environment, builder.Configuration);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = $"Cadastro API - {builder.Environment.EnvironmentName}", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = @"JWT Authorization header Ex.: 'Bearer 12345abcdef'",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      builder =>
                      {
                          builder.AllowAnyOrigin()
                                  .AllowAnyHeader()
                                  .AllowAnyMethod();
                      });
    options.DefaultPolicyName = MyAllowSpecificOrigins;
});


builder.Services.AddHealthChecks().ForwardToPrometheus(); 

var serviceName = typeof(FuncionarioAppService).Assembly.GetName().Name;
var serviceVersion = typeof(FuncionarioAppService).Assembly.GetName().Version!.ToString() ?? "unknown";

builder.Services.AddCustomOpenTelemetryMetrics(serviceName, serviceVersion, builder.Configuration);
builder.Services.AddCustomOpenTelemetryTracing(serviceName, serviceVersion, builder.Configuration);
builder.Services.AddCustomOpenTelemetryLogging(serviceName, serviceVersion, builder.Logging);

builder.Services.AddScoped<IDbConnection>(sp => {
    var connection = new NpgsqlConnection(builder.Configuration.GetConnectionString("Base"));
    connection.Open();
    return connection;
});

builder.Services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

builder.Services.AddScoped<IFuncionarioReadRepository, FuncionarioRepository>();

builder.Services.AddScoped<IFuncionarioAppService, FuncionarioAppService>();

builder.Services.AddSingleton(TracerProvider.Default.GetTracer(serviceName));

builder.Services.AddRabbitCustomConfiguration(builder.Configuration);

builder.Services.AddHttpClient(Options.DefaultName)
        .UseHttpClientMetrics();

builder.Services.AddLogging().AddHttpLogging(opt =>
{
    opt.LoggingFields = HttpLoggingFields.All;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.EnvironmentName.ToUpper().Contains("PROD"))
{
    app.UseHttpLogging();
    app.UseDeveloperExceptionPage();
    IdentityModelEventSource.ShowPII = true;
}

app.UseSwagger();

var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseSwaggerUI(c =>
{
    foreach (var description in provider.ApiVersionDescriptions)
    {
        c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                          $"Cadastro API - {app.Environment.EnvironmentName} {description.GroupName.ToUpperInvariant()}");
    }
    c.DocExpansion(DocExpansion.List);
});

//app.UseHttpsRedirection();
//app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.UseHttpLogging();

app.UseHealthChecks("/health");

app.UseStaticFiles();

app.UseRouting(); 

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();

app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

app.Run();
