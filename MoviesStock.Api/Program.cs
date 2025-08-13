using Movies.Api.Clients;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<IMoviesClient, MoviesClient>(c => c.BaseAddress = new Uri("https://localhost:7234"));

builder.Services.AddOpenApi();
builder.Services.AddControllers();

// builder.Services.AddHttpClient<IMoviesClient>(c => c.BaseAddress = new Uri("https://localhost:7127"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();