using HogentVmPortal.Shared.Repositories;
using HogentVmPortal.Shared;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using HogentVmPortal.Shared.DTO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Pulumi.Automation;
using HogentVmPortal.RequestQueue.VmHandler.ProviderStrategies;
using HogentVmPortal.Shared.Model;
using Renci.SshNet;

namespace HogentVmPortal.RequestQueue.CtHandler
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IConnection _connection;
        private IModel _createChannel;
        private IModel _removeChannel;

        private readonly Regex _ipRegex = new Regex(@"\b(?:\d{1,3}\.){3}\d{1,3}\b", RegexOptions.Compiled);
        private const string IPLOGFILENAME = "iplog";

        private readonly IContainerRepository _containerRepository;
        private readonly IContainerTemplateRepository _containerTemplateRepository;
        private readonly IAppUserRepository _appUserRepository;

        private readonly IOptions<ProxmoxConfig> _proxmoxConfig;
        private readonly IOptions<ProxmoxSshConfig> _proxmoxSshConfig;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IOptions<ProxmoxConfig> proxmoxConfig, IOptions<ProxmoxSshConfig> proxmoxSshConfig)
        {
            _logger = logger;
            var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            _appUserRepository = services.GetRequiredService<IAppUserRepository>();
            _containerRepository = services.GetRequiredService<IContainerRepository>();
            _containerTemplateRepository = services.GetRequiredService<IContainerTemplateRepository>();

            _proxmoxConfig = proxmoxConfig;
            _proxmoxSshConfig = proxmoxSshConfig;

            //TODO: read connection settings from appsettings
            var factory = new ConnectionFactory() { HostName = "192.168.152.142", UserName = "serviceuser", Password = "honda0603", Port = 5672 };
            _connection = factory.CreateConnection();

            _createChannel = _connection.CreateModel();
            _removeChannel = _connection.CreateModel();

            // Declare the create channel
            _createChannel.QueueDeclare(queue: "ct_create_queue",
                durable: true, exclusive: false, autoDelete: false, arguments: null);
            // Set the prefetch count to 2 for the create channel
            _createChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            // Declare the remove channel
            _removeChannel.QueueDeclare(queue: "ct_remove_queue",
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

                var createRequest = JsonSerializer.Deserialize<ContainerCreateRequest>(jsonMessage);
                if (createRequest != null) await Task.Run(async () => await HandleContainerCreateRequest(createRequest));

                _createChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                Debug.WriteLine(jsonMessage);
            };

            var removeConsumer = new EventingBasicConsumer(_removeChannel);
            removeConsumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var jsonMessage = Encoding.UTF8.GetString(body);

                var removeRequest = JsonSerializer.Deserialize<ContainerRemoveRequest>(jsonMessage);
                if (removeRequest != null) await Task.Run(async () => await HandleContainerRemoveRequest(removeRequest));

                _removeChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                Debug.WriteLine(jsonMessage);
            };

            _createChannel.BasicConsume(queue: "ct_create_queue",
                autoAck: false, consumer: createConsumer);

            _removeChannel.BasicConsume(queue: "ct_remove_queue",
                autoAck: false, consumer: removeConsumer);

            return Task.CompletedTask;
        }

        private async Task HandleContainerCreateRequest(ContainerCreateRequest createRequest)
        {
            if (createRequest == null) return;

            ProviderStrategy? pulumiProvider;
            try
            {
                var owner = await _appUserRepository.GetById(createRequest.OwnerId);
                var template = await _containerTemplateRepository.GetByCloneId(createRequest.CloneId);

                var vmArgs = ProxmoxContainerCreateParams.FromViewModel(createRequest);

                pulumiProvider = new ProxmoxStrategy(_proxmoxConfig.Value);

                var projectName = "pulumi_inline";
                var stackName = vmArgs.ContainerName;

                var pulumiFn = pulumiProvider.CreateContainer(vmArgs);
                var stackArgs = new InlineProgramArgs(projectName, stackName, pulumiFn);
                var stack = await LocalWorkspace.CreateOrSelectStackAsync(stackArgs);

                //Provision the container
                var result = await stack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine });

                var proxmoxId = int.Parse(GetValue(result, "id", value => value.ToString())!);
                var ip = await GetIpForContainerId(proxmoxId);

                var container = new Container
                {
                    Id = Guid.NewGuid(),
                    Name = createRequest.Name,
                    Owner = owner,
                    Template = template,
                    ProxmoxId = proxmoxId,
                    IpAddress = ip
                };

                await _containerRepository.Add(container);
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

        private async Task HandleContainerRemoveRequest(ContainerRemoveRequest removeRequest)
        {
            if (removeRequest == null) return;

            ProviderStrategy? pulumiProvider;

            try
            {
                var vmArgs = ProxmoxContainerRemoveParams.FromViewModel(removeRequest);

                pulumiProvider = new ProxmoxStrategy(_proxmoxConfig.Value);

                var projectName = "pulumi_inline";
                var stackName = vmArgs.ContainerName;

                var containerDefinition = pulumiProvider.RemoveContainer(vmArgs);
                var stackArgs = new InlineProgramArgs(projectName, stackName, containerDefinition);
                var stack = await LocalWorkspace.SelectStackAsync(stackArgs);

                await stack.DestroyAsync(new DestroyOptions { OnStandardOutput = Console.WriteLine }); //destroying the stack only removes the resources in the stack
                await stack.Workspace.RemoveStackAsync(removeRequest.Name); //use workspace property to remove the now empty stack

                await _containerRepository.Delete(removeRequest.VmId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Creates an SSH connection to the proxmox server and lets the proxmox server query the container ip address
        /// </summary>
        /// <param name="id">The proxmox id of the container</param>
        /// <returns>The ip adddress of the container</returns>
        private async Task<string> GetIpForContainerId(int id)
        {
            var ip = string.Empty;
            var retryCount = 0;

            using (var client = new SshClient(_proxmoxSshConfig.Value.Endpoint, _proxmoxSshConfig.Value.UserName, _proxmoxSshConfig.Value.Password))
            {
                try
                {
                    client.Connect();

                    while (string.IsNullOrEmpty(ip) && retryCount < 5)
                    {
                        //outputs the ip address of the container and writes to filename specified in IPLOGFILENAME
                        await RunCommandAsync(client, "lxc-info -i -n " + id + " > " + IPLOGFILENAME);

                        var result = await RunCommandAsync(client, "cat " + IPLOGFILENAME);

                        var match = _ipRegex.Match(result);
                        if (match.Success)
                        {
                            ip = match.Value;
                            break;
                        }

                        retryCount++;
                        await Task.Delay(2000);
                    }

                    client.Disconnect();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            if (string.IsNullOrEmpty(ip)) { ip = "no ip found"; }
            return ip;
        }

        private async Task<string> RunCommandAsync(SshClient client, string command)
        {
            var commandResult = await Task.Run(() =>
            {
                var result = client.RunCommand(command);
                return result.Result;
            });

            return commandResult;
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
