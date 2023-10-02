var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var cities = new[]
{
    "Sarajevo", "Tesanj", "London", "Paris", "Berlin", "Stuttgart", "Madrid", "Cairo", "Mostar", "New York"
};

app.MapGet("/cities", () =>
{
    Console.WriteLine("Cities API from app1 is called");
    return cities;
})
.WithName("GetCities")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
