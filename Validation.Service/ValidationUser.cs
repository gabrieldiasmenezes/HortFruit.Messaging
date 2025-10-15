using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;

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

// Declarar exchange
await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, durable: true);

// Declarar a fila de validação de usuários
string queueName = "validation.users.queue";
await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
await channel.QueueBindAsync(queueName, exchangeName, "request.users");

Console.WriteLine("[Validation.Users] Aguardando mensagens de usuários...");

// Criar consumidor assíncrono
var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"\n[Validation.Users] Mensagem recebida: {message}");

    // Validação simples: verificar se todos os campos obrigatórios existem
    bool valido = message.Contains("nomeCompleto") &&
                  message.Contains("endereco") &&
                  message.Contains("RG") &&
                  message.Contains("CPF") &&
                  message.Contains("dataHoraRegistro");

    if (valido)
    {
        // Reenviar para o Receiver
        await channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: "validated.users",
            mandatory: false,
            body: body.AsMemory(),
            cancellationToken: CancellationToken.None
        );

        Console.WriteLine("[Validation.Users] Mensagem validada e reenviada com sucesso!");
    }
    else
    {
        Console.WriteLine("[Validation.Users] Mensagem inválida!");
    }
};

await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

Console.WriteLine("Pressione Ctrl+C para encerrar.");
while (true) Thread.Sleep(1000);
