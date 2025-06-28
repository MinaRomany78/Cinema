using System.ComponentModel.DataAnnotations;

namespace Cinema.Models
{
    public class Cinema
    {
        //Name, Description, CinemaLogo, Address
        public int Id { get; set; }
        [MaxLength(100)]
        [Required]
        [MinLength(3)]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CinemaLogo { get; set; } = string.Empty;
        [Required]
        public string Address { get; set; } = string.Empty;
        public ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
}
