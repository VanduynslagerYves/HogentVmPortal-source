using HogentVmPortal.Shared;
using HogentVmPortal.Shared.DTO;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace HogentVmPortal.RequestQueue.WebAPI.Handlers
{
    public class VirtualMachineQueueHandler
    {
        private readonly IConnectionFactory _factory;
        private readonly RabbitMQConfig _rabbitMQConfig;

        public VirtualMachineQueueHandler(IOptions<RabbitMQConfig> rabbitMQConfig)
        {
            _rabbitMQConfig = rabbitMQConfig.Value;

            _factory = new ConnectionFactory
            {
                HostName = _rabbitMQConfig.Uri,
                Port = _rabbitMQConfig.Port,
                UserName = _rabbitMQConfig.UserName,
                Password = _rabbitMQConfig.Password,
            };
        }

        public void EnqueueCreateRequest(VirtualMachineCreateRequest createRequest)
        {
            try
            {
                // Establish a connection to RabbitMQ server
                using(var connection = _factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var messageData = GetMessageData(createRequest);

                    if (messageData != null)
                    {
                        // Declare a queue (if it doesn't already exist)
                        channel.QueueDeclare(queue: "vm_create_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

                        // Publish the message to the queue
                        channel.BasicPublish(exchange: "", routingKey: "vm_create_queue", basicProperties: null, body: messageData);
                    }
                }
            }
            catch(BrokerUnreachableException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void EnqueueRemoveRequest(VirtualMachineRemoveRequest removeRequest)
        {
            try
            {
                using (var connection = _factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var messageData = GetMessageData(removeRequest);

                    if (messageData != null)
                    {
                        // Declare a queue (if it doesn't already exist)
                        channel.QueueDeclare(queue: "vm_remove_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

                        // Publish the message to the queue
                        channel.BasicPublish(exchange: "", routingKey: "vm_remove_queue", basicProperties: null, body: messageData);
                    }
                }
            }
            catch (BrokerUnreachableException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        private byte[]? GetMessageData(object data)
        {
            byte[]? messageBody = null;

            try
            {
                //Parse the data to json, to byte[]
                var jsonMessage = JsonSerializer.Serialize(data);
                messageBody = Encoding.UTF8.GetBytes(jsonMessage);

                return messageBody;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return messageBody;
        }
    }
}
