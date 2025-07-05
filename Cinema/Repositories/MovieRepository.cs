
using Cinema.Repositories.IRepositories;

namespace Cinema.Repositories
{
    public class MovieRepository: Repository<Movie>,IMovieRepository
    {
        private readonly ApplicationDbContext _context;
        public MovieRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

    }
}
