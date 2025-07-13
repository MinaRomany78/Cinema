using System.ComponentModel.DataAnnotations;

namespace Cinema.ViewModel
{
    public class ResendEmailConfirmationVM
    {
        public int Id { get; set; }
        [Required]
        public string EmailOrUserName { get; set; }= null!;
    }
}
