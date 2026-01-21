using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using VitalOps.API.Models;

namespace VitalOps.API.Data;

public class AppDbContext : IdentityDbContext<User>
{

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Workout> Workouts { get; set; }
    public DbSet<ExerciseEntry> ExerciseEntries { get; set; }
    public DbSet<SetEntry> SetEntries { get; set; }
    public DbSet<WeightEntry> WeightEntries { get; set; }
    public DbSet<CalorieEntry> CalorieEntries { get; set; }

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