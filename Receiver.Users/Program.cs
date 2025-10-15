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

// Declarar fila do Receiver de usuários
string queueName = "receiver.users.queue";
await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
await channel.QueueBindAsync(queueName, exchangeName, "validated.users");

Console.WriteLine("[Receiver.Users] Aguardando mensagens validadas...");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"\n[Receiver.Users] Mensagem recebida: {message}");
    await Task.Yield();
};

await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

Console.WriteLine("Pressione Ctrl+C para encerrar.");
while (true) Thread.Sleep(1000);
