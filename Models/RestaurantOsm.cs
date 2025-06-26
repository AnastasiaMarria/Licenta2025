using System.ComponentModel.DataAnnotations;

namespace DineSure.Models;

public class RestaurantOsm
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Street { get; set; }
    public string? Number { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }
    public string ImageUrl { get; set; } = "";
}
