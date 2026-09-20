using Backend.Api.Endpoints.AccessUsers;
using Backend.Api.Endpoints.Auth;
using Backend.Application.DependencyInjection;
using Backend.Infrastructure.DependencyInjection;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Backend API";
        options.Theme = ScalarTheme.BluePlanet;
    });
}

app.MapAccessUserEndpoints();
app.MapAuthEndpoints();

app.Run();

public partial class Program;