using System.ComponentModel.DataAnnotations;

namespace DineSure.Models;

public class UserFavoriteRestaurant
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    
    [Required]
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
} 