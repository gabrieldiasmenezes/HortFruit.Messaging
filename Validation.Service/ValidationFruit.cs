using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;
using System.Threading.Tasks;

namespace HortFruit.Messaging.Validation.Service.ValidationFruit
{
    class Program
    {
        static async Task Main(string[] args)
        {
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

            // Declarar a fila de validação de frutas
            string queueName = "validation.fruits.queue";
            await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            await channel.QueueBindAsync(queueName, exchangeName, "request.fruits");

            Console.WriteLine("[Validation.Fruits] Aguardando mensagens de frutas...");

            // Criar consumidor assíncrono
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"\n[Validation.Fruits] Mensagem recebida: {message}");

                // Validação simples: verificar se contém "nome" e "resumo"
                bool valido = message.Contains("nome") && message.Contains("resumo");

                if (valido)
                {
                    // Reenviar para o Receiver
                    await channel.BasicPublishAsync(
                        exchange: exchangeName,
                        routingKey: "validated.fruits",
                        mandatory: false,
                        body: body.AsMemory(),
                        cancellationToken: CancellationToken.None
                    );

                    Console.WriteLine("[Validation.Fruits] Mensagem validada e reenviada com sucesso!");
                }
                else
                {
                    Console.WriteLine("[Validation.Fruits] Mensagem inválida!");
                }
            };

            await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

            Console.WriteLine("Pressione Ctrl+C para encerrar.");
            while (true) Thread.Sleep(1000);
        }
    }
}
