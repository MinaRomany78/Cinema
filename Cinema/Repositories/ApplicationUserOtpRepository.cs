
using Cinema.Repositories.IRepositories;

namespace Cinema.Repositories
{
    public class ApplicationUserOtpRepository: Repository<ApplicationUserOtp>, IApplicationUserOtpRepository
    {
        private readonly ApplicationDbContext _context;
        public ApplicationUserOtpRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
