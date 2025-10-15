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

// Criar conexão e canal de forma assíncrona
using IConnection connection = await factory.CreateConnectionAsync();
using IChannel channel = await connection.CreateChannelAsync();

// Declarar exchange do tipo Topic
string exchangeName = "hortifruti.exchange";
await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, durable: true);

// Criar a mensagem
var fruit = new
{
    tipo = "fruta",
    nome = "Manga",
    resumo = "Fruta tropical, doce, abundante no verão.",
    dataHoraSolicitacao = DateTime.Now
};

byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(fruit));
string routingKey = "request.fruits";

// Publicar a mensagem corretamente
await channel.BasicPublishAsync(
    exchange: exchangeName,
    routingKey: routingKey,
    mandatory: false,
    body: body.AsMemory(),
    cancellationToken: CancellationToken.None
);

Console.WriteLine($"[Sender.Fruits] Mensagem enviada: {JsonSerializer.Serialize(fruit)}");
Console.WriteLine("Pressione qualquer tecla para sair...");
Console.ReadKey();
