using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading.Tasks;
using System;
using Azure.Messaging.ServiceBus;
using System.Collections.Generic;
using RESTFUL.Interfaces;

namespace RESTFUL.Services
{
    public class ServiceBusService
    {
        private readonly ServiceBusClient _client;
        private readonly ILogger<ServiceBusService> _logger;
        private readonly IItemsRepository _repository;

        private const string DEFAULT_TOPIC_NAME = "topic.1";
        private const string DEFAULT_QUEUE_NAME = "queue.1";

        public ServiceBusService(ServiceBusClient client, ILogger<ServiceBusService> logger, IItemsRepository repository)
        {
            _client = client;
            _logger = logger;
            _repository = repository;
        }

        public async Task PublishMessageToDefaultQueue(string msg)
        {
            var sender = _client.CreateSender(DEFAULT_QUEUE_NAME);

            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            messageBatch.TryAddMessage(new ServiceBusMessage($"{msg}"));

            await sender.SendMessagesAsync(messageBatch);

            await sender.DisposeAsync();

            _logger.LogInformation($"A message has been published to the queue.");
        }

        public async Task<List<string>> ConsumeMessageFromDefaultQueue()
        {
            var messages = new List<string>();

            ServiceBusReceiverOptions opt = new ServiceBusReceiverOptions
            {
                ReceiveMode = ServiceBusReceiveMode.PeekLock,
            };

            ServiceBusReceiver receiver = _client.CreateReceiver(DEFAULT_QUEUE_NAME, opt);

            while (true)
            {
                ServiceBusReceivedMessage message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));
                if (message != null)
                {
                    // Process the message
                    var messageBody = Encoding.UTF8.GetString(message.Body);

                    _logger.LogInformation($"Received message: {messageBody}");

                    messages.Add(messageBody);

                    // Complete the message to remove it from the queue
                    await receiver.CompleteMessageAsync(message);
                }
                else
                {
                    _logger.LogInformation("No messages received.");
                    break;
                }
            }

            _logger.LogInformation("Done recieving");

            await receiver.DisposeAsync();

            return messages;
        }

        public async Task PublishMessageToDefaultTopic(string msg)
        {
            ServiceBusSender sender = _client.CreateSender(DEFAULT_TOPIC_NAME);


            ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes($"Sending a message - {msg}"))
            {
                ContentType = "application/json"
            };

            await sender.SendMessageAsync(message);

            _logger.LogInformation("Sent a message to the default topic.");
        }

    }
}
