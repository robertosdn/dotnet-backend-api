using Backend.Api.Endpoints.AccessUsers;
using Backend.Api.Endpoints.Auth;
using Backend.Application.DependencyInjection;
using Backend.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddHttpsRedirection(options => options.HttpsPort = 8443);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapHealthChecks("/health");
app.MapAccessUserEndpoints();
app.MapAuthEndpoints();

app.Run();

public partial class Program;