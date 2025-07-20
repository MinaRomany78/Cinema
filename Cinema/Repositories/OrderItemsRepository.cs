
using Cinema.Repositories.IRepositories;

namespace Cinema.Repositories
{
    public class OrderItemsRepository: Repository<OrderItem>,IOrderItemsRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderItemsRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> CreateRangeAsync(List<OrderItem> entities)
        {
            try
            {
                 _context.OrderItems.AddRange(entities);
                await _context.SaveChangesAsync();
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex:{ex}");
                return false;
            }

        }
    } 
}
