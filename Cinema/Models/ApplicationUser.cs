using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Cinema.Models
{
    public class ApplicationUser :IdentityUser
    {
        public string? Address { get; set; }
        public string FirstName { get; set; } = null!;
   
        public string LastName { get; set; } = null!;
    }
}
