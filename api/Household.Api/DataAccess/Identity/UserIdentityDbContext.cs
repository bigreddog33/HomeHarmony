using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class UserIdentityDbContext : IdentityDbContext<ApplicationIdentityUser>
{
    public UserIdentityDbContext(DbContextOptions<UserIdentityDbContext> options) : base(options)
    {
        
    }
}