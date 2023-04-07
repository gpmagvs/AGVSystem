using System.ComponentModel.DataAnnotations;

namespace AGVSystem.UserManagers
{

    public class UserLoginRequest
    {
        [Key]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
