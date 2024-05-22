using System.ComponentModel.DataAnnotations;

namespace HogentVmPortal.Shared.ViewModel
{
    public class VirtualMachineCreate
    {
        [Required]
        public required string Name { get; set; }
        [Required]
        public required string Login { get; set; }
        [Required]
        public required string Password { get; set; }
        [Required]
        [Range(100,999)]
        public required int CloneId { get; set; }
        [Required]
        public required string SshKey { get; set; }
    }
}
