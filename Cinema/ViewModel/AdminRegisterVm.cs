using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cinema.ViewModel
{
    public class AdminRegisterVm
    {
        public int Id { get; set; }
        public string userId { get; set; }= null!;

        [Required]
        public string UserName { get; set; } = null!;
        [Required]
        public string FirstName { get; set; } = null!;
        [Required]
        public string LastName { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        //[Required]
        //[DataType(DataType.Password)]
        //public string Password { get; set; } = null!;
        //[Required]
        //[DataType(DataType.Password)]
        //[Compare(nameof(Password))]
        //public string ConfirmPassword { get; set; } = null!;
        //[Required]
        //public string PhoneNumber { get; set; } = null!;
        public string? Address { get; set; }

        
        [NotMapped]

        public string Role { get; set; } = null!;
        [NotMapped]
        public List<SelectListItem> Roles { get; set; } = new();
    }
}
