
using Cinema.Repositories.IRepositories;

namespace Cinema.Repositories
{
    public class ActorMoviesRepository: Repository<ActorMovie>,IActorMoviesRepository
    {
        private readonly ApplicationDbContext _context;
        public ActorMoviesRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
