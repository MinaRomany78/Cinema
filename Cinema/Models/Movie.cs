using Cinema.Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cinema.Models
{
    public class Movie
    {//Name, Description, Price, ImgUrl, TrailerUrl, StartDate, EndDate, MovieStatus, CinemaId, CategoryId
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImgUrl { get; set; } = string.Empty;
        public string TrailerUrl { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        //[NotMapped]
        //public MovieStatus MovieStatus {
        //    get
        //    {
        //        var currentDate = DateTime.Now;
        //        if (currentDate < StartDate)
        //            return MovieStatus.Upcoming;
        //        else if (currentDate >= StartDate && currentDate <= EndDate)
        //            return MovieStatus.Available;
        //        else
        //            return MovieStatus.Expired;
        //    }

        //}
        public MovieStatus MovieStatus
        {
            get; set;
        }

        public int CinemaId { get; set; }
        public Cinema Cinema { get; set; } =null!;
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public ICollection<ActorMovie> ActorMovies { get; set; } = new List<ActorMovie>();

    }
}
