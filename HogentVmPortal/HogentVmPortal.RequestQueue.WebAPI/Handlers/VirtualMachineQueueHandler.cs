using HogentVmPortal.Shared.DTO;
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

        public VirtualMachineQueueHandler()
        {

            // TDOO: read from appsettings.json
            _factory = new ConnectionFactory
            {
                HostName = "192.168.152.142",
                UserName = "serviceuser",
                Password = "honda0603",
                Port = 5672,
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
            catch (Exception ex) //CustomException hier op basis van exceptions bij ophalen data
            {
                Debug.WriteLine(ex.Message);
            }
            //Exception voor serialize error

            return messageBody;
        }
    }
}
