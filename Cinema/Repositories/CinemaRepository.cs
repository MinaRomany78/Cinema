
using Cinema.Repositories.IRepositories;

namespace Cinema.Repositories
{
    public class CinemaRepository: Repository<Models.Cinema>,ICinemaRepository
    {
        private readonly ApplicationDbContext _context;
        public CinemaRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
