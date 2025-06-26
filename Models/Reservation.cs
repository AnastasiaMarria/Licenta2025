using System.ComponentModel.DataAnnotations;

namespace DineSure.Models;

public class Reservation
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    
    [Required]
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    
    [Required]
    public DateTime ReservationDateTime { get; set; }
    
    [Required]
    [Range(1, 20)]
    public int NumberOfGuests { get; set; }
    
    [StringLength(500)]
    public string SpecialRequests { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    public string CustomerPhone { get; set; } = string.Empty;
    
    [EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;
    
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum ReservationStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Completed,
    NoShow
} 