using System.ComponentModel.DataAnnotations;

namespace Cinema.ViewModel
{
    public class LoginVm
    {
       public int Id { get; set; }    
        [Required]
        public string UserNameOrEmail { get; set; } = null!;
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        public bool RememberMe { get; set; }
    }
   
}
