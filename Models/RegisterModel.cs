using System.ComponentModel.DataAnnotations;

namespace DineSure.Models;

public class RegisterModel
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Username can only contain letters, numbers, underscores, and hyphens")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Birth year is required")]
    [Range(1900, 2024, ErrorMessage = "Please enter a valid birth year")]
    public int BirthYear { get; set; } = DateTime.Now.Year - 18; // Default to 18 years old

    public DietaryType DietaryType { get; set; }
    public List<Allergy> Allergies { get; set; } = new();
    public List<string> FoodPreferences { get; set; } = new();
    public List<string> DietaryRestrictions { get; set; } = new();
    public bool HasDiabetes { get; set; }
    public bool IsLactoseIntolerant { get; set; }
    public bool IsGlutenFree { get; set; }
    public bool NeedsPureeFoods { get; set; }
} 