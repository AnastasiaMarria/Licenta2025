using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DineSure.Models;

public class Restaurant
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    [Required]
    public int City { get; set; } = 1; // 1 = Bucharest (temporarily as int to match database)

    [StringLength(50)]
    public string County { get; set; } = string.Empty;

    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Url]
    public string Website { get; set; } = string.Empty;

    [Range(1, 5)]
    public double Rating { get; set; }

    [StringLength(50)]
    public string PriceRange { get; set; } = string.Empty; // €, €€, €€€, €€€€

    public List<CuisineType> CuisineTypes { get; set; } = new();

    public List<DietaryType> SupportedDietaryTypes { get; set; } = new();

    public List<Allergy> AllergenFreeOptions { get; set; } = new();

    public bool HasVegetarianOptions { get; set; }
    public bool HasVeganOptions { get; set; }
    public bool HasGlutenFreeOptions { get; set; }
    public bool HasDiabeticFriendlyOptions { get; set; }
    public bool HasLactoseFreeOptions { get; set; }
    public bool HasHalalOptions { get; set; }
    public bool HasKosherOptions { get; set; }

    [StringLength(100)]
    public string OpeningHours { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<RestaurantReview> Reviews { get; set; } = new();
}

public class RestaurantReview
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Review must be between 10 and 1000 characters")]
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/*public enum RomanianCity
{
    // Major cities
    Bucharest,
    ClujNapoca,
    Timisoara,
    Iasi,
    Constanta,
    Craiova,
    Brasov,
    Galati,
    Ploiesti,
    Oradea,
    Braila,
    Arad,
    Pitesti,
    Sibiu,
    Bacau,
    TarguMures,
    BaiaMare,
    Buzau,
    Botosani,
    SatuMare,
    RamnicuValcea,
    Suceava,
    PiatraNeamt,
    DrobetaTurnuSeverin,
    Focsani,
    Tulcea,
    Targoviste,
    Resita,
    AlbaIulia,
    Bistrita,
    Slatina,
    Calarasi,
    Deva,
    Hunedoara,
    Vaslui,
    Roman,
    TurnuMagurele,
    Giurgiu,
    Slobozia,
    Tecuci,
    Onesti,
    SfantuGheorghe,
    Medgidia,
    Pascani,
    Reghin,
    Dorohoi,
    RamnicuSarat,
    Lugoj,
    Medias,
    RosioriiDeVede,
    Caracal
}*/

public enum CuisineType
{
    Romanian,
    Italian,
    French,
    Chinese,
    Japanese,
    Indian,
    Thai,
    Mexican,
    Greek,
    Turkish,
    Lebanese,
    American,
    British,
    German,
    Spanish,
    Vietnamese,
    Korean,
    Mediterranean,
    Seafood,
    Steakhouse,
    Barbecue,
    Pizza,
    FastFood,
    Cafe,
    Bakery,
    Desserts,
    International,
    Fusion,
    StreetFood,
    Buffet,
    Vegan,
    Organic,
    Sushi,
    Ramen,
    Tapas,
    Bistro,
    BurgerBar,
    WineBar,
    SportsBar,
    FamilyDining
} 