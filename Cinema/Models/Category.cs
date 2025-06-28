using System.ComponentModel.DataAnnotations;

namespace Cinema.Models
{
    public class Category
    {// name
        public int Id { get; set; }
        [Required]
        [MinLength(3)]
        public string Name { get; set; } = string.Empty;
   
        public ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
}
