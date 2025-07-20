namespace Cinema.Repositories.IRepositories
{
    public interface IOrderItemsRepository : IRepository<OrderItem>
    {
        Task<bool>CreateRangeAsync(List<OrderItem> entities);
    }
}
