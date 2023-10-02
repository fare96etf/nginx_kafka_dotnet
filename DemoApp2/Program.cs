using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

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

app.MapGet("/testApi", () =>
{
    Console.WriteLine("Test API from app2 is called");
    return "testApi";
})
.WithName("GetTestApi")
.WithOpenApi();

app.MapPost("/create-topic/{topic}", async ([FromRoute] string topic) =>
{
    using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = "kafka:9092" }).Build())
    {
        try
        {
            await adminClient.CreateTopicsAsync(new TopicSpecification[] {
                    new TopicSpecification { Name = topic, ReplicationFactor = 1, NumPartitions = 1 } })

;           Console.WriteLine($"Topic {topic} successfully created.");
        }
        catch (CreateTopicsException e)
        {
            Console.WriteLine($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
        }
    }
})
.WithName("KafkaTopic")
.WithOpenApi();

app.MapPost("/queue/{topic}", ([FromRoute] string topic, [FromBody] ProducerBody producerRequest) =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = "kafka:9092"
    };
    using var producer = new ProducerBuilder<Null, string>(config).Build();

    var topicMessage = new Message<Null, string> { Value = producerRequest.Message != null ? producerRequest.Message : "Default Mesagge" };
    producer.Produce(topic, topicMessage, deliveryReport => {
        Console.WriteLine(deliveryReport.Message.Value);
    });
})
.WithName("KafkaProducer")
.WithOpenApi();

app.MapGet("/queue/{topic}", ([FromRoute] string topic) =>
{
    var config = new ConsumerConfig
    {
        BootstrapServers = "kafka:9092",
        GroupId = topic,
        AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest
    };
    using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
    consumer.Subscribe(topic);

    while (true)
    {
        try
        {
            var message = consumer.Consume();
            Console.WriteLine($"Received message: {message.Message.Value}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
})
.WithName("KafkaConsumer")
.WithOpenApi();

var config = new ConsumerConfig
{
    BootstrapServers = "host1:9092,host2:9092",
    GroupId = "foo",
    AutoOffsetReset = AutoOffsetReset.Earliest
};

app.Run();

internal class ProducerBody
{
    public string? Message { get; set; }
}
