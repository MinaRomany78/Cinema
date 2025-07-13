using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Cinema.Utilty.DBInitializer
{
    public class DBInitializer: IDBInitializer
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public DBInitializer(ApplicationDbContext dbContext,RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public void Initialze()
        {
            if (_dbContext.Database.GetPendingMigrations().Any())
            {
                _dbContext.Database.Migrate();
            }
            if (_roleManager.Roles.IsNullOrEmpty()/*&& _userManager.Users.IsNullOrEmpty()*/)
            { 
            _roleManager.CreateAsync(new IdentityRole(SD.SuperAdmin)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Admin)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Customer)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Employee)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Campany)).GetAwaiter().GetResult();

            _userManager.CreateAsync(new()
            {
                FirstName = "Super",
                LastName = "Admin",
                UserName = "SuperAdmin",
                Email = "Superadmin@eraasoft.com",
                EmailConfirmed = true,



            }, "Admin123$").GetAwaiter().GetResult();
            var user = _userManager.FindByNameAsync("SuperAdmin").GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(user, SD.SuperAdmin).GetAwaiter().GetResult();
            }

        }
    }
    
}
