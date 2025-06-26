using Microsoft.EntityFrameworkCore;
using DineSure.Data;
using DineSure.Models;

namespace DineSure.Services;

public class FavoriteService
{
    private readonly ApplicationDbContext _context;

    public FavoriteService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ToggleFavoriteAsync(string userId, int restaurantId)
    {
        var existingFavorite = await _context.UserFavoriteRestaurants
            .FirstOrDefaultAsync(f => f.UserId == userId && f.RestaurantId == restaurantId);

        if (existingFavorite != null)
        {
            // Remove from favorites
            _context.UserFavoriteRestaurants.Remove(existingFavorite);
            await _context.SaveChangesAsync();
            return false; // Removed
        }
        else
        {
            // Add to favorites
            var favorite = new UserFavoriteRestaurant
            {
                UserId = userId,
                RestaurantId = restaurantId,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.UserFavoriteRestaurants.Add(favorite);
            await _context.SaveChangesAsync();
            return true; // Added
        }
    }

    public async Task<bool> IsFavoriteAsync(string userId, int restaurantId)
    {
        return await _context.UserFavoriteRestaurants
            .AnyAsync(f => f.UserId == userId && f.RestaurantId == restaurantId);
    }

    public async Task<List<Restaurant>> GetUserFavoritesAsync(string userId)
    {
        return await _context.UserFavoriteRestaurants
            .Where(f => f.UserId == userId)
            .Include(f => f.Restaurant)
            .Select(f => f.Restaurant)
            .ToListAsync();
    }

    public async Task<int> GetFavoriteCountAsync(int restaurantId)
    {
        return await _context.UserFavoriteRestaurants
            .CountAsync(f => f.RestaurantId == restaurantId);
    }
} 