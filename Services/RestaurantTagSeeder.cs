using DineSure.Data;
using DineSure.Models;
using Microsoft.EntityFrameworkCore;
using static DineSure.Models.User;

namespace DineSure.Services;

public class RestaurantTagSeeder
{
    private readonly ApplicationDbContext _context;

    public RestaurantTagSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedRestaurantTags()
    {
        var restaurants = await _context.Restaurants.ToListAsync();
        var random = new Random(42); // Use seed for consistent results

        var dietaryTypes = new[]
        {
            DietaryType.Vegetarian,
            DietaryType.Vegan,
            DietaryType.Pescatarian,
            DietaryType.Paleo,
            DietaryType.Keto,
            DietaryType.Mediterranean,
            DietaryType.LowCarb
        };

        var allergies = new[]
        {
            Allergy.Peanuts,
            Allergy.TreeNuts,
            Allergy.Milk,
            Allergy.Eggs,
            Allergy.Soy,
            Allergy.Fish,
            Allergy.Shellfish,
            Allergy.Wheat,
            Allergy.Sesame
        };

        int updatedCount = 0;

        foreach (var restaurant in restaurants.Take(100)) // Update first 100 restaurants for testing
        {
            bool needsUpdate = false;

            // Ensure every restaurant has at least one dietary type
            if (!restaurant.SupportedDietaryTypes.Any())
            {
                // Assign 1-3 random dietary types based on cuisine
                int numDietaryTypes = random.Next(1, 4);
                var selectedTypes = new HashSet<DietaryType>();

                // Add cuisine-appropriate dietary types
                var firstCuisine = restaurant.CuisineTypes.FirstOrDefault();
                if (firstCuisine == CuisineType.Italian)
                {
                    selectedTypes.Add(DietaryType.Mediterranean);
                    if (random.NextDouble() < 0.6) selectedTypes.Add(DietaryType.Vegetarian);
                }
                else if (firstCuisine == CuisineType.Indian)
                {
                    selectedTypes.Add(DietaryType.Vegetarian);
                    if (random.NextDouble() < 0.4) selectedTypes.Add(DietaryType.Vegan);
                }
                else if (firstCuisine == CuisineType.Japanese || firstCuisine == CuisineType.Thai)
                {
                    selectedTypes.Add(DietaryType.Pescatarian);
                    if (random.NextDouble() < 0.5) selectedTypes.Add(DietaryType.LowCarb);
                }
                else if (firstCuisine == CuisineType.French)
                {
                    selectedTypes.Add(DietaryType.Mediterranean);
                    if (random.NextDouble() < 0.3) selectedTypes.Add(DietaryType.Keto);
                }

                // Fill remaining slots with random types
                while (selectedTypes.Count < numDietaryTypes)
                {
                    selectedTypes.Add(dietaryTypes[random.Next(dietaryTypes.Length)]);
                }

                restaurant.SupportedDietaryTypes = selectedTypes.ToList();
                needsUpdate = true;
            }

            // Assign 1-4 random allergen-free options
            if (!restaurant.AllergenFreeOptions.Any())
            {
                int numAllergies = random.Next(1, 5);
                var selectedAllergies = new HashSet<Allergy>();

                for (int i = 0; i < numAllergies; i++)
                {
                    selectedAllergies.Add(allergies[random.Next(allergies.Length)]);
                }

                restaurant.AllergenFreeOptions = selectedAllergies.ToList();
                needsUpdate = true;
            }

            // Ensure boolean flags match the dietary types and allergies
            if (restaurant.SupportedDietaryTypes.Contains(DietaryType.Vegetarian) && !restaurant.HasVegetarianOptions)
            {
                restaurant.HasVegetarianOptions = true;
                needsUpdate = true;
            }

            if (restaurant.SupportedDietaryTypes.Contains(DietaryType.Vegan) && !restaurant.HasVeganOptions)
            {
                restaurant.HasVeganOptions = true;
                needsUpdate = true;
            }

            if (restaurant.AllergenFreeOptions.Contains(Allergy.Milk) && !restaurant.HasLactoseFreeOptions)
            {
                restaurant.HasLactoseFreeOptions = true;
                needsUpdate = true;
            }

            if (restaurant.AllergenFreeOptions.Contains(Allergy.Wheat) && !restaurant.HasGlutenFreeOptions)
            {
                restaurant.HasGlutenFreeOptions = true;
                needsUpdate = true;
            }

            // Add some halal and kosher options randomly
            if (random.NextDouble() < 0.2 && !restaurant.HasHalalOptions)
            {
                restaurant.HasHalalOptions = true;
                needsUpdate = true;
            }

            if (random.NextDouble() < 0.15 && !restaurant.HasKosherOptions)
            {
                restaurant.HasKosherOptions = true;
                needsUpdate = true;
            }

            // Add diabetic-friendly options to some restaurants
            if (random.NextDouble() < 0.4 && !restaurant.HasDiabeticFriendlyOptions)
            {
                restaurant.HasDiabeticFriendlyOptions = true;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                updatedCount++;
            }
        }

        if (updatedCount > 0)
        {
            await _context.SaveChangesAsync();
            Console.WriteLine($"Successfully assigned dietary tags to {updatedCount} restaurants.");
        }
        else
        {
            Console.WriteLine("All restaurants already have dietary tags assigned.");
        }
    }
} 