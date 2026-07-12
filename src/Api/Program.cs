using EventsManager.Application.Events;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton(TimeProvider.System);

// CQRS light : handlers résolus par injection DI directe dans les futurs endpoints (pas de bus).
// IEventRepository n'a pas encore d'implémentation (couche Infrastructure à venir) :
// résoudre les handlers échouera tant qu'elle n'est pas enregistrée.
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
