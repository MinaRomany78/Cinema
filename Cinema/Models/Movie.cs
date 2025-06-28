using Cinema.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cinema.Models
{
    public class Movie
    {//Name, Description, Price, ImgUrl, TrailerUrl, StartDate, EndDate, MovieStatus, CinemaId, CategoryId
        public int Id { get; set; }
        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Range(50,1000)]
        public decimal Price { get; set; }

        public string ImgUrl { get; set; } = string.Empty;
        public string TrailerUrl { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        
        public MovieStatus MovieStatus
        {
            get; set;
        }

        
        

       

        [Required]
        

        public int CinemaId { get; set; }
        public Cinema Cinema { get; set; } =null!;
        [Required]
        
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public ICollection<ActorMovie> ActorMovies { get; set; } = new List<ActorMovie>();

    }
}
