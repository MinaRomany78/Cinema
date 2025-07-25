﻿
using Cinema.Repositories.IRepositories;

namespace Cinema.Repositories
{
    public class OrderRepository: Repository<Order>,IOrderRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
