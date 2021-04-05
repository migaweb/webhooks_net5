using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AirlineSendAgent.Client;
using AirlineSendAgent.Data;
using AirlineSendAgent.Dtos;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AirlineSendAgent.App
{
  public class AppHost : IAppHost
  {
    private readonly SendAgentDbContext _dbContext;
    private readonly IWebhookClient _webhookClient;

    public AppHost(SendAgentDbContext dbContext, IWebhookClient webhookClient)
    {
      _dbContext = dbContext;
      _webhookClient = webhookClient;
    }
    public void Run()
    {
      var factory = new ConnectionFactory() {
        HostName = "localhost", Port = 5672
      };

      using (var connection = factory.CreateConnection())
      using (var channel = connection.CreateModel()) 
      {
        channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);
        var queueName = channel.QueueDeclare().QueueName;

        channel.QueueBind(queue: queueName, exchange: "trigger", routingKey: "");

        var consumer = new EventingBasicConsumer(channel);
        Console.WriteLine("Listening on the message bus ...");

        consumer.Received += async (ModuleHandle, ea) => {
          Console.WriteLine("Event is triggered!");
          var notificationMessage = Encoding.UTF8.GetString(ea.Body.ToArray());
          var message = JsonSerializer.Deserialize<NotificationMessageDto>(notificationMessage);

          var webhookToSend = new FlightDetailChangePayloadDto {
            WebhookType = message.WebhookType,
            WebhookUri = string.Empty,
            Secret = string.Empty,
            Publisher = string.Empty,
            OldPrice = message.OldPrice,
            NewPrice = message.NewPrice,
            FlightCode = message.FlightCode
          };

          foreach (var webhookSubscription in _dbContext.WebhookSubscriptions
              .Where(subscription => subscription.WebhookType == message.WebhookType)) 
          {
            webhookToSend.WebhookUri = webhookSubscription.WebhookURI;
            webhookToSend.Secret = webhookSubscription.Secret;
            webhookToSend.Publisher = webhookSubscription.WebhookPublisher;

            await _webhookClient.SendWebhookNotification(webhookToSend);
          }
        };

        channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

        Console.ReadLine();
      }
    }
  }
}