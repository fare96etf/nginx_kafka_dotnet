using Confluent.Kafka;

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

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    Console.WriteLine("Weather forecast is called");
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapPost("/kafka", () =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = "kafka:9092"
    };
    using var producer = new ProducerBuilder<Null, string>(config).Build();

    var topic = "topic1";
    var message = new Message<Null, string> { Value = "Hello, Kafka!" };
    producer.Produce(topic, message, deliveryReport => {
        Console.WriteLine(deliveryReport.Message.Value);
    });
})
.WithName("KafkaProducer")
.WithOpenApi();

app.MapGet("/kafka", () =>
{
    var config = new ConsumerConfig
    {
        BootstrapServers = "kafka:9092",
        GroupId = "topic1",
        AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest
    };
    using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
    consumer.Subscribe("topic1");
    try
    {
        var message = consumer.Consume();
        Console.WriteLine($"Received message: {message.Value}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
})
.WithName("KafkaConsumer")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
