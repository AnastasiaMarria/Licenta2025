using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using DineSure.Models;

namespace DineSure.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Restaurant> Restaurants { get; set; }
    public DbSet<RestaurantReview> RestaurantReviews { get; set; }
    public DbSet<UserFavoriteRestaurant> UserFavoriteRestaurants { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure User entity
        builder.Entity<User>(entity =>
        {
            // Configure collections to use value converters and comparers
            entity.Property(e => e.DietaryRestrictions)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                )
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            entity.Property(e => e.FoodPreferences)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                )
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            entity.Property(e => e.Allergies)
                .HasConversion(
                    v => string.Join(',', v.Select(a => a.ToString())),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => Enum.Parse<Allergy>(a))
                        .ToList()
                )
                .Metadata.SetValueComparer(new ValueComparer<List<Allergy>>(
                    (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));
        });

        // Configure Restaurant entity
        builder.Entity<Restaurant>(entity =>
        {
            entity.Property(e => e.CuisineTypes)
                .HasConversion(
                    v => string.Join(',', v.Select(c => c.ToString())),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(c => Enum.Parse<CuisineType>(c))
                        .ToList()
                )
                .Metadata.SetValueComparer(new ValueComparer<List<CuisineType>>(
                    (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            entity.Property(e => e.SupportedDietaryTypes)
                .HasConversion(
                    v => string.Join(',', v.Select(d => d.ToString())),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(d => Enum.Parse<DietaryType>(d))
                        .ToList()
                )
                .Metadata.SetValueComparer(new ValueComparer<List<DietaryType>>(
                    (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            entity.Property(e => e.AllergenFreeOptions)
                .HasConversion(
                    v => string.Join(',', v.Select(a => a.ToString())),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => Enum.Parse<Allergy>(a))
                        .ToList()
                )
                .Metadata.SetValueComparer(new ValueComparer<List<Allergy>>(
                    (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));
        });

        // Configure RestaurantReview entity
        builder.Entity<RestaurantReview>(entity =>
        {
            entity.HasOne(r => r.Restaurant)
                .WithMany(r => r.Reviews)
                .HasForeignKey(r => r.RestaurantId);

            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId);
        });

        // Configure UserFavoriteRestaurant entity
        builder.Entity<UserFavoriteRestaurant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.RestaurantId }).IsUnique();
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Restaurant)
                .WithMany()
                .HasForeignKey(e => e.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Reservation entity
        builder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ReservationDateTime);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.RestaurantId);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Restaurant)
                .WithMany()
                .HasForeignKey(e => e.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure MenuItem entity
        builder.Entity<MenuItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Configure decimal precision for Price
            entity.Property(e => e.Price)
                .HasPrecision(10, 2);
            
            // Configure collections to use value converters and comparers
            entity.Property(e => e.SuitableForCuisines)
                .HasConversion(
                    v => string.Join(',', v.Select(x => (int)x)),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(x => (CuisineType)int.Parse(x)).ToList()
                )
                .Metadata.SetValueComparer(new ValueComparer<List<CuisineType>>(
                    (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            entity.Property(e => e.DietaryTypes)
                .HasConversion(
                    v => string.Join(',', v.Select(x => (int)x)),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(x => (DietaryType)int.Parse(x)).ToList()
                )
                .Metadata.SetValueComparer(new ValueComparer<List<DietaryType>>(
                    (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            entity.Property(e => e.AllergenFree)
                .HasConversion(
                    v => string.Join(',', v.Select(x => (int)x)),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(x => (Allergy)int.Parse(x)).ToList()
                )
                .Metadata.SetValueComparer(new ValueComparer<List<Allergy>>(
                    (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));
        });
    }
} 