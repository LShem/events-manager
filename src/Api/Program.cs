using EventsManager.Application.Events;
using EventsManager.Infrastructure;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton(TimeProvider.System);

var connectionString = builder.Configuration.GetConnectionString("EventsManager")
    ?? throw new InvalidOperationException("Chaîne de connexion « EventsManager » absente de la configuration.");
builder.Services.AddInfrastructure(connectionString);

// CQRS light : handlers résolus par injection DI directe dans les futurs endpoints (pas de bus).
builder.Services.AddSingleton<IValidator<CreateEventCommand>, CreateEventCommandValidator>();
builder.Services.AddScoped<CreateEventCommandHandler>();
builder.Services.AddScoped<GetEventQueryHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
