using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace DineSure.Models;

public class User : IdentityUser
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Username can only contain letters, numbers, underscores, and hyphens")]
    public override string? UserName { get => base.UserName ?? string.Empty; set => base.UserName = value ?? string.Empty; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public override string? Email { get => base.Email ?? string.Empty; set => base.Email = value ?? string.Empty; }

    public DietaryType DietaryType { get; set; }

    public List<string> DietaryRestrictions { get; set; } = new();

    public List<Allergy> Allergies { get; set; } = new();

    public List<string> FoodPreferences { get; set; } = new();

    public bool HasDiabetes { get; set; }

    public bool IsLactoseIntolerant { get; set; }

    public bool IsGlutenFree { get; set; }

    public bool NeedsPureeFoods { get; set; }

    [Required(ErrorMessage = "Birth year is required")]
    [Range(1900, 2024, ErrorMessage = "Please enter a valid birth year")]
    public int BirthYear { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Required by Identity framework
    public override string? NormalizedUserName { get; set; }
    public override string? NormalizedEmail { get; set; }
    public override string? PasswordHash { get; set; }
    public override string? SecurityStamp { get; set; }
    public override string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
    public override bool EmailConfirmed { get; set; }
    public override bool PhoneNumberConfirmed { get; set; }
    public override bool TwoFactorEnabled { get; set; }
    public override DateTimeOffset? LockoutEnd { get; set; }
    public override bool LockoutEnabled { get; set; }
    public override int AccessFailedCount { get; set; }
}

public enum DietaryType
{
    None,
    Vegetarian,
    Vegan,
    Pescatarian,
    FlexitarianMostlyPlantBased,
    Paleo,
    Keto,
    Mediterranean,
    DairyFree,
    LowCarb,
    Halal,
    Kosher
}

public enum Allergy
{
    None,
    Peanuts,
    TreeNuts,
    Milk,
    Eggs,
    Soy,
    Fish,
    Shellfish,
    Wheat,
    Sesame,
    Mustard,
    Celery,
    Lupin,
    Molluscs,
    Sulphites,
    Corn,
    Berries,
    Citrus,
    Chocolate,
    Garlic,
    Onion,
    Avocado,
    Mushrooms,
    Tomatoes,
    Strawberries,
    Pineapple,
    Mango,
    Kiwi,
    Melon,
    Sunflower,
    Poppy,
    Coconut
} 