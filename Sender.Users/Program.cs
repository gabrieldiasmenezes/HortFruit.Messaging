using System.Text;
using System.Text.Json;
using RabbitMQ.Client;


var factory = new ConnectionFactory
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest"
};

using IConnection connection = await factory.CreateConnectionAsync();
using IChannel channel = await connection.CreateChannelAsync();

string exchangeName = "hortifruti.exchange";
await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, durable: true);

// Simulando dados do usuário
var user = new
{
    nomeCompleto = "João Gomes",
    endereco = "Rua das Flores, 123, São Paulo",
    RG = "12.345.678-9",
    CPF = "123.456.789-00",
    dataHoraRegistro = DateTime.Now
};

byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(user));
string routingKey = "request.users";

await channel.BasicPublishAsync(
    exchange: exchangeName,
    routingKey: routingKey,
    mandatory: false,
    body: body.AsMemory(),
    cancellationToken: CancellationToken.None
);

Console.WriteLine($"[Sender.Users] Mensagem enviada: {JsonSerializer.Serialize(user)}");
Console.WriteLine("Pressione qualquer tecla para sair...");
Console.ReadKey();
