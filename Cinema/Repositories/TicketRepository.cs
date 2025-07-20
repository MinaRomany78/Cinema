
using Cinema.Repositories.IRepositories;

namespace Cinema.Repositories
{
    public class TicketRepository: Repository<Ticket>, ITicketRepository
    {
        private readonly ApplicationDbContext _context;
        public TicketRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

    }
}
