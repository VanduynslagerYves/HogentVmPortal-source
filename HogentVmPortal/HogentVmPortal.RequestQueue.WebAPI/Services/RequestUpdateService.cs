using HogentVmPortal.Shared.Repositories;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;

//TODO: move this to separate project
namespace HogentVmPortal.RequestQueue.WebAPI.Services
{
    public class RequestUpdateService : BackgroundService
    {
        private IConnection _connection;
        private IModel _requestUpdateChannel;

        private readonly IRequestRepository _requestRepository;

        public RequestUpdateService(IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            _requestRepository = services.GetRequiredService<IRequestRepository>();

            var factory = new ConnectionFactory() { HostName = "192.168.152.142", UserName = "serviceuser", Password = "root0603", Port = 5672 };
            _connection = factory.CreateConnection();

            _requestUpdateChannel = _connection.CreateModel();

            // Declare the update channel
            _requestUpdateChannel.QueueDeclare(queue: "request_update_queue",
                durable: true, exclusive: false, autoDelete: false, arguments: null);
            // Set the prefetch count to 10 for the update channel
            _requestUpdateChannel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //Create Requests
            var updateConsumer = new EventingBasicConsumer(_requestUpdateChannel);
            updateConsumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var jsonMessage = Encoding.UTF8.GetString(body);

                //var updateRequest = JsonConvert.DeserializeObject<Request>(jsonMessage);
                var updateRequest = JsonConvert.DeserializeObject<dynamic>(jsonMessage);

                if (updateRequest != null)
                {
                    var id = Guid.Parse((string)updateRequest.Id);
                    var request = await _requestRepository.GetById(id);

                    if (request != null)
                    {
                        request.Status = updateRequest.Status;
                        request.TimeStamp = updateRequest.TimeStamp;
                        await _requestRepository.Update(request);
                    }
                }
                //if (updateRequest != null) await _requestRepository.Update(updateRequest);

                _requestUpdateChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                Debug.WriteLine(jsonMessage);
            };

            _requestUpdateChannel.BasicConsume(queue: "request_update_queue",
                autoAck: false, consumer: updateConsumer);

            return Task.CompletedTask;
        }
    }
}
