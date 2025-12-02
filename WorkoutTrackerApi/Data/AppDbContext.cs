using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using WorkoutTrackerApi.Models;

namespace WorkoutTrackerApi.Data;

public class AppDbContext : IdentityDbContext<User>
{

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        base.OnModelCreating(builder);

        builder.Entity<User>().ToTable("Users");

        builder.Entity<IdentityRole>().ToTable("Roles");

        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

    } 
}