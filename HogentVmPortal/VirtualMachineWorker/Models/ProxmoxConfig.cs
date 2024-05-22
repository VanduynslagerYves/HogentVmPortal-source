namespace VirtualMachineWorker.Models
{
    public class ProxmoxConfig
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required bool Insecure { get; set; }
        public required string Endpoint { get; set; }
        public required string TargetNode { get; set; }
        public required string SourceNode { get; set; }
    }
}
