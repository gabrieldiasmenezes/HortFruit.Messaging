using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;

// Configuração da conexão
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

// Declarar a fila do Receiver
string queueName = "receiver.fruits.queue";
await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
await channel.QueueBindAsync(queueName, exchangeName, "validated.fruits");

Console.WriteLine("[Receiver.Fruits] Aguardando mensagens validadas...");

// Criar consumidor assíncrono
var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"\n[Receiver.Fruits] Mensagem recebida: {message}");

    await Task.Yield(); // manter assíncrono
};

// Iniciar o consumidor
await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

Console.WriteLine("Pressione Ctrl+C para encerrar.");
while (true) Thread.Sleep(1000);
