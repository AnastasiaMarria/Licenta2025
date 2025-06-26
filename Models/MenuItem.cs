using System.ComponentModel.DataAnnotations;

namespace DineSure.Models;

public class MenuItem
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, 1000.00)]
    public decimal Price { get; set; }
    
    public MenuCategory Category { get; set; }
    
    public List<CuisineType> SuitableForCuisines { get; set; } = new();
    
    public List<DietaryType> DietaryTypes { get; set; } = new();
    
    public List<Allergy> AllergenFree { get; set; } = new();
    
    public bool IsVegetarian { get; set; }
    public bool IsVegan { get; set; }
    public bool IsGlutenFree { get; set; }
    public bool IsDiabeticFriendly { get; set; }
    public bool IsLactoseFree { get; set; }
    public bool IsHalal { get; set; }
    public bool IsKosher { get; set; }
    public bool IsPureeFriendly { get; set; }
    
    [StringLength(200)]
    public string Ingredients { get; set; } = string.Empty;
    
    public int PreparationTimeMinutes { get; set; }
    
    public bool IsPopular { get; set; }
    public bool IsSpecialDiet { get; set; }
}

public enum MenuCategory
{
    Appetizer,
    Soup,
    Salad,
    MainCourse,
    SideDish,
    Dessert,
    Beverage,
    Alcohol,
    Special
} 