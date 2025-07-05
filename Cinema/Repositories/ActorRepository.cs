
using Cinema.Repositories.IRepositories;

namespace Cinema.Repositories
{
    public class ActorRepository: Repository<Actor>,IActorRepository
    {
        private readonly ApplicationDbContext _context;
        public ActorRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
