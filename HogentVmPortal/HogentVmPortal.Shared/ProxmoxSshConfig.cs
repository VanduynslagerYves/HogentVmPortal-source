namespace HogentVmPortal.Shared
{
    public class ProxmoxSshConfig
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required string Endpoint { get; set; }
    }
}
