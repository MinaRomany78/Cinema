using Cinema.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Cinema.ViewModel;


namespace Cinema.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public ApplicationDbContext()
        {
        }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Models.Cinema> Cinemas { get; set; }
        public DbSet<ActorMovie> ActorMovies { get; set; }
        public DbSet<ApplicationUserOtp> ApplicationUserOtps { get; set; }
        //Legacy code for migrations
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer("Data Source =.; Initial Catalog = Cinema; Integrated Security = True; TrustServerCertificate = True");

        }
        public DbSet<Cinema.ViewModel.ResetPasswordVM> ResetPasswordVM { get; set; } = default!;
        public DbSet<Cinema.ViewModel.ProfileVm> ProfileVm { get; set; } = default!;
        public DbSet<ChangePasswordVm> ChangePasswordVm { get; set; } = default!;
        public DbSet<Cinema.ViewModel.AdminRegisterVm> AdminRegisterVm { get; set; } = default!;
        

    }
}
