using Microsoft.EntityFrameworkCore;

namespace Cinema.Models
{


    [PrimaryKey(nameof(ActorId), nameof(MovieId))]

    public class ActorMovie
    { 
        public int ActorId { get; set; }
        public Actor Actor { get; set; } = null!;
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;
       
    }
}
