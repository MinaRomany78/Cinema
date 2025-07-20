using Cinema.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Threading.Tasks;

namespace Cinema.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly IOrderItemsRepository _orderItemsRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(
            IOrderItemsRepository orderItemsRepository,
            IOrderRepository orderRepository,ITicketRepository ticketRepository,UserManager<ApplicationUser> userManager)
        {
            _orderItemsRepository = orderItemsRepository;
            _orderRepository = orderRepository;
            _ticketRepository = ticketRepository;
            _userManager = userManager;
        }


        public async Task<IActionResult> Success(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var order = await _orderRepository.GetOneAsync(e => e.Id == orderId);
            if (order == null)
                return NotFound();

            // تحديث حالة الطلب
            order.OrderStatus = OrderStatus.Completed;

            // استعلام عن الدفع من Stripe
            var service = new SessionService();
            var session = service.Get(order.SessionId);
            order.PaymentId = session.PaymentIntentId;
            await _orderRepository.Commit();

            // جلب كل التذاكر
            var allTickets = await _ticketRepository.GetAsync(
                e => e.ApplicationUser.Id == user.Id,
                include: [e => e.Movie] );

            List<OrderItem> orderItems = new();
            foreach (var item in allTickets)
            {
                orderItems.Add(new()
                {
                    MovieId = item.MovieId,
                    OrderId = order.Id,
                    TotalPrice = item.TotalPrice,
                    Selected = item.SelectedSeats,
                    datetime = item.Date,

                    Movie = item.Movie // تضمين الفيلم
                });
            }

            // حفظ الطلبات
            await _orderItemsRepository.CreateRangeAsync(orderItems);
            await _orderItemsRepository.Commit();

            // حذف التذاكر من السلة
            foreach (var item in allTickets)
                await _ticketRepository.DeleteAsync(item);

            await _ticketRepository.Commit();

            var vm = new CheckoutSuccessVM
            {
                Order = order,
                OrderItems = orderItems
            };

            return View(vm);
        }


    }
}
