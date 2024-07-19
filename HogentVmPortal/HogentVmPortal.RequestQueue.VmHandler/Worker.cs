using HogentVmPortal.Shared;
using HogentVmPortal.Shared.DTO;
using HogentVmPortal.Shared.Repositories;
using HogentVmPortal.Shared.Model;
using HogentVmPortal.RequestQueue.VmHandler.ProviderStrategies;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Pulumi.Automation;

namespace HogentVmPortal.RequestQueue.VmHandler
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IConnection _connection;
        private IModel _createChannel;
        private IModel _removeChannel;

        private readonly IVirtualMachineRepository _virtualMachineRepository;
        private readonly IVirtualMachineTemplateRepository _virtualMachineTemplateRepository;
        private readonly IAppUserRepository _appUserRepository;

        private readonly IOptions<ProxmoxConfig> _proxmoxConfig;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IOptions<ProxmoxConfig> proxmoxConfig)
        {
            _logger = logger;
            var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            _appUserRepository = services.GetRequiredService<IAppUserRepository>();
            _virtualMachineRepository = services.GetRequiredService<IVirtualMachineRepository>();
            _virtualMachineTemplateRepository = services.GetRequiredService<IVirtualMachineTemplateRepository>();

            _proxmoxConfig = proxmoxConfig;

            //TODO: read connection settings from appsettings
            var factory = new ConnectionFactory() { HostName = "192.168.152.142", UserName = "serviceuser", Password = "root0603", Port = 5672 };
            _connection = factory.CreateConnection();

            _createChannel = _connection.CreateModel();
            _removeChannel = _connection.CreateModel();

            // Declare the create channel
            _createChannel.QueueDeclare(queue: "vm_create_queue",
                durable: true, exclusive: false, autoDelete: false, arguments: null);
            // Set the prefetch count to 2 for the create channel
            _createChannel.BasicQos(prefetchSize: 0, prefetchCount: 2, global: false);

            // Declare the remove channel
            _removeChannel.QueueDeclare(queue: "vm_remove_queue",
                durable: true, exclusive: false, autoDelete: false, arguments: null);
            // Set the prefetch count to 10 for the remove channel
            _removeChannel.BasicQos(prefetchSize: 0, prefetchCount: 5, global: false);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //Create Requests
            var createConsumer = new EventingBasicConsumer(_createChannel);
            createConsumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var jsonMessage = Encoding.UTF8.GetString(body);

                var createRequest = JsonSerializer.Deserialize<VirtualMachineCreateRequest>(jsonMessage);
                if (createRequest != null) await Task.Run(async () => await HandleVirtualMachineCreateRequest(createRequest));

                _createChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                Debug.WriteLine(jsonMessage);
                //_logger.LogInformation(" [x] Received {message}", jsonMessage);
            };

            var removeConsumer = new EventingBasicConsumer(_removeChannel);
            removeConsumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var jsonMessage = Encoding.UTF8.GetString(body);

                var removeRequest = JsonSerializer.Deserialize<VirtualMachineRemoveRequest>(jsonMessage);
                if (removeRequest != null) await Task.Run(async() => await HandleVirtualMachineRemoveRequest(removeRequest));

                _removeChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                Debug.WriteLine(jsonMessage);
            };

            _createChannel.BasicConsume(queue: "vm_create_queue",
                autoAck: false, consumer: createConsumer);

            _removeChannel.BasicConsume(queue: "vm_remove_queue",
                autoAck: false, consumer: removeConsumer);

            return Task.CompletedTask;
        }

        private async Task HandleVirtualMachineCreateRequest(VirtualMachineCreateRequest createRequest)
        {
            if (createRequest == null) return;

            //TODO: init in caller (IOC), pulumiProvider should not have access to .CreateContainer here (ISP - factory pattern)
            //pulumiProvider should have a Factory (CreatorFactory)
            //use the Factory to return the correct Creator class based on the provider, with their CreateVirtualMachine or CreateContainer method
            ProviderStrategy? pulumiProvider;

            try
            {
                var owner = await _appUserRepository.GetById(createRequest.OwnerId);
                var template = await _virtualMachineTemplateRepository.GetByCloneId(createRequest.CloneId);

                var vmArgs = ProxmoxVirtualMachineCreateParams.FromViewModel(createRequest);

                pulumiProvider = new ProxmoxStrategy(_proxmoxConfig.Value); //TODO: init in base

                var projectName = "pulumi_inline";
                var stackName = vmArgs.VmName;

                var pulumiFn = pulumiProvider.CreateVirtualMachine(vmArgs);
                var stackArgs = new InlineProgramArgs(projectName, stackName, pulumiFn);
                var stack = await LocalWorkspace.CreateOrSelectStackAsync(stackArgs);

                //Provision the virtual machine
                var result = await stack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine, ShowSecrets = true });

                var virtualMachine = new VirtualMachine
                {
                    Id = Guid.NewGuid(),
                    Name = createRequest.Name,
                    Owner = owner,
                    Template = template,
                    IpAddress = GetValue(result, "ip", value => value.ToString()) ?? string.Empty,
                    ProxmoxId = GetValue(result, "id", value => int.Parse(value.ToString()!)),
                    Login = GetValue(result, "login", value => value.ToString()) ?? string.Empty
                };

                await _virtualMachineRepository.Add(virtualMachine);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private async Task HandleVirtualMachineRemoveRequest(VirtualMachineRemoveRequest removeRequest)
        {
            if (removeRequest == null) return;

            ProviderStrategy? pulumiProvider;

            try
            {
                var vmArgs = ProxmoxVirtualMachineDeleteParams.FromViewModel(removeRequest);

                pulumiProvider = new ProxmoxStrategy(_proxmoxConfig.Value);

                var projectName = "pulumi_inline";
                var stackName = vmArgs.VmName;

                var pulumiFn = pulumiProvider.RemoveVirtualMachine(vmArgs);
                var stackArgs = new InlineProgramArgs(projectName, stackName, pulumiFn);
                var stack = await LocalWorkspace.SelectStackAsync(stackArgs);

                await stack.DestroyAsync(new DestroyOptions { OnStandardOutput = Console.WriteLine }); //destroying the stack only removes the resources in the stack
                await stack.Workspace.RemoveStackAsync(removeRequest.Name); //use workspace property to remove the now empty stack

                await _virtualMachineRepository.Delete(removeRequest.VmId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static T? GetValue<T>(UpResult? result, string key, Func<object, T> convertFunc)
        {
            return result != null && result.Outputs.TryGetValue(key, out var outputValue) ? convertFunc(outputValue.Value) : default;
        }

        public override void Dispose()
        {
            _createChannel.Close();
            _removeChannel.Close();

            _connection.Close();

            base.Dispose();
        }
    }
}
