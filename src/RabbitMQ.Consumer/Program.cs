using RabbitMQ.Client;
using RabbitMQ.Model;
using System.Text.Json;

const string exchangeName = "pedido.exchange";
const string queueName = "pedido.criados";
const string routingKey = "pedido.criado";

var factory = new RabbitMQ.Client.ConnectionFactory()
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest",
    VirtualHost = "/",
    AutomaticRecoveryEnabled = true,
    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
};

await using var connection = await factory.CreateConnectionAsync();
await using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(
    exchange: exchangeName,
    type: RabbitMQ.Client.ExchangeType.Direct,
    durable: true,
    autoDelete: false);

await channel.QueueDeclareAsync(
    queue: queueName,
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null);

await channel.QueueBindAsync(
    queue: queueName,
    exchange: exchangeName,
    routingKey: routingKey,
    arguments: null);

await channel.BasicQosAsync(
    prefetchSize: 0,
    prefetchCount: 1, // indica a quantidade que o RabbitMQ envia de mensagens para este consumer
    global: false);

var consumer = new RabbitMQ.Client.Events.AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (model, ea) =>
{
    try
    {

        var body = ea.Body.ToArray();
        var json = System.Text.Encoding.UTF8.GetString(body);
        var pedido = JsonSerializer.Deserialize<Pedido>(json);

        Console.WriteLine($"====== Pedido recebido ======");
        Console.WriteLine($"[x] Id: {pedido.Id}");
        Console.WriteLine($"[x] Email: {pedido.ClienteEmail}");
        Console.WriteLine($"====== ======= ======");

        await Task.Delay(2000);


        await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
    }
    catch (JsonException)
    {
        Console.WriteLine("Houve um erro ao desserializar o pedido");
        await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
        throw;
    }
    catch (Exception)
    {
        Console.WriteLine("Houve um erro ao processar o pedido");
        await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
        throw;
    }
};

await channel.BasicConsumeAsync(
    queue: queueName,
    autoAck: false,
    consumer: consumer);

Console.WriteLine("Consumer iniciado! Pressione ENTER para sair");
Console.ReadLine();