// TicketController.cs
using Cinema.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PaymentMethod = Cinema.Models.PaymentMethod;

namespace Cinema.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class TicketController : Controller
    {
        private readonly IMovieRepository _movieRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITicketRepository _ticketRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemsRepository _orderItemsRepository;

        public TicketController(IMovieRepository movieRepository, UserManager<ApplicationUser> userManager, ITicketRepository ticketRepository,IOrderRepository orderRepository,IOrderItemsRepository orderItemsRepository)
        {
            _movieRepository = movieRepository;
            _userManager = userManager;
            _ticketRepository = ticketRepository;
            _orderRepository = orderRepository;
            _orderItemsRepository = orderItemsRepository;
        }

        public async Task<IActionResult> Index(int Id)
        {
            var movie = await _movieRepository.GetOneAsync(e => e.Id == Id, include: [e => e.Cinema, e => e.Category]);
            if (movie == null)
                return NotFound();

            return View(movie);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int MovieId, DateTime Time, int Count, decimal TotalPrice, string SelectedSeats)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var movie = await _movieRepository.GetOneAsync(e => e.Id == MovieId, include: [e => e.Cinema]);
            if (movie.Cinema.HallCapacity < Count || Count <= 0)
            {
                TempData["error-notification"] = "Over capacity";
                return RedirectToAction("Index", "Ticket", new { area = "customer", id = MovieId });
            }

            var fullDateTime = Time;
            var requestedSeats = SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            var takenSeats = (await _ticketRepository.GetAsync(e =>
                e.MovieId == MovieId &&
                e.Date == fullDateTime &&
                !string.IsNullOrEmpty(e.SelectedSeats)))
                .SelectMany(e => e.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .ToHashSet();

            if (requestedSeats.Any(seat => takenSeats.Contains(seat)))
            {
                TempData["error-notification"] = "بعض الكراسي التي اخترتها محجوزة بالفعل.";
                return RedirectToAction("Index", "Ticket", new { area = "customer", id = MovieId });
            }

            var MoveInCart = await _ticketRepository.GetOneAsync(a => a.ApplicationUserId == user.Id && a.MovieId == MovieId);
            if (MoveInCart is not null)
            {
                if (movie.Cinema.HallCapacity >= (MoveInCart.Count + Count))
                {
                    MoveInCart.Count += Count;
                    MoveInCart.TotalPrice += TotalPrice;
                    MoveInCart.SelectedSeats += "," + SelectedSeats;
                    await _ticketRepository.UpdateAsync(MoveInCart);
                }
                else
                {
                    TempData["error-notification"] = "Over capacity";
                    return RedirectToAction("Index", "Ticket", new { area = "customer", id = MovieId });
                }
            }
            else
            {
                await _ticketRepository.CreateAsync(new Ticket
                {
                    ApplicationUserId = user.Id,
                    MovieId = MovieId,
                    Count = Count,
                    Date = Time,
                    TotalPrice = TotalPrice,
                    SelectedSeats = SelectedSeats
                });
            }

            await _ticketRepository.Commit();
            TempData["success-notification"] = "Add to cart successfully!";
            return RedirectToAction("Tickets", "Ticket", new { area = "customer" });
        }

        [HttpGet]
        public async Task<IActionResult> GetTakenSeats(int movieId, DateTime date, string time)
        {
            DateTime fullDateTime = DateTime.Parse($"{date.ToShortDateString()} {time}");

            var tickets = await _orderItemsRepository.GetAsync(
                e => e.MovieId == movieId && e.datetime == fullDateTime);

            var takenSeats = tickets
                .Where(t => !string.IsNullOrEmpty(t.Selected))
                .SelectMany(t => t.Selected.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Distinct()
                .ToList();

            return Json(takenSeats);
        }

        public async Task<IActionResult> Tickets()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var tickets = await _ticketRepository.GetAsync(e => e.ApplicationUserId == user.Id, include: [e => e.Movie]);
            if (tickets == null)
                return NotFound();

            return View(tickets);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var ticket = await _ticketRepository.GetOneAsync(
                e => e.ApplicationUserId == user.Id && e.MovieId == movieId,
                include: [e => e.Movie, e => e.Movie.Cinema]);

            if (ticket is null || ticket.Movie is null)
            {
                TempData["error-notification"] = "Ticket not found!";
                return RedirectToAction("Tickets");
            }

            var model = new SeatPickerViewModel
            {
                MovieId = ticket.MovieId,
                Movie = ticket.Movie,
                Count = ticket.Count,
                SelectedSeats = ticket.SelectedSeats,
                Time = ticket.Date
            };

            return View("Edit", model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(SeatPickerViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var ticket = await _ticketRepository.GetOneAsync(
                e => e.ApplicationUserId == user.Id && e.MovieId == model.MovieId,
                include: [e => e.Movie, e => e.Movie.Cinema]);

            if (ticket is null || ticket.Movie is null)
            {
                TempData["error-notification"] = "Ticket not found!";
                return RedirectToAction("Tickets");
            }

            var requestedSeats = model.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            var takenSeats = (await _ticketRepository.GetAsync(e =>
                e.MovieId == model.MovieId &&
                e.Date == model.Time &&
                e.ApplicationUserId != user.Id))
                .SelectMany(e => e.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .ToHashSet();

            if (requestedSeats.Any(seat => takenSeats.Contains(seat)))
            {
                TempData["error-notification"] = "Some seats are already taken.";
                return RedirectToAction("Edit", new { movieId = model.MovieId });
            }

            if (ticket.Movie.Cinema.HallCapacity < model.Count || model.Count <= 0)
            {
                TempData["error-notification"] = "Invalid ticket count.";
                return RedirectToAction("Edit", new { movieId = model.MovieId });
            }

            ticket.Count = model.Count;
            ticket.Date = model.Time;
            ticket.SelectedSeats = model.SelectedSeats;
            ticket.TotalPrice = ticket.Count * ticket.Movie.Price;

            await _ticketRepository.UpdateAsync(ticket);
            await _ticketRepository.Commit();

            TempData["success-notification"] = "Ticket updated successfully!";
            return RedirectToAction("Tickets");
        }
        public async Task<IActionResult> Delete(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return RedirectToAction("Login", controllerName: "Account", new { area = "Identity" });
            var ticket = await _ticketRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.MovieId == movieId, include: [e => e.Movie]);
            if (ticket is null || ticket.Movie is null)
            {
                TempData["error-notification"] = "Ticket not found!";
                return RedirectToAction("Tickets");
            }
            await _ticketRepository.DeleteAsync(ticket);
            await _ticketRepository.Commit();
            TempData["success-notification"] = "Ticket deleted successfully!";
            return RedirectToAction("Tickets");
        }
        public async Task<IActionResult> Pay()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var ticket = await _ticketRepository.GetAsync(e => e.ApplicationUserId == user.Id, include: [e => e.Movie]);
            if (ticket is not null)
            {
               await _orderRepository.CreateAsync(new ()
                {
                    ApplicationUserId = user.Id,
                    Date = DateTime.UtcNow,
                    TotalPrice = ticket.Sum(t => t.TotalPrice),
                    OrderStatus = OrderStatus.pending,
                    PaymentMethod= PaymentMethod.Visa,


               });
                await _orderRepository.Commit();
               var lastOrder= (await _orderRepository.GetAsync(e => e.ApplicationUserId == user.Id)).OrderBy(e=>e.Id).LastOrDefault();
               if (lastOrder is null)
                {
                    TempData["error-notification"] = "Failed to create order.";
                    return RedirectToAction("Tickets");
                }
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>(),

                    Mode = "payment",
                    SuccessUrl = $"{Request.Scheme}://{Request.Host}/Customer/Checkout/Success?orderId={lastOrder.Id}",
                    CancelUrl = $"{Request.Scheme}://{Request.Host}/Customer/Checkout/Cancel",
                };


                foreach (var item in ticket)
                {
                    options.LineItems.Add(new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "egp",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Movie.Name,
                                Description = $"Ticket for {item.Movie.Name} on {item.Date}",
                               


                            },
                            UnitAmount = (long)(item.TotalPrice * 100),
                        },
                        Quantity = item.Count,

                    }); 
                }


                var service = new SessionService();
                var session = service.Create(options);
                lastOrder.SessionId = session.Id;
                await _orderRepository.Commit();
                return Redirect(session.Url);
            }
            else
            {
                TempData["error-notification"] = "No tickets found for payment.";
                return RedirectToAction("Tickets");
            }

        }
    }
}
