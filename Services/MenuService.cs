using DineSure.Models;

namespace DineSure.Services;

public class MenuService
{
    private readonly List<MenuItem> _baseMenuItems;

    public MenuService()
    {
        _baseMenuItems = InitializeBaseMenuItems();
    }

    public List<MenuItem> GenerateMenuForUser(User? user, List<CuisineType> restaurantCuisines)
    {
        // Get restaurant-specific menu items
        var restaurantMenu = GenerateRestaurantSpecificMenu(restaurantCuisines);
        var suitableItems = new List<MenuItem>();

        foreach (var item in restaurantMenu)
        {
            // Check dietary restrictions
            if (IsItemSuitableForUser(item, user))
            {
                // Create a copy and adjust for user
                var menuItem = CreatePersonalizedMenuItem(item, user);
                suitableItems.Add(menuItem);
            }
        }

        // Ensure we have at least some items from each category
        return OrganizeMenuByCategory(suitableItems, restaurantCuisines.FirstOrDefault());
    }

    private List<MenuItem> GenerateRestaurantSpecificMenu(List<CuisineType> restaurantCuisines)
    {
        var restaurantMenu = new List<MenuItem>();
        var primaryCuisine = restaurantCuisines.FirstOrDefault();

        // Create cuisine-specific menus
        switch (primaryCuisine)
        {
            case CuisineType.Italian:
                restaurantMenu.AddRange(GetItalianMenu());
                break;
            case CuisineType.Chinese:
                restaurantMenu.AddRange(GetChineseMenu());
                break;
            case CuisineType.Indian:
                restaurantMenu.AddRange(GetIndianMenu());
                break;
            case CuisineType.French:
                restaurantMenu.AddRange(GetFrenchMenu());
                break;
            case CuisineType.Mexican:
                restaurantMenu.AddRange(GetMexicanMenu());
                break;
            case CuisineType.Thai:
                restaurantMenu.AddRange(GetThaiMenu());
                break;
            case CuisineType.Japanese:
                restaurantMenu.AddRange(GetJapaneseMenu());
                break;
            case CuisineType.Romanian:
                restaurantMenu.AddRange(GetRomanianMenu());
                break;
            default:
                // For generic restaurants, mix items from base menu
                restaurantMenu.AddRange(GetGenericMenu());
                break;
        }

        // Add some common items that most restaurants would have
        restaurantMenu.AddRange(GetCommonItems());

        return restaurantMenu;
    }

    public List<MenuItem> GenerateMenuForRestaurant(Restaurant restaurant, User? user = null)
    {
        var restaurantMenu = new List<MenuItem>();
        
        // Get base cuisine menu
        var cuisineTypes = restaurant.CuisineTypes ?? new List<CuisineType>();
        restaurantMenu.AddRange(GenerateRestaurantSpecificMenu(cuisineTypes));
        
        // Add specialized dishes based on restaurant's dietary offerings
        restaurantMenu.AddRange(GetDietaryTypeSpecificDishes(restaurant));
        restaurantMenu.AddRange(GetHealthConditionSpecificDishes(restaurant));
        restaurantMenu.AddRange(GetAllergenSafeDishes(restaurant));
        
        // Filter for user if provided
        if (user != null)
        {
            var suitableItems = new List<MenuItem>();
            foreach (var item in restaurantMenu)
            {
                if (IsItemSuitableForUser(item, user))
                {
                    var personalizedItem = CreatePersonalizedMenuItem(item, user);
                    suitableItems.Add(personalizedItem);
                }
            }
            return OrganizeMenuByCategory(suitableItems, cuisineTypes.FirstOrDefault());
        }
        
        return OrganizeMenuByCategory(restaurantMenu, cuisineTypes.FirstOrDefault());
    }

    private List<MenuItem> GetDietaryTypeSpecificDishes(Restaurant restaurant)
    {
        var dietaryDishes = new List<MenuItem>();
        
        if (restaurant.HasVegetarianOptions)
        {
            dietaryDishes.AddRange(GetRestaurantVegetarianDishes());
        }
        
        if (restaurant.HasVeganOptions)
        {
            dietaryDishes.AddRange(GetRestaurantVeganDishes());
        }
        
        if (restaurant.SupportedDietaryTypes?.Contains(DietaryType.Paleo) == true)
        {
            dietaryDishes.AddRange(GetRestaurantPaleoDishes());
        }
        
        if (restaurant.SupportedDietaryTypes?.Contains(DietaryType.Keto) == true)
        {
            dietaryDishes.AddRange(GetRestaurantKetoDishes());
        }
        
        if (restaurant.SupportedDietaryTypes?.Contains(DietaryType.Mediterranean) == true)
        {
            dietaryDishes.AddRange(GetRestaurantMediterraneanDishes());
        }
        
        if (restaurant.SupportedDietaryTypes?.Contains(DietaryType.Pescatarian) == true)
        {
            dietaryDishes.AddRange(GetRestaurantPescatarianDishes());
        }
        
        if (restaurant.SupportedDietaryTypes?.Contains(DietaryType.FlexitarianMostlyPlantBased) == true)
        {
            dietaryDishes.AddRange(GetRestaurantFlexitarianDishes());
        }
        
        if (restaurant.SupportedDietaryTypes?.Contains(DietaryType.LowCarb) == true)
        {
            dietaryDishes.AddRange(GetRestaurantLowCarbDishes());
        }
        
        if (restaurant.HasHalalOptions)
        {
            dietaryDishes.AddRange(GetRestaurantHalalDishes());
        }
        
        if (restaurant.HasKosherOptions)
        {
            dietaryDishes.AddRange(GetRestaurantKosherDishes());
        }
        
        return dietaryDishes;
    }

    private List<MenuItem> GetHealthConditionSpecificDishes(Restaurant restaurant)
    {
        var healthDishes = new List<MenuItem>();
        
        if (restaurant.HasDiabeticFriendlyOptions)
        {
            healthDishes.AddRange(GetRestaurantDiabeticFriendlyDishes());
        }
        
        if (restaurant.HasLactoseFreeOptions)
        {
            healthDishes.AddRange(GetRestaurantLactoseFreeDishes());
        }
        
        if (restaurant.HasGlutenFreeOptions)
        {
            healthDishes.AddRange(GetRestaurantGlutenFreeDishes());
        }
        
        return healthDishes;
    }

    private List<MenuItem> GetAllergenSafeDishes(Restaurant restaurant)
    {
        var allergenSafeDishes = new List<MenuItem>();
        
        if (restaurant.AllergenFreeOptions?.Contains(Allergy.Peanuts) == true)
        {
            allergenSafeDishes.AddRange(GetRestaurantPeanutFreeDishes());
        }
        
        if (restaurant.AllergenFreeOptions?.Contains(Allergy.TreeNuts) == true)
        {
            allergenSafeDishes.AddRange(GetRestaurantTreeNutFreeDishes());
        }
        
        if (restaurant.AllergenFreeOptions?.Contains(Allergy.Milk) == true)
        {
            allergenSafeDishes.AddRange(GetRestaurantMilkFreeDishes());
        }
        
        if (restaurant.AllergenFreeOptions?.Contains(Allergy.Eggs) == true)
        {
            allergenSafeDishes.AddRange(GetRestaurantEggFreeDishes());
        }
        
        if (restaurant.AllergenFreeOptions?.Contains(Allergy.Soy) == true)
        {
            allergenSafeDishes.AddRange(GetRestaurantSoyFreeDishes());
        }
        
        if (restaurant.AllergenFreeOptions?.Contains(Allergy.Fish) == true)
        {
            allergenSafeDishes.AddRange(GetRestaurantFishFreeDishes());
        }
        
        if (restaurant.AllergenFreeOptions?.Contains(Allergy.Shellfish) == true)
        {
            allergenSafeDishes.AddRange(GetRestaurantShellfishFreeDishes());
        }
        
        if (restaurant.AllergenFreeOptions?.Contains(Allergy.Wheat) == true)
        {
            allergenSafeDishes.AddRange(GetRestaurantWheatFreeDishes());
        }
        
        if (restaurant.AllergenFreeOptions?.Contains(Allergy.Sesame) == true)
        {
            allergenSafeDishes.AddRange(GetRestaurantSesameFreeDishes());
        }
        
        return allergenSafeDishes;
    }

    private bool IsItemSuitableForUser(MenuItem item, User? user)
    {
        // If no user, include all items (anonymous users see full menu)
        if (user == null) return true;

        // Check dietary type
        if (user.DietaryType != DietaryType.None && 
            item.DietaryTypes.Any() && 
            !item.DietaryTypes.Contains(user.DietaryType))
        {
            return false;
        }

        // Check allergies
        if (user.Allergies.Any(a => a != Allergy.None))
        {
            var userAllergies = user.Allergies.Where(a => a != Allergy.None);
            if (!userAllergies.All(allergy => item.AllergenFree.Contains(allergy)))
            {
                return false;
            }
        }

        // Check health conditions
        if (user.HasDiabetes && !item.IsDiabeticFriendly) return false;
        if (user.IsLactoseIntolerant && !item.IsLactoseFree) return false;
        if (user.IsGlutenFree && !item.IsGlutenFree) return false;
        if (user.NeedsPureeFoods && !item.IsPureeFriendly) return false;

        // Check dietary restrictions
        if (user.DietaryRestrictions.Contains("Vegetarian") && !item.IsVegetarian) return false;
        if (user.DietaryRestrictions.Contains("Vegan") && !item.IsVegan) return false;
        if (user.DietaryRestrictions.Contains("Halal") && !item.IsHalal) return false;
        if (user.DietaryRestrictions.Contains("Kosher") && !item.IsKosher) return false;

        return true;
    }

    private MenuItem CreatePersonalizedMenuItem(MenuItem baseItem, User? user)
    {
        var item = new MenuItem
        {
            Name = baseItem.Name,
            Description = baseItem.Description,
            Price = baseItem.Price,
            Category = baseItem.Category,
            SuitableForCuisines = baseItem.SuitableForCuisines,
            DietaryTypes = baseItem.DietaryTypes,
            AllergenFree = baseItem.AllergenFree,
            IsVegetarian = baseItem.IsVegetarian,
            IsVegan = baseItem.IsVegan,
            IsGlutenFree = baseItem.IsGlutenFree,
            IsDiabeticFriendly = baseItem.IsDiabeticFriendly,
            IsLactoseFree = baseItem.IsLactoseFree,
            IsHalal = baseItem.IsHalal,
            IsKosher = baseItem.IsKosher,
            IsPureeFriendly = baseItem.IsPureeFriendly,
            Ingredients = baseItem.Ingredients,
            PreparationTimeMinutes = baseItem.PreparationTimeMinutes,
            IsPopular = baseItem.IsPopular,
            IsSpecialDiet = baseItem.IsSpecialDiet
        };

        // Add special dietary indicators to description (only for logged-in users)
        if (user != null)
        {
            var indicators = new List<string>();
            if (user.NeedsPureeFoods && item.IsPureeFriendly) indicators.Add("🥤 Puree-friendly");
            if (user.IsGlutenFree && item.IsGlutenFree) indicators.Add("🌾 Gluten-free");
            if (user.IsLactoseIntolerant && item.IsLactoseFree) indicators.Add("🥛 Lactose-free");
            if (user.HasDiabetes && item.IsDiabeticFriendly) indicators.Add("💚 Diabetic-friendly");

            if (indicators.Any())
            {
                item.Description += $" • {string.Join(" • ", indicators)}";
            }
        }

        return item;
    }

    private List<MenuItem> OrganizeMenuByCategory(List<MenuItem> items, CuisineType cuisine)
    {
        var organizedMenu = new List<MenuItem>();

        // Ensure we have items from each major category
        var categories = new[] { MenuCategory.Appetizer, MenuCategory.Soup, MenuCategory.Salad, 
                                MenuCategory.MainCourse, MenuCategory.SideDish, MenuCategory.Dessert, 
                                MenuCategory.Beverage };

        foreach (var category in categories)
        {
            var categoryItems = items.Where(i => i.Category == category).ToList();
            if (categoryItems.Any())
            {
                organizedMenu.AddRange(categoryItems.Take(3)); // Max 3 items per category
            }
            else if (category == MenuCategory.MainCourse)
            {
                // Always ensure main courses exist
                organizedMenu.AddRange(GetFallbackMainCourses(cuisine));
            }
        }

        return organizedMenu.OrderBy(i => i.Category).ThenBy(i => i.Name).ToList();
    }

    private List<MenuItem> GetFallbackMainCourses(CuisineType cuisine)
    {
        return new List<MenuItem>
        {
            new MenuItem
            {
                Name = "Grilled Chicken Breast",
                Description = "Tender grilled chicken with herbs • Gluten-free • Diabetic-friendly",
                Price = 45.99m,
                Category = MenuCategory.MainCourse,
                IsGlutenFree = true,
                IsDiabeticFriendly = true,
                IsLactoseFree = true,
                IsHalal = false,
                PreparationTimeMinutes = 25
            },
            new MenuItem
            {
                Name = "Vegetable Stir Fry",
                Description = "Fresh seasonal vegetables with rice • Vegetarian • Vegan",
                Price = 35.99m,
                Category = MenuCategory.MainCourse,
                IsVegetarian = true,
                IsVegan = true,
                IsGlutenFree = true,
                IsDiabeticFriendly = true,
                IsLactoseFree = true,
                PreparationTimeMinutes = 15
            }
        };
    }

    private List<MenuItem> InitializeBaseMenuItems()
    {
        return new List<MenuItem>
        {
            // Appetizers
            new MenuItem
            {
                Name = "Bruschetta Classica",
                Description = "Toasted bread with fresh tomatoes, basil, and garlic",
                Price = 18.99m,
                Category = MenuCategory.Appetizer,
                SuitableForCuisines = new List<CuisineType> { CuisineType.Italian, CuisineType.Mediterranean },
                IsVegetarian = true,
                IsVegan = true,
                AllergenFree = new List<Allergy> { Allergy.Milk, Allergy.Eggs },
                Ingredients = "Tomatoes, basil, garlic, olive oil, bread",
                PreparationTimeMinutes = 10
            },
            new MenuItem
            {
                Name = "Hummus Plate",
                Description = "Traditional hummus with fresh vegetables and pita bread",
                Price = 22.99m,
                Category = MenuCategory.Appetizer,
                SuitableForCuisines = new List<CuisineType> { CuisineType.Mediterranean, CuisineType.Lebanese },
                IsVegetarian = true,
                IsVegan = true,
                IsGlutenFree = false,
                AllergenFree = new List<Allergy> { Allergy.Milk, Allergy.Eggs },
                Ingredients = "Chickpeas, tahini, olive oil, vegetables",
                PreparationTimeMinutes = 5
            },
            new MenuItem
            {
                Name = "Chicken Satay",
                Description = "Grilled chicken skewers with peanut sauce",
                Price = 28.99m,
                Category = MenuCategory.Appetizer,
                SuitableForCuisines = new List<CuisineType> { CuisineType.Thai },
                IsGlutenFree = true,
                IsHalal = true,
                AllergenFree = new List<Allergy> { Allergy.Milk, Allergy.Wheat },
                Ingredients = "Chicken, peanuts, coconut milk, spices",
                PreparationTimeMinutes = 15
            },

            // Soups
            new MenuItem
            {
                Name = "Vegetable Puree Soup",
                Description = "Smooth blend of seasonal vegetables perfect for easy swallowing",
                Price = 19.99m,
                Category = MenuCategory.Soup,
                IsVegetarian = true,
                IsVegan = true,
                IsGlutenFree = true,
                IsDiabeticFriendly = true,
                IsLactoseFree = true,
                IsPureeFriendly = true,
                AllergenFree = new List<Allergy> { Allergy.Milk, Allergy.Wheat, Allergy.Eggs },
                Ingredients = "Mixed vegetables, vegetable broth, herbs",
                PreparationTimeMinutes = 20
            },
            new MenuItem
            {
                Name = "Lentil Soup",
                Description = "Hearty red lentil soup with aromatic spices",
                Price = 17.99m,
                Category = MenuCategory.Soup,
                IsVegetarian = true,
                IsVegan = true,
                IsGlutenFree = true,
                IsDiabeticFriendly = true,
                IsLactoseFree = true,
                AllergenFree = new List<Allergy> { Allergy.Milk, Allergy.Wheat, Allergy.Eggs },
                Ingredients = "Red lentils, vegetables, spices, vegetable broth",
                PreparationTimeMinutes = 25
            },

            // Salads
            new MenuItem
            {
                Name = "Mediterranean Quinoa Salad",
                Description = "Quinoa with cucumber, tomatoes, olives, and herbs",
                Price = 26.99m,
                Category = MenuCategory.Salad,
                SuitableForCuisines = new List<CuisineType> { CuisineType.Mediterranean },
                IsVegetarian = true,
                IsVegan = true,
                IsGlutenFree = true,
                IsDiabeticFriendly = true,
                IsLactoseFree = true,
                AllergenFree = new List<Allergy> { Allergy.Milk, Allergy.Wheat, Allergy.Eggs },
                Ingredients = "Quinoa, cucumber, tomatoes, olives, herbs, olive oil",
                PreparationTimeMinutes = 15
            },
            new MenuItem
            {
                Name = "Grilled Chicken Caesar",
                Description = "Romaine lettuce with grilled chicken and parmesan",
                Price = 32.99m,
                Category = MenuCategory.Salad,
                IsGlutenFree = true,
                IsDiabeticFriendly = true,
                AllergenFree = new List<Allergy> { Allergy.Wheat },
                Ingredients = "Chicken breast, romaine lettuce, parmesan, olive oil",
                PreparationTimeMinutes = 12
            },

            // Main Courses
            new MenuItem
            {
                Name = "Grilled Salmon",
                Description = "Fresh Atlantic salmon with lemon and herbs",
                Price = 52.99m,
                Category = MenuCategory.MainCourse,
                SuitableForCuisines = new List<CuisineType> { CuisineType.Mediterranean, CuisineType.American },
                IsGlutenFree = true,
                IsDiabeticFriendly = true,
                IsLactoseFree = true,
                AllergenFree = new List<Allergy> { Allergy.Milk, Allergy.Wheat, Allergy.Eggs },
                Ingredients = "Salmon fillet, lemon, herbs, olive oil",
                PreparationTimeMinutes = 20,
                IsPopular = true
            },
            new MenuItem
            {
                Name = "Vegetable Curry",
                Description = "Mixed vegetables in creamy coconut curry sauce",
                Price = 38.99m,
                Category = MenuCategory.MainCourse,
                SuitableForCuisines = new List<CuisineType> { CuisineType.Indian },
                IsVegetarian = true,
                IsVegan = true,
                IsGlutenFree = true,
                IsLactoseFree = true,
                AllergenFree = new List<Allergy> { Allergy.Milk, Allergy.Wheat, Allergy.Eggs },
                Ingredients = "Mixed vegetables, coconut milk, curry spices, rice",
                PreparationTimeMinutes = 25
            },
            new MenuItem
            {
                Name = "Beef Tagine",
                Description = "Slow-cooked beef with vegetables and Moroccan spices",
                Price = 48.99m,
                Category = MenuCategory.MainCourse,
                SuitableForCuisines = new List<CuisineType> { CuisineType.Lebanese },
                IsGlutenFree = true,
                IsDiabeticFriendly = true,
                IsLactoseFree = true,
                IsHalal = true,
                AllergenFree = new List<Allergy> { Allergy.Milk, Allergy.Wheat, Allergy.Eggs },
                Ingredients = "Beef, vegetables, Moroccan spices, broth",
                PreparationTimeMinutes = 40
            },
            new MenuItem
            {
                Name = "Smooth Chicken Puree",
                Description = "Tender chicken breast pureed with vegetables, perfect for swallowing difficulties",
                Price = 41.99m,
                Category = MenuCategory.MainCourse,
                IsGlutenFree = true,
                IsDiabeticFriendly = true,
                IsLactoseFree = true,
                IsPureeFriendly = true,
                AllergenFree = new List<Allergy> { Allergy.Milk, Allergy.Wheat, Allergy.Eggs },
                Ingredients = "Chicken breast, vegetables, chicken broth",
                PreparationTimeMinutes = 30,
                IsSpecialDiet = true
            },

            // Side Dishes
            new MenuItem
            {
                Name = "Steamed Broccoli",
                Description = "Fresh broccoli steamed to perfection",
                Price = 14.99m,
                Category = MenuCategory.SideDish,
                IsVegetarian = true,
                IsVegan = true,
                IsGlutenFree = true,
                IsDiabeticFriendly = true,
                IsLactoseFree = true,
                IsPureeFriendly = true,
                AllergenFree = new List<Allergy> { Allergy.Milk, Allergy.Wheat, Allergy.Eggs, Allergy.TreeNuts },
                Ingredients = "Fresh broccoli",
                PreparationTimeMinutes = 8
            },
            new MenuItem
            {
                Name = "Brown Rice Pilaf",
                Description = "Nutty brown rice with herbs and vegetables",
                Price = 16.99m,
                Category = MenuCategory.SideDish,
                IsVegetarian = true,
                IsVegan = true,
                IsGlutenFree = true,
                IsDiabeticFriendly = true,
                IsLactoseFree = true,
                AllergenFree = new List<Allergy> { Allergy.Milk, Allergy.Wheat, Allergy.Eggs },
                Ingredients = "Brown rice, vegetables, herbs, vegetable broth",
                PreparationTimeMinutes = 25
            },

            // Desserts
            new MenuItem
            {
                Name = "Fresh Fruit Smoothie",
                Description = "Blended seasonal fruits, perfect smooth texture",
                Price = 18.99m,
                Category = MenuCategory.Dessert,
                IsVegetarian = true,
                IsVegan = true,
                IsGlutenFree = true,
                IsDiabeticFriendly = true,
                IsLactoseFree = true,
                IsPureeFriendly = true,
                AllergenFree = new List<Allergy> { Allergy.Milk, Allergy.Wheat, Allergy.Eggs, Allergy.TreeNuts },
                Ingredients = "Seasonal fruits, natural sweetener",
                PreparationTimeMinutes = 5
            },
            new MenuItem
            {
                Name = "Vegan Chocolate Mousse",
                Description = "Rich chocolate mousse made with coconut cream",
                Price = 21.99m,
                Category = MenuCategory.Dessert,
                IsVegetarian = true,
                IsVegan = true,
                IsGlutenFree = true,
                IsLactoseFree = true,
                AllergenFree = new List<Allergy> { Allergy.Milk, Allergy.Wheat, Allergy.Eggs },
                Ingredients = "Dark chocolate, coconut cream, natural sweetener",
                PreparationTimeMinutes = 15
            },

            // Beverages
            new MenuItem
            {
                Name = "Fresh Vegetable Juice",
                Description = "Blend of fresh vegetables, smooth and nutritious",
                Price = 12.99m,
                Category = MenuCategory.Beverage,
                IsVegetarian = true,
                IsVegan = true,
                IsGlutenFree = true,
                IsDiabeticFriendly = true,
                IsLactoseFree = true,
                IsPureeFriendly = true,
                AllergenFree = new List<Allergy> { Allergy.Milk, Allergy.Wheat, Allergy.Eggs, Allergy.TreeNuts },
                Ingredients = "Fresh vegetables, water",
                PreparationTimeMinutes = 5
            },
            new MenuItem
            {
                Name = "Herbal Tea Selection",
                Description = "Choose from chamomile, peppermint, or ginger tea",
                Price = 8.99m,
                Category = MenuCategory.Beverage,
                IsVegetarian = true,
                IsVegan = true,
                IsGlutenFree = true,
                IsDiabeticFriendly = true,
                IsLactoseFree = true,
                IsPureeFriendly = true,
                AllergenFree = new List<Allergy> { Allergy.Milk, Allergy.Wheat, Allergy.Eggs, Allergy.TreeNuts },
                Ingredients = "Herbal tea, hot water",
                PreparationTimeMinutes = 3
            }
        };
    }

    private List<MenuItem> GetItalianMenu()
    {
        return new List<MenuItem>
        {
            // Appetizers
            new MenuItem { Name = "Bruschetta al Pomodoro", Description = "Toasted bread topped with fresh tomatoes and basil", Price = 18m, Category = MenuCategory.Appetizer, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Antipasto Misto", Description = "Selection of cured meats, cheeses, and marinated vegetables", Price = 24m, Category = MenuCategory.Appetizer },
            new MenuItem { Name = "Arancini Siciliani", Description = "Crispy fried rice balls with mozzarella and ragu", Price = 20m, Category = MenuCategory.Appetizer, IsVegetarian = true },
            new MenuItem { Name = "Caprese Salad", Description = "Fresh mozzarella, tomatoes, and basil with balsamic", Price = 16m, Category = MenuCategory.Appetizer, IsVegetarian = true },
            new MenuItem { Name = "Prosciutto e Melone", Description = "Parma ham with fresh cantaloupe", Price = 22m, Category = MenuCategory.Appetizer },
            new MenuItem { Name = "Calamari Fritti", Description = "Golden fried squid rings with marinara sauce", Price = 19m, Category = MenuCategory.Appetizer },
            
            // Soups
            new MenuItem { Name = "Minestrone", Description = "Hearty vegetable soup with pasta and beans", Price = 14m, Category = MenuCategory.Soup, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Zuppa Toscana", Description = "Tuscan soup with sausage, beans, and kale", Price = 16m, Category = MenuCategory.Soup },
            
            // Salads
            new MenuItem { Name = "Panzanella", Description = "Tuscan bread salad with tomatoes and herbs", Price = 15m, Category = MenuCategory.Salad, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Insalata di Rucola", Description = "Arugula salad with parmesan and lemon dressing", Price = 13m, Category = MenuCategory.Salad, IsVegetarian = true },
            
            // Main Courses
            new MenuItem { Name = "Spaghetti Carbonara", Description = "Classic pasta with eggs, cheese, and pancetta", Price = 32m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Risotto ai Funghi", Description = "Creamy mushroom risotto with parmesan", Price = 28m, Category = MenuCategory.MainCourse, IsVegetarian = true },
            new MenuItem { Name = "Osso Buco", Description = "Braised veal shanks with vegetables and white wine", Price = 48m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Lasagna della Casa", Description = "Homemade lasagna with meat sauce and bechamel", Price = 30m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Fettuccine Alfredo", Description = "Creamy pasta with parmesan and butter", Price = 26m, Category = MenuCategory.MainCourse, IsVegetarian = true },
            new MenuItem { Name = "Chicken Parmigiana", Description = "Breaded chicken with marinara and mozzarella", Price = 35m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Veal Marsala", Description = "Tender veal in marsala wine sauce with mushrooms", Price = 42m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Linguine alle Vongole", Description = "Linguine pasta with fresh clams in white wine", Price = 34m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Pizza Margherita", Description = "Classic pizza with tomato, mozzarella, and basil", Price = 24m, Category = MenuCategory.MainCourse, IsVegetarian = true },
            new MenuItem { Name = "Pizza Quattro Stagioni", Description = "Four seasons pizza with artichokes, ham, mushrooms, olives", Price = 28m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Gnocchi Gorgonzola", Description = "Potato dumplings in creamy gorgonzola sauce", Price = 25m, Category = MenuCategory.MainCourse, IsVegetarian = true },
            new MenuItem { Name = "Branzino alla Griglia", Description = "Grilled sea bass with lemon and herbs", Price = 38m, Category = MenuCategory.MainCourse },
            
            // Side Dishes
            new MenuItem { Name = "Polenta Cremosa", Description = "Creamy polenta with parmesan", Price = 12m, Category = MenuCategory.SideDish, IsVegetarian = true },
            new MenuItem { Name = "Verdure Grigliate", Description = "Grilled seasonal vegetables with olive oil", Price = 14m, Category = MenuCategory.SideDish, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Focaccia", Description = "Italian flatbread with rosemary and sea salt", Price = 8m, Category = MenuCategory.SideDish, IsVegetarian = true, IsVegan = true },
            
            // Desserts
            new MenuItem { Name = "Tiramisu", Description = "Coffee-flavored dessert with mascarpone cheese", Price = 16m, Category = MenuCategory.Dessert, IsVegetarian = true },
            new MenuItem { Name = "Panna Cotta", Description = "Silky vanilla custard with berry coulis", Price = 14m, Category = MenuCategory.Dessert, IsVegetarian = true },
            new MenuItem { Name = "Cannoli Siciliani", Description = "Crispy shells filled with sweet ricotta cream", Price = 15m, Category = MenuCategory.Dessert, IsVegetarian = true },
            new MenuItem { Name = "Gelato", Description = "Artisanal Italian ice cream - ask for flavors", Price = 12m, Category = MenuCategory.Dessert, IsVegetarian = true },
            new MenuItem { Name = "Affogato", Description = "Vanilla gelato 'drowned' in hot espresso", Price = 13m, Category = MenuCategory.Dessert, IsVegetarian = true },
            
            // Beverages
            new MenuItem { Name = "Espresso", Description = "Traditional Italian coffee", Price = 8m, Category = MenuCategory.Beverage, IsVegan = true },
            new MenuItem { Name = "Cappuccino", Description = "Espresso with steamed milk and foam", Price = 10m, Category = MenuCategory.Beverage, IsVegetarian = true },
            new MenuItem { Name = "Aperol Spritz", Description = "Italian aperitif with Aperol, Prosecco, and soda", Price = 15m, Category = MenuCategory.Beverage },
            new MenuItem { Name = "Limoncello", Description = "Traditional lemon liqueur from Sorrento", Price = 12m, Category = MenuCategory.Beverage },
            new MenuItem { Name = "Chianti Classico", Description = "Full-bodied Tuscan red wine", Price = 18m, Category = MenuCategory.Beverage }
        };
    }

    private List<MenuItem> GetChineseMenu()
    {
        return new List<MenuItem>
        {
            // Appetizers
            new MenuItem { Name = "Spring Rolls", Description = "Crispy vegetable rolls with sweet and sour sauce", Price = 15m, Category = MenuCategory.Appetizer, IsVegetarian = true },
            new MenuItem { Name = "Dim Sum Platter", Description = "Selection of steamed dumplings", Price = 22m, Category = MenuCategory.Appetizer },
            new MenuItem { Name = "Peking Duck Pancakes", Description = "Crispy duck with pancakes, cucumber, and hoisin sauce", Price = 28m, Category = MenuCategory.Appetizer },
            new MenuItem { Name = "Pot Stickers", Description = "Pan-fried pork and cabbage dumplings", Price = 18m, Category = MenuCategory.Appetizer },
            new MenuItem { Name = "Chicken Wings", Description = "Crispy wings with Chinese five-spice", Price = 16m, Category = MenuCategory.Appetizer },
            new MenuItem { Name = "Prawn Crackers", Description = "Light and crispy prawn-flavored crackers", Price = 8m, Category = MenuCategory.Appetizer },
            
            // Soups
            new MenuItem { Name = "Hot and Sour Soup", Description = "Traditional soup with tofu, mushrooms, and white pepper", Price = 12m, Category = MenuCategory.Soup, IsVegetarian = true },
            new MenuItem { Name = "Wonton Soup", Description = "Pork and shrimp wontons in clear broth", Price = 14m, Category = MenuCategory.Soup },
            new MenuItem { Name = "Egg Drop Soup", Description = "Silky soup with beaten eggs and green onions", Price = 10m, Category = MenuCategory.Soup, IsVegetarian = true },
            
            // Salads
            new MenuItem { Name = "Chinese Chicken Salad", Description = "Mixed greens with crispy noodles and sesame dressing", Price = 16m, Category = MenuCategory.Salad },
            new MenuItem { Name = "Cucumber Salad", Description = "Refreshing cucumbers with rice vinegar and chili oil", Price = 12m, Category = MenuCategory.Salad, IsVegetarian = true, IsVegan = true },
            
            // Main Courses
            new MenuItem { Name = "Sweet and Sour Pork", Description = "Battered pork with pineapple and bell peppers", Price = 34m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Kung Pao Chicken", Description = "Spicy stir-fried chicken with peanuts", Price = 32m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Ma Po Tofu", Description = "Silky tofu in spicy Sichuan sauce", Price = 26m, Category = MenuCategory.MainCourse, IsVegetarian = true },
            new MenuItem { Name = "General Tso's Chicken", Description = "Crispy chicken in sweet and tangy sauce", Price = 30m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Beef and Broccoli", Description = "Tender beef with fresh broccoli in brown sauce", Price = 32m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Orange Chicken", Description = "Battered chicken with fresh orange glaze", Price = 29m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Szechuan Fish", Description = "Spicy fish fillets with vegetables in chili sauce", Price = 36m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Mongolian Beef", Description = "Sliced beef with onions and scallions", Price = 35m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Cashew Chicken", Description = "Diced chicken with cashews and vegetables", Price = 31m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Vegetable Lo Mein", Description = "Soft noodles with mixed vegetables", Price = 24m, Category = MenuCategory.MainCourse, IsVegetarian = true },
            new MenuItem { Name = "Honey Walnut Prawns", Description = "Crispy prawns with candied walnuts", Price = 38m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Char Siu Pork", Description = "Cantonese BBQ pork with honey glaze", Price = 33m, Category = MenuCategory.MainCourse },
            
            // Side Dishes
            new MenuItem { Name = "Fried Rice", Description = "Wok-fried rice with vegetables and soy sauce", Price = 18m, Category = MenuCategory.SideDish, IsVegetarian = true },
            new MenuItem { Name = "Chow Mein", Description = "Stir-fried crispy noodles with vegetables", Price = 16m, Category = MenuCategory.SideDish, IsVegetarian = true },
            new MenuItem { Name = "Steamed White Rice", Description = "Perfectly steamed jasmine rice", Price = 6m, Category = MenuCategory.SideDish, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Chinese Greens", Description = "Stir-fried bok choy with garlic", Price = 14m, Category = MenuCategory.SideDish, IsVegetarian = true, IsVegan = true },
            
            // Desserts
            new MenuItem { Name = "Fried Ice Cream", Description = "Vanilla ice cream in crispy coating", Price = 12m, Category = MenuCategory.Dessert, IsVegetarian = true },
            new MenuItem { Name = "Sesame Balls", Description = "Deep-fried glutinous rice balls with red bean paste", Price = 10m, Category = MenuCategory.Dessert, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Fortune Cookies", Description = "Traditional cookies with fortune messages", Price = 6m, Category = MenuCategory.Dessert, IsVegetarian = true },
            
            // Beverages
            new MenuItem { Name = "Jasmine Tea", Description = "Traditional Chinese green tea", Price = 6m, Category = MenuCategory.Beverage, IsVegan = true },
            new MenuItem { Name = "Oolong Tea", Description = "Semi-fermented traditional tea", Price = 7m, Category = MenuCategory.Beverage, IsVegan = true },
            new MenuItem { Name = "Chinese Rice Wine", Description = "Traditional Shaoxing cooking wine", Price = 12m, Category = MenuCategory.Beverage },
            new MenuItem { Name = "Lychee Juice", Description = "Sweet tropical fruit juice", Price = 8m, Category = MenuCategory.Beverage, IsVegetarian = true, IsVegan = true }
        };
    }

    private List<MenuItem> GetIndianMenu()
    {
        return new List<MenuItem>
        {
            // Appetizers
            new MenuItem { Name = "Samosas", Description = "Crispy pastries filled with spiced potatoes", Price = 16m, Category = MenuCategory.Appetizer, IsVegetarian = true },
            new MenuItem { Name = "Chicken Tikka", Description = "Marinated grilled chicken pieces", Price = 20m, Category = MenuCategory.Appetizer },
            new MenuItem { Name = "Pakoras", Description = "Mixed vegetable fritters with mint chutney", Price = 14m, Category = MenuCategory.Appetizer, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Aloo Tikki", Description = "Spiced potato patties with yogurt and chutneys", Price = 12m, Category = MenuCategory.Appetizer, IsVegetarian = true },
            new MenuItem { Name = "Tandoori Wings", Description = "Chicken wings marinated in yogurt and spices", Price = 18m, Category = MenuCategory.Appetizer },
            new MenuItem { Name = "Paneer Tikka", Description = "Grilled cottage cheese with bell peppers", Price = 19m, Category = MenuCategory.Appetizer, IsVegetarian = true },
            
            // Soups
            new MenuItem { Name = "Mulligatawny Soup", Description = "Spiced lentil soup with coconut milk", Price = 12m, Category = MenuCategory.Soup, IsVegetarian = true },
            new MenuItem { Name = "Tomato Shorba", Description = "Spiced tomato soup with fresh herbs", Price = 10m, Category = MenuCategory.Soup, IsVegetarian = true, IsVegan = true },
            
            // Salads
            new MenuItem { Name = "Kachumber Salad", Description = "Fresh cucumber and tomato salad with lemon", Price = 11m, Category = MenuCategory.Salad, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Raita", Description = "Yogurt salad with cucumber and mint", Price = 9m, Category = MenuCategory.Salad, IsVegetarian = true },
            
            // Main Courses
            new MenuItem { Name = "Butter Chicken", Description = "Creamy tomato-based chicken curry", Price = 36m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Dal Makhani", Description = "Rich black lentil curry with cream", Price = 24m, Category = MenuCategory.MainCourse, IsVegetarian = true },
            new MenuItem { Name = "Biryani", Description = "Fragrant basmati rice with spices and meat", Price = 38m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Chicken Tikka Masala", Description = "Grilled chicken in creamy tomato sauce", Price = 34m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Palak Paneer", Description = "Cottage cheese in creamy spinach curry", Price = 26m, Category = MenuCategory.MainCourse, IsVegetarian = true },
            new MenuItem { Name = "Rogan Josh", Description = "Tender lamb in aromatic Kashmiri curry", Price = 40m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Chana Masala", Description = "Chickpea curry with onions and tomatoes", Price = 22m, Category = MenuCategory.MainCourse, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Tandoori Chicken", Description = "Whole chicken marinated and roasted in tandoor", Price = 42m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Vindaloo", Description = "Spicy Goan curry with vinegar and chilies", Price = 35m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Saag Chicken", Description = "Chicken curry with spinach and mustard greens", Price = 33m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Aloo Gobi", Description = "Cauliflower and potato curry with turmeric", Price = 20m, Category = MenuCategory.MainCourse, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Fish Curry", Description = "South Indian fish curry with coconut", Price = 37m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Korma", Description = "Mild curry with cashews and cream", Price = 32m, Category = MenuCategory.MainCourse, IsVegetarian = true },
            
            // Side Dishes
            new MenuItem { Name = "Naan Bread", Description = "Traditional oven-baked flatbread", Price = 8m, Category = MenuCategory.SideDish, IsVegetarian = true },
            new MenuItem { Name = "Garlic Naan", Description = "Naan bread with roasted garlic and cilantro", Price = 10m, Category = MenuCategory.SideDish, IsVegetarian = true },
            new MenuItem { Name = "Roti", Description = "Whole wheat flatbread", Price = 6m, Category = MenuCategory.SideDish, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Basmati Rice", Description = "Aromatic long-grain rice", Price = 8m, Category = MenuCategory.SideDish, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Papadum", Description = "Crispy lentil wafers", Price = 5m, Category = MenuCategory.SideDish, IsVegetarian = true, IsVegan = true },
            
            // Desserts
            new MenuItem { Name = "Gulab Jamun", Description = "Sweet milk dumplings in rose syrup", Price = 12m, Category = MenuCategory.Dessert, IsVegetarian = true },
            new MenuItem { Name = "Rasmalai", Description = "Cottage cheese dumplings in cardamom cream", Price = 14m, Category = MenuCategory.Dessert, IsVegetarian = true },
            new MenuItem { Name = "Kheer", Description = "Rice pudding with cardamom and nuts", Price = 10m, Category = MenuCategory.Dessert, IsVegetarian = true },
            new MenuItem { Name = "Kulfi", Description = "Traditional Indian ice cream with pistachios", Price = 11m, Category = MenuCategory.Dessert, IsVegetarian = true },
            
            // Beverages
            new MenuItem { Name = "Masala Chai", Description = "Spiced tea with milk", Price = 7m, Category = MenuCategory.Beverage, IsVegetarian = true },
            new MenuItem { Name = "Lassi", Description = "Yogurt drink - sweet or salty", Price = 8m, Category = MenuCategory.Beverage, IsVegetarian = true },
            new MenuItem { Name = "Mango Lassi", Description = "Sweet yogurt drink with fresh mango", Price = 9m, Category = MenuCategory.Beverage, IsVegetarian = true },
            new MenuItem { Name = "Fresh Lime Soda", Description = "Sparkling water with fresh lime and salt", Price = 6m, Category = MenuCategory.Beverage, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Kingfisher Beer", Description = "Indian lager beer", Price = 12m, Category = MenuCategory.Beverage }
        };
    }

    private List<MenuItem> GetFrenchMenu()
    {
        return new List<MenuItem>
        {
            // Appetizers
            new MenuItem { Name = "Escargots de Bourgogne", Description = "Burgundy snails with garlic butter", Price = 28m, Category = MenuCategory.Appetizer },
            new MenuItem { Name = "French Onion Soup", Description = "Classic onion soup with gruyere cheese", Price = 18m, Category = MenuCategory.Soup, IsVegetarian = true },
            
            // Main Courses
            new MenuItem { Name = "Coq au Vin", Description = "Chicken braised in red wine with mushrooms", Price = 45m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Beef Bourguignon", Description = "Slow-cooked beef in burgundy wine", Price = 52m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Ratatouille", Description = "Provençal vegetable stew", Price = 28m, Category = MenuCategory.MainCourse, IsVegetarian = true },
            
            // Desserts
            new MenuItem { Name = "Crème Brûlée", Description = "Vanilla custard with caramelized sugar", Price = 18m, Category = MenuCategory.Dessert, IsVegetarian = true },
            
            // Beverages
            new MenuItem { Name = "French Wine", Description = "Selection of regional wines", Price = 25m, Category = MenuCategory.Beverage }
        };
    }

    private List<MenuItem> GetMexicanMenu()
    {
        return new List<MenuItem>
        {
            // Appetizers
            new MenuItem { Name = "Guacamole & Chips", Description = "Fresh avocado dip with tortilla chips", Price = 14m, Category = MenuCategory.Appetizer, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Quesadillas", Description = "Grilled tortillas with cheese and peppers", Price = 18m, Category = MenuCategory.Appetizer, IsVegetarian = true },
            
            // Main Courses
            new MenuItem { Name = "Tacos al Pastor", Description = "Marinated pork tacos with pineapple", Price = 24m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Chicken Enchiladas", Description = "Rolled tortillas with chicken and green sauce", Price = 28m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Vegetarian Burrito", Description = "Black beans, rice, and vegetables in a flour tortilla", Price = 22m, Category = MenuCategory.MainCourse, IsVegetarian = true },
            
            // Side Dishes
            new MenuItem { Name = "Mexican Rice", Description = "Seasoned rice with tomatoes and spices", Price = 12m, Category = MenuCategory.SideDish, IsVegetarian = true, IsVegan = true },
            
            // Beverages
            new MenuItem { Name = "Horchata", Description = "Sweet rice and cinnamon drink", Price = 8m, Category = MenuCategory.Beverage, IsVegetarian = true }
        };
    }

    private List<MenuItem> GetThaiMenu()
    {
        return new List<MenuItem>
        {
            // Appetizers
            new MenuItem { Name = "Fresh Spring Rolls", Description = "Rice paper rolls with vegetables and herbs", Price = 14m, Category = MenuCategory.Appetizer, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Chicken Satay", Description = "Grilled chicken skewers with peanut sauce", Price = 18m, Category = MenuCategory.Appetizer },
            new MenuItem { Name = "Thai Fish Cakes", Description = "Spicy fish patties with cucumber relish", Price = 16m, Category = MenuCategory.Appetizer },
            new MenuItem { Name = "Coconut Prawns", Description = "Crispy prawns with sweet chili sauce", Price = 20m, Category = MenuCategory.Appetizer },
            new MenuItem { Name = "Vegetable Spring Rolls", Description = "Crispy fried rolls with sweet and sour sauce", Price = 12m, Category = MenuCategory.Appetizer, IsVegetarian = true, IsVegan = true },
            
            // Soups
            new MenuItem { Name = "Tom Yum Soup", Description = "Spicy and sour soup with shrimp", Price = 16m, Category = MenuCategory.Soup },
            new MenuItem { Name = "Tom Kha Gai", Description = "Coconut chicken soup with galangal", Price = 15m, Category = MenuCategory.Soup },
            new MenuItem { Name = "Thai Vegetable Soup", Description = "Clear broth with mixed vegetables and tofu", Price = 12m, Category = MenuCategory.Soup, IsVegetarian = true, IsVegan = true },
            
            // Salads
            new MenuItem { Name = "Som Tam", Description = "Spicy green papaya salad with lime dressing", Price = 14m, Category = MenuCategory.Salad, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Larb Gai", Description = "Thai chicken salad with mint and chili", Price = 16m, Category = MenuCategory.Salad },
            new MenuItem { Name = "Thai Beef Salad", Description = "Spicy beef salad with cucumber and herbs", Price = 18m, Category = MenuCategory.Salad },
            
            // Main Courses
            new MenuItem { Name = "Pad Thai", Description = "Stir-fried rice noodles with tamarind sauce", Price = 26m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Green Curry", Description = "Coconut curry with Thai basil and vegetables", Price = 28m, Category = MenuCategory.MainCourse, IsVegetarian = true },
            new MenuItem { Name = "Massaman Beef", Description = "Rich curry with tender beef and potatoes", Price = 34m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Red Curry", Description = "Spicy coconut curry with bamboo shoots", Price = 30m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Panang Curry", Description = "Thick curry with peanuts and kaffir lime", Price = 32m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Pad Kra Pao", Description = "Stir-fried holy basil with ground meat", Price = 24m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Thai Fried Rice", Description = "Jasmine rice with vegetables and egg", Price = 22m, Category = MenuCategory.MainCourse, IsVegetarian = true },
            new MenuItem { Name = "Drunken Noodles", Description = "Spicy stir-fried noodles with basil", Price = 25m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Cashew Chicken", Description = "Stir-fried chicken with cashews and chili", Price = 28m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Pineapple Fried Rice", Description = "Fried rice served in fresh pineapple", Price = 26m, Category = MenuCategory.MainCourse, IsVegetarian = true },
            new MenuItem { Name = "Thai Fish Curry", Description = "Red curry with fish and Thai eggplant", Price = 33m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Mango Sticky Rice", Description = "Sweet dessert with coconut rice and mango", Price = 15m, Category = MenuCategory.Dessert, IsVegetarian = true, IsVegan = true },
            
            // Side Dishes
            new MenuItem { Name = "Jasmine Rice", Description = "Fragrant Thai rice", Price = 8m, Category = MenuCategory.SideDish, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Coconut Rice", Description = "Rice cooked in coconut milk", Price = 10m, Category = MenuCategory.SideDish, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Thai Vegetables", Description = "Stir-fried mixed vegetables with garlic", Price = 12m, Category = MenuCategory.SideDish, IsVegetarian = true, IsVegan = true },
            
            // Desserts
            new MenuItem { Name = "Thai Coconut Ice Cream", Description = "Creamy ice cream with coconut and peanuts", Price = 12m, Category = MenuCategory.Dessert, IsVegetarian = true },
            new MenuItem { Name = "Fried Banana", Description = "Crispy banana fritters with honey", Price = 10m, Category = MenuCategory.Dessert, IsVegetarian = true, IsVegan = true },
            
            // Beverages
            new MenuItem { Name = "Thai Iced Tea", Description = "Sweet tea with condensed milk", Price = 7m, Category = MenuCategory.Beverage, IsVegetarian = true },
            new MenuItem { Name = "Thai Iced Coffee", Description = "Strong coffee with sweetened condensed milk", Price = 8m, Category = MenuCategory.Beverage, IsVegetarian = true },
            new MenuItem { Name = "Coconut Water", Description = "Fresh young coconut water", Price = 6m, Category = MenuCategory.Beverage, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Thai Basil Lemonade", Description = "Fresh lemonade with Thai basil", Price = 7m, Category = MenuCategory.Beverage, IsVegetarian = true, IsVegan = true }
        };
    }

    private List<MenuItem> GetJapaneseMenu()
    {
        return new List<MenuItem>
        {
            // Appetizers
            new MenuItem { Name = "Edamame", Description = "Steamed young soybeans with sea salt", Price = 12m, Category = MenuCategory.Appetizer, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Gyoza", Description = "Pan-fried pork and vegetable dumplings", Price = 18m, Category = MenuCategory.Appetizer },
            
            // Main Courses
            new MenuItem { Name = "Sushi Platter", Description = "Selection of fresh nigiri and maki rolls", Price = 42m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Chicken Teriyaki", Description = "Grilled chicken with sweet teriyaki glaze", Price = 32m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Vegetable Tempura", Description = "Lightly battered and fried seasonal vegetables", Price = 24m, Category = MenuCategory.MainCourse, IsVegetarian = true },
            
            // Soups
            new MenuItem { Name = "Miso Soup", Description = "Traditional soybean paste soup", Price = 8m, Category = MenuCategory.Soup, IsVegetarian = true, IsVegan = true },
            
            // Beverages
            new MenuItem { Name = "Green Tea", Description = "Premium Japanese green tea", Price = 6m, Category = MenuCategory.Beverage, IsVegan = true }
        };
    }

    private List<MenuItem> GetRomanianMenu()
    {
        return new List<MenuItem>
        {
            // Appetizers
            new MenuItem { Name = "Mici", Description = "Grilled meat rolls with mustard", Price = 20m, Category = MenuCategory.Appetizer },
            new MenuItem { Name = "Salată de Icre", Description = "Fish roe salad with onions", Price = 16m, Category = MenuCategory.Appetizer },
            
            // Main Courses
            new MenuItem { Name = "Ciorbă de Burtă", Description = "Traditional tripe soup with sour cream", Price = 22m, Category = MenuCategory.Soup },
            new MenuItem { Name = "Sarmale", Description = "Cabbage rolls stuffed with meat and rice", Price = 28m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Mămăligă cu Brânză", Description = "Polenta with cheese and sour cream", Price = 18m, Category = MenuCategory.MainCourse, IsVegetarian = true },
            
            // Side Dishes
            new MenuItem { Name = "Murături", Description = "Traditional pickled vegetables", Price = 10m, Category = MenuCategory.SideDish, IsVegetarian = true, IsVegan = true },
            
            // Beverages
            new MenuItem { Name = "Țuică", Description = "Traditional Romanian plum brandy", Price = 15m, Category = MenuCategory.Beverage }
        };
    }

    private List<MenuItem> GetGenericMenu()
    {
        return new List<MenuItem>
        {
            // Appetizers
            new MenuItem { Name = "Chicken Wings", Description = "Buffalo or BBQ style wings with celery", Price = 18m, Category = MenuCategory.Appetizer },
            new MenuItem { Name = "Loaded Nachos", Description = "Tortilla chips with cheese, jalapeños, and sour cream", Price = 16m, Category = MenuCategory.Appetizer, IsVegetarian = true },
            new MenuItem { Name = "Mozzarella Sticks", Description = "Crispy breaded mozzarella with marinara sauce", Price = 14m, Category = MenuCategory.Appetizer, IsVegetarian = true },
            new MenuItem { Name = "Spinach Artichoke Dip", Description = "Creamy dip served with tortilla chips", Price = 15m, Category = MenuCategory.Appetizer, IsVegetarian = true },
            
            // Soups
            new MenuItem { Name = "Tomato Basil Soup", Description = "Creamy tomato soup with fresh basil", Price = 12m, Category = MenuCategory.Soup, IsVegetarian = true },
            new MenuItem { Name = "Chicken Noodle Soup", Description = "Classic comfort soup with vegetables", Price = 14m, Category = MenuCategory.Soup },
            
            // Salads
            new MenuItem { Name = "Caesar Salad", Description = "Romaine lettuce with parmesan and croutons", Price = 16m, Category = MenuCategory.Salad, IsVegetarian = true },
            new MenuItem { Name = "Garden Salad", Description = "Mixed greens with cucumber, tomato, and choice of dressing", Price = 14m, Category = MenuCategory.Salad, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Greek Salad", Description = "Tomatoes, olives, feta cheese, and cucumber", Price = 17m, Category = MenuCategory.Salad, IsVegetarian = true },
            new MenuItem { Name = "Cobb Salad", Description = "Mixed greens with bacon, egg, blue cheese, and chicken", Price = 19m, Category = MenuCategory.Salad },
            
            // Main Courses
            new MenuItem { Name = "Grilled Chicken Breast", Description = "Herb-seasoned chicken with vegetables", Price = 32m, Category = MenuCategory.MainCourse, IsGlutenFree = true },
            new MenuItem { Name = "Fish & Chips", Description = "Beer-battered fish with french fries", Price = 28m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Vegetable Stir Fry", Description = "Mixed vegetables with soy sauce", Price = 22m, Category = MenuCategory.MainCourse, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Classic Burger", Description = "Beef patty with lettuce, tomato, and fries", Price = 24m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "BBQ Ribs", Description = "Slow-cooked pork ribs with barbecue sauce", Price = 36m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Grilled Salmon", Description = "Atlantic salmon with lemon herb butter", Price = 34m, Category = MenuCategory.MainCourse, IsGlutenFree = true },
            new MenuItem { Name = "Chicken Parmesan", Description = "Breaded chicken with marinara and mozzarella", Price = 30m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Steak Frites", Description = "Grilled steak with seasoned french fries", Price = 38m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Vegetarian Pasta", Description = "Penne with roasted vegetables and pesto", Price = 26m, Category = MenuCategory.MainCourse, IsVegetarian = true },
            new MenuItem { Name = "Fish Tacos", Description = "Grilled fish with cabbage slaw and chipotle mayo", Price = 25m, Category = MenuCategory.MainCourse },
            new MenuItem { Name = "Mushroom Risotto", Description = "Creamy arborio rice with wild mushrooms", Price = 28m, Category = MenuCategory.MainCourse, IsVegetarian = true },
            
            // Side Dishes
            new MenuItem { Name = "French Fries", Description = "Crispy seasoned potato fries", Price = 8m, Category = MenuCategory.SideDish, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Onion Rings", Description = "Beer-battered onion rings", Price = 10m, Category = MenuCategory.SideDish, IsVegetarian = true },
            new MenuItem { Name = "Coleslaw", Description = "Creamy cabbage and carrot salad", Price = 6m, Category = MenuCategory.SideDish, IsVegetarian = true },
            new MenuItem { Name = "Garlic Bread", Description = "Toasted bread with garlic butter", Price = 7m, Category = MenuCategory.SideDish, IsVegetarian = true },
            
            // Desserts
            new MenuItem { Name = "Chocolate Cake", Description = "Rich chocolate layer cake", Price = 14m, Category = MenuCategory.Dessert, IsVegetarian = true },
            new MenuItem { Name = "Cheesecake", Description = "New York style cheesecake with berry sauce", Price = 16m, Category = MenuCategory.Dessert, IsVegetarian = true },
            new MenuItem { Name = "Apple Pie", Description = "Traditional apple pie with vanilla ice cream", Price = 13m, Category = MenuCategory.Dessert, IsVegetarian = true },
            new MenuItem { Name = "Ice Cream Sundae", Description = "Vanilla ice cream with chocolate sauce and nuts", Price = 11m, Category = MenuCategory.Dessert, IsVegetarian = true },
            
            // Beverages
            new MenuItem { Name = "House Wine", Description = "Red or white wine selection", Price = 15m, Category = MenuCategory.Beverage },
            new MenuItem { Name = "Craft Beer", Description = "Local brewery selection", Price = 12m, Category = MenuCategory.Beverage },
            new MenuItem { Name = "Iced Tea", Description = "Fresh brewed sweet or unsweetened tea", Price = 5m, Category = MenuCategory.Beverage, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Lemonade", Description = "Fresh squeezed lemonade", Price = 6m, Category = MenuCategory.Beverage, IsVegetarian = true, IsVegan = true }
        };
    }

    private List<MenuItem> GetCommonItems()
    {
        var commonItems = new List<MenuItem>
        {
            // Common beverages that most restaurants have
            new MenuItem { Name = "Coca Cola", Description = "Classic soft drink", Price = 6m, Category = MenuCategory.Beverage, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Fresh Orange Juice", Description = "Freshly squeezed orange juice", Price = 8m, Category = MenuCategory.Beverage, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Still Water", Description = "Premium bottled water", Price = 4m, Category = MenuCategory.Beverage, IsVegetarian = true, IsVegan = true },
            new MenuItem { Name = "Coffee", Description = "Freshly brewed coffee", Price = 6m, Category = MenuCategory.Beverage, IsVegetarian = true, IsVegan = true }
        };

        // Add specialized dietary items
        commonItems.AddRange(GetSpecializedDietaryItems());
        return commonItems;
    }

    private List<MenuItem> GetSpecializedDietaryItems()
    {
        var specializedItems = new List<MenuItem>();
        
        // Add items for each dietary type, health condition, and allergy
        specializedItems.AddRange(GetVegetarianSpecializedItems());
        specializedItems.AddRange(GetVeganSpecializedItems());
        specializedItems.AddRange(GetGlutenFreeSpecializedItems());
        specializedItems.AddRange(GetDiabeticFriendlyItems());
        specializedItems.AddRange(GetLactoseFreeItems());
        specializedItems.AddRange(GetHalalItems());
        specializedItems.AddRange(GetKosherItems());
        specializedItems.AddRange(GetPureeFriendlyItems());
        specializedItems.AddRange(GetAllergenFreeItems());
        specializedItems.AddRange(GetPaleoDietItems());
        specializedItems.AddRange(GetKetoDietItems());
        specializedItems.AddRange(GetMediterraneanDietItems());
        specializedItems.AddRange(GetLowCarbItems());
        
        return specializedItems;
    }

    private List<MenuItem> GetVegetarianSpecializedItems()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Quinoa Buddha Bowl", Description = "Protein-rich quinoa with roasted vegetables and tahini dressing", Price = 28m, Category = MenuCategory.MainCourse, IsVegetarian = true, IsVegan = true, IsGlutenFree = true, IsDiabeticFriendly = true },
            new MenuItem { Name = "Caprese Stuffed Portobello", Description = "Grilled portobello mushroom stuffed with fresh mozzarella and tomatoes", Price = 26m, Category = MenuCategory.MainCourse, IsVegetarian = true, IsGlutenFree = true },
            new MenuItem { Name = "Vegetarian Shepherd's Pie", Description = "Lentil and vegetable filling topped with creamy mashed potatoes", Price = 24m, Category = MenuCategory.MainCourse, IsVegetarian = true },
            new MenuItem { Name = "Spinach and Ricotta Cannelloni", Description = "Fresh pasta tubes filled with spinach and ricotta in tomato sauce", Price = 22m, Category = MenuCategory.MainCourse, IsVegetarian = true },
            new MenuItem { Name = "Mediterranean Vegetable Wrap", Description = "Grilled vegetables with hummus in a whole wheat tortilla", Price = 18m, Category = MenuCategory.MainCourse, IsVegetarian = true, IsVegan = true }
        };
    }

    private List<MenuItem> GetVeganSpecializedItems()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Jackfruit Carnitas Tacos", Description = "Seasoned jackfruit with cilantro lime slaw in corn tortillas", Price = 24m, Category = MenuCategory.MainCourse, IsVegan = true, IsGlutenFree = true },
            new MenuItem { Name = "Cashew Alfredo Pasta", Description = "Creamy cashew-based sauce over fresh pasta with nutritional yeast", Price = 26m, Category = MenuCategory.MainCourse, IsVegan = true },
            new MenuItem { Name = "Beetroot and Walnut Burger", Description = "House-made patty with avocado and vegan mayo on a brioche bun", Price = 22m, Category = MenuCategory.MainCourse, IsVegan = true },
            new MenuItem { Name = "Coconut Curry Lentil Soup", Description = "Red lentils in aromatic coconut curry broth with vegetables", Price = 16m, Category = MenuCategory.Soup, IsVegan = true, IsGlutenFree = true },
            new MenuItem { Name = "Acai Berry Bowl", Description = "Frozen acai topped with granola, fresh berries, and coconut flakes", Price = 18m, Category = MenuCategory.Dessert, IsVegan = true, IsGlutenFree = true }
        };
    }

    private List<MenuItem> GetGlutenFreeSpecializedItems()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Zucchini Noodle Carbonara", Description = "Spiralized zucchini with creamy egg sauce and crispy pancetta", Price = 28m, Category = MenuCategory.MainCourse, IsGlutenFree = true },
            new MenuItem { Name = "Cauliflower Crust Pizza", Description = "Gluten-free cauliflower base with fresh mozzarella and basil", Price = 24m, Category = MenuCategory.MainCourse, IsGlutenFree = true, IsVegetarian = true },
            new MenuItem { Name = "Almond-Crusted Salmon", Description = "Fresh salmon fillet with almond crust and lemon butter sauce", Price = 32m, Category = MenuCategory.MainCourse, IsGlutenFree = true },
            new MenuItem { Name = "Quinoa Stuffed Bell Peppers", Description = "Colorful peppers filled with quinoa, vegetables, and herbs", Price = 22m, Category = MenuCategory.MainCourse, IsGlutenFree = true, IsVegetarian = true },
            new MenuItem { Name = "Flourless Chocolate Cake", Description = "Rich, decadent chocolate cake made with almond flour", Price = 16m, Category = MenuCategory.Dessert, IsGlutenFree = true, IsVegetarian = true }
        };
    }

    private List<MenuItem> GetDiabeticFriendlyItems()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Grilled Chicken with Steamed Broccoli", Description = "Lean protein with fiber-rich vegetables, no added sugars", Price = 26m, Category = MenuCategory.MainCourse, IsDiabeticFriendly = true, IsGlutenFree = true },
            new MenuItem { Name = "Baked Cod with Herbs", Description = "Fresh white fish with Mediterranean herbs and olive oil", Price = 28m, Category = MenuCategory.MainCourse, IsDiabeticFriendly = true, IsGlutenFree = true },
            new MenuItem { Name = "Cauliflower Rice Stir-Fry", Description = "Low-carb cauliflower rice with mixed vegetables and tofu", Price = 20m, Category = MenuCategory.MainCourse, IsDiabeticFriendly = true, IsVegetarian = true, IsGlutenFree = true },
            new MenuItem { Name = "Greek Salad with Grilled Shrimp", Description = "Fresh vegetables with lean protein and olive oil dressing", Price = 24m, Category = MenuCategory.Salad, IsDiabeticFriendly = true, IsGlutenFree = true },
            new MenuItem { Name = "Sugar-Free Berry Parfait", Description = "Greek yogurt layered with fresh berries and nuts", Price = 14m, Category = MenuCategory.Dessert, IsDiabeticFriendly = true, IsVegetarian = true, IsGlutenFree = true }
        };
    }

    private List<MenuItem> GetLactoseFreeItems()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Coconut Milk Risotto", Description = "Creamy risotto made with coconut milk and wild mushrooms", Price = 26m, Category = MenuCategory.MainCourse, IsLactoseFree = true, IsVegetarian = true },
            new MenuItem { Name = "Dairy-Free Fettuccine Alfredo", Description = "Rich cashew cream sauce over fresh fettuccine pasta", Price = 24m, Category = MenuCategory.MainCourse, IsLactoseFree = true, IsVegetarian = true },
            new MenuItem { Name = "Grilled Salmon with Olive Tapenade", Description = "Fresh salmon with Mediterranean olive and herb spread", Price = 30m, Category = MenuCategory.MainCourse, IsLactoseFree = true, IsGlutenFree = true },
            new MenuItem { Name = "Oat Milk Smoothie Bowl", Description = "Tropical fruits blended with oat milk, topped with granola", Price = 16m, Category = MenuCategory.Dessert, IsLactoseFree = true, IsVegan = true },
            new MenuItem { Name = "Dark Chocolate Avocado Mousse", Description = "Rich, creamy mousse made with avocado and coconut cream", Price = 14m, Category = MenuCategory.Dessert, IsLactoseFree = true, IsVegan = true }
        };
    }

    private List<MenuItem> GetHalalItems()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Halal Lamb Kebab Platter", Description = "Grilled halal lamb skewers with rice pilaf and vegetables", Price = 32m, Category = MenuCategory.MainCourse, IsHalal = true },
            new MenuItem { Name = "Moroccan Chicken Tagine", Description = "Slow-cooked halal chicken with apricots and Middle Eastern spices", Price = 28m, Category = MenuCategory.MainCourse, IsHalal = true },
            new MenuItem { Name = "Halal Beef Shawarma Bowl", Description = "Seasoned halal beef over rice with tahini sauce and fresh vegetables", Price = 26m, Category = MenuCategory.MainCourse, IsHalal = true },
            new MenuItem { Name = "Turkish Lentil Soup", Description = "Traditional red lentil soup with Middle Eastern spices", Price = 14m, Category = MenuCategory.Soup, IsHalal = true, IsVegan = true },
            new MenuItem { Name = "Baklava with Honey", Description = "Flaky pastry layers with nuts and honey syrup", Price = 12m, Category = MenuCategory.Dessert, IsHalal = true, IsVegetarian = true }
        };
    }

    private List<MenuItem> GetKosherItems()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Kosher Beef Brisket", Description = "Slow-braised kosher beef with root vegetables and herbs", Price = 34m, Category = MenuCategory.MainCourse, IsKosher = true },
            new MenuItem { Name = "Gefilte Fish with Horseradish", Description = "Traditional kosher fish preparation with fresh horseradish", Price = 18m, Category = MenuCategory.Appetizer, IsKosher = true },
            new MenuItem { Name = "Kosher Chicken Matzo Ball Soup", Description = "Homemade kosher chicken broth with fluffy matzo balls", Price = 16m, Category = MenuCategory.Soup, IsKosher = true },
            new MenuItem { Name = "Challah French Toast", Description = "Traditional challah bread made into sweet French toast", Price = 14m, Category = MenuCategory.Dessert, IsKosher = true, IsVegetarian = true },
            new MenuItem { Name = "Kosher Wine Braised Vegetables", Description = "Seasonal vegetables braised in kosher wine with herbs", Price = 20m, Category = MenuCategory.SideDish, IsKosher = true, IsVegetarian = true }
        };
    }

    private List<MenuItem> GetPureeFriendlyItems()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Smooth Butternut Squash Soup", Description = "Silky pureed soup with roasted butternut squash and cream", Price = 16m, Category = MenuCategory.Soup, IsPureeFriendly = true, IsVegetarian = true },
            new MenuItem { Name = "Creamy Chicken and Rice Puree", Description = "Tender chicken and rice blended into a smooth, nutritious meal", Price = 22m, Category = MenuCategory.MainCourse, IsPureeFriendly = true },
            new MenuItem { Name = "Smooth Fruit Compote", Description = "Pureed seasonal fruits with natural sweetness", Price = 12m, Category = MenuCategory.Dessert, IsPureeFriendly = true, IsVegan = true },
            new MenuItem { Name = "Vegetable Protein Smoothie", Description = "Blended vegetables and plant protein for complete nutrition", Price = 18m, Category = MenuCategory.MainCourse, IsPureeFriendly = true, IsVegan = true },
            new MenuItem { Name = "Smooth Lentil Dal", Description = "Finely pureed red lentils with gentle spices and coconut", Price = 20m, Category = MenuCategory.MainCourse, IsPureeFriendly = true, IsVegan = true }
        };
    }

    private List<MenuItem> GetAllergenFreeItems()
    {
        return new List<MenuItem>
        {
            // Tree Nut and Peanut-free items
            new MenuItem { Name = "Nut-Free Granola Bowl", Description = "Seed-based granola with fresh fruit and coconut yogurt", Price = 16m, Category = MenuCategory.Dessert, IsVegetarian = true, AllergenFree = new List<Allergy> { Allergy.TreeNuts, Allergy.Peanuts } },
            new MenuItem { Name = "Seed-Crusted Chicken", Description = "Chicken breast crusted with sunflower and pumpkin seeds", Price = 26m, Category = MenuCategory.MainCourse, AllergenFree = new List<Allergy> { Allergy.TreeNuts, Allergy.Peanuts } },
            new MenuItem { Name = "Coconut-Free Curry", Description = "Mild curry made without coconut milk, using vegetable broth", Price = 22m, Category = MenuCategory.MainCourse, IsVegetarian = true, AllergenFree = new List<Allergy> { Allergy.Coconut } },
            new MenuItem { Name = "Sunflower-Free Salad", Description = "Mixed greens with pumpkin seeds and olive oil dressing", Price = 14m, Category = MenuCategory.Salad, IsVegetarian = true, IsVegan = true, AllergenFree = new List<Allergy> { Allergy.Sunflower } },
            new MenuItem { Name = "Sesame-Free Asian Bowl", Description = "Asian-style vegetables and rice without sesame oil or seeds", Price = 20m, Category = MenuCategory.MainCourse, IsVegetarian = true, IsVegan = true, AllergenFree = new List<Allergy> { Allergy.Sesame } },
            
            // Shellfish and Fish-free items
            new MenuItem { Name = "Land-Based Paella", Description = "Traditional rice dish with chicken and vegetables, no seafood", Price = 28m, Category = MenuCategory.MainCourse, AllergenFree = new List<Allergy> { Allergy.Shellfish, Allergy.Fish, Allergy.Molluscs } },
            new MenuItem { Name = "Chicken and Vegetable Stir-Fry", Description = "Fresh chicken with seasonal vegetables in savory sauce", Price = 24m, Category = MenuCategory.MainCourse, AllergenFree = new List<Allergy> { Allergy.Shellfish, Allergy.Fish } },
            new MenuItem { Name = "Vegetarian Land Feast", Description = "Hearty vegetable stew with beans and grains, no seafood", Price = 18m, Category = MenuCategory.MainCourse, IsVegetarian = true, IsVegan = true, AllergenFree = new List<Allergy> { Allergy.Shellfish, Allergy.Fish, Allergy.Molluscs } },
            
            // Egg-free items
            new MenuItem { Name = "Egg-Free Caesar Salad", Description = "Classic Caesar with vegan dressing and nutritional yeast", Price = 18m, Category = MenuCategory.Salad, IsVegan = true, AllergenFree = new List<Allergy> { Allergy.Eggs } },
            new MenuItem { Name = "Vegan Mayonnaise Sandwich", Description = "Fresh vegetables with plant-based mayo on artisan bread", Price = 16m, Category = MenuCategory.MainCourse, IsVegan = true, AllergenFree = new List<Allergy> { Allergy.Eggs } },
            
            // Dairy/Milk-free items (beyond already covered lactose-free)
            new MenuItem { Name = "Dairy-Free Chocolate Mousse", Description = "Rich chocolate dessert made with coconut cream", Price = 14m, Category = MenuCategory.Dessert, IsVegan = true, AllergenFree = new List<Allergy> { Allergy.Milk } },
            new MenuItem { Name = "Milk-Free Mac and Cheese", Description = "Pasta with cashew-based cheese sauce", Price = 20m, Category = MenuCategory.MainCourse, IsVegan = true, AllergenFree = new List<Allergy> { Allergy.Milk } },
            
            // Soy-free items
            new MenuItem { Name = "Soy-Free Teriyaki Bowl", Description = "Grilled chicken with coconut aminos glaze and vegetables", Price = 26m, Category = MenuCategory.MainCourse, AllergenFree = new List<Allergy> { Allergy.Soy } },
            new MenuItem { Name = "Soy-Free Vegetable Stir-Fry", Description = "Mixed vegetables with garlic and herbs, no soy sauce", Price = 18m, Category = MenuCategory.MainCourse, IsVegetarian = true, IsVegan = true, AllergenFree = new List<Allergy> { Allergy.Soy } },
            
                         // Wheat-free (Gluten-free already covered above)
             new MenuItem { Name = "Rice Paper Spring Rolls", Description = "Fresh vegetables wrapped in rice paper with peanut dip", Price = 16m, Category = MenuCategory.Appetizer, IsVegetarian = true, IsVegan = true, AllergenFree = new List<Allergy> { Allergy.Wheat } }
        };
    }

    private List<MenuItem> GetPaleoDietItems()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Paleo Grilled Steak", Description = "Grass-fed beef with roasted root vegetables and herbs", Price = 38m, Category = MenuCategory.MainCourse, IsDiabeticFriendly = true, IsGlutenFree = true },
            new MenuItem { Name = "Wild Salmon with Sweet Potato", Description = "Paleo-friendly fish with roasted sweet potato and greens", Price = 34m, Category = MenuCategory.MainCourse, IsDiabeticFriendly = true, IsGlutenFree = true },
            new MenuItem { Name = "Coconut Chicken Curry", Description = "Free-range chicken in coconut milk with vegetables, no grains", Price = 30m, Category = MenuCategory.MainCourse, IsDiabeticFriendly = true, IsGlutenFree = true },
            new MenuItem { Name = "Paleo Beef Bowl", Description = "Grass-fed ground beef with cauliflower rice and avocado", Price = 28m, Category = MenuCategory.MainCourse, IsDiabeticFriendly = true, IsGlutenFree = true },
            new MenuItem { Name = "Coconut Macaroons", Description = "Sugar-free coconut treats sweetened with dates", Price = 12m, Category = MenuCategory.Dessert, IsDiabeticFriendly = true, IsGlutenFree = true }
        };
    }

    private List<MenuItem> GetKetoDietItems()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Keto Bacon Cheeseburger Bowl", Description = "Grass-fed beef patty with cheese, bacon, and lettuce (no bun)", Price = 26m, Category = MenuCategory.MainCourse, IsDiabeticFriendly = true, IsGlutenFree = true },
            new MenuItem { Name = "Avocado and Egg Salad", Description = "Keto-friendly salad with high-fat dressing and minimal carbs", Price = 18m, Category = MenuCategory.Salad, IsVegetarian = true, IsDiabeticFriendly = true, IsGlutenFree = true },
            new MenuItem { Name = "Keto Salmon with Asparagus", Description = "High-fat fish with buttered asparagus and hollandaise", Price = 32m, Category = MenuCategory.MainCourse, IsDiabeticFriendly = true, IsGlutenFree = true },
            new MenuItem { Name = "Cauliflower Mac and Cheese", Description = "Low-carb cauliflower with high-fat cheese sauce", Price = 20m, Category = MenuCategory.MainCourse, IsVegetarian = true, IsDiabeticFriendly = true, IsGlutenFree = true },
            new MenuItem { Name = "Keto Fat Bomb Dessert", Description = "High-fat, low-carb chocolate dessert with MCT oil", Price = 14m, Category = MenuCategory.Dessert, IsVegetarian = true, IsDiabeticFriendly = true, IsGlutenFree = true }
        };
    }

    private List<MenuItem> GetMediterraneanDietItems()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Mediterranean Grilled Fish", Description = "Fresh fish with olive oil, herbs, and lemon", Price = 30m, Category = MenuCategory.MainCourse, IsDiabeticFriendly = true, IsGlutenFree = true },
            new MenuItem { Name = "Greek Style Quinoa Bowl", Description = "Quinoa with olives, feta, tomatoes, and olive oil", Price = 22m, Category = MenuCategory.MainCourse, IsVegetarian = true, IsDiabeticFriendly = true, IsGlutenFree = true },
            new MenuItem { Name = "Mediterranean Chickpea Salad", Description = "Protein-rich chickpeas with vegetables and olive oil dressing", Price = 18m, Category = MenuCategory.Salad, IsVegetarian = true, IsVegan = true, IsDiabeticFriendly = true, IsGlutenFree = true },
            new MenuItem { Name = "Herb-Crusted Lamb", Description = "Mediterranean-style lamb with rosemary and garlic", Price = 36m, Category = MenuCategory.MainCourse, IsDiabeticFriendly = true, IsGlutenFree = true },
            new MenuItem { Name = "Greek Yogurt with Honey and Nuts", Description = "Traditional Mediterranean dessert with healthy fats", Price = 12m, Category = MenuCategory.Dessert, IsVegetarian = true, IsDiabeticFriendly = true, IsGlutenFree = true }
        };
    }

    private List<MenuItem> GetLowCarbItems()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Zucchini Lasagna", Description = "Layers of zucchini with meat sauce and cheese, no pasta", Price = 24m, Category = MenuCategory.MainCourse, IsDiabeticFriendly = true, IsGlutenFree = true },
            new MenuItem { Name = "Lettuce Wrap Tacos", Description = "Ground turkey in crisp lettuce cups with salsa", Price = 20m, Category = MenuCategory.MainCourse, IsDiabeticFriendly = true, IsGlutenFree = true },
            new MenuItem { Name = "Cabbage Roll Soup", Description = "All the flavors of cabbage rolls in a hearty, low-carb soup", Price = 16m, Category = MenuCategory.Soup, IsDiabeticFriendly = true, IsGlutenFree = true },
            new MenuItem { Name = "Spaghetti Squash Bolognese", Description = "Roasted spaghetti squash with rich meat sauce", Price = 22m, Category = MenuCategory.MainCourse, IsDiabeticFriendly = true, IsGlutenFree = true },
            new MenuItem { Name = "Cheesecake Bites (Sugar-Free)", Description = "Mini cheesecakes sweetened with stevia on almond crust", Price = 14m, Category = MenuCategory.Dessert, IsVegetarian = true, IsDiabeticFriendly = true, IsGlutenFree = true }
                 };
     }

    // Restaurant-specific dietary dishes based on restaurant tags
    private List<MenuItem> GetRestaurantVegetarianDishes()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Quinoa Stuffed Peppers", Description = "Colorful bell peppers filled with protein-rich quinoa and vegetables", Price = 22m, Category = MenuCategory.MainCourse, IsVegetarian = true, IsVegan = true, IsGlutenFree = true },
            new MenuItem { Name = "Caprese Grilled Vegetable Panini", Description = "Fresh mozzarella, tomatoes, and basil with grilled vegetables", Price = 18m, Category = MenuCategory.MainCourse, IsVegetarian = true }
        };
    }

    private List<MenuItem> GetRestaurantVeganDishes()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Lentil and Sweet Potato Curry", Description = "Hearty red lentils with sweet potato in aromatic spices", Price = 20m, Category = MenuCategory.MainCourse, IsVegan = true, IsGlutenFree = true },
            new MenuItem { Name = "Chickpea and Avocado Salad Wrap", Description = "Protein-packed chickpeas with creamy avocado in a whole wheat wrap", Price = 16m, Category = MenuCategory.MainCourse, IsVegan = true }
        };
    }

    private List<MenuItem> GetRestaurantPescatarianDishes()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Salmon-Avocado Rice Bowl", Description = "Fresh grilled salmon over rice with avocado and vegetables", Price = 26m, Category = MenuCategory.MainCourse, IsGlutenFree = true },
            new MenuItem { Name = "Tuna Niçoise Salad", Description = "Classic French salad with seared tuna and fresh vegetables", Price = 24m, Category = MenuCategory.Salad, IsGlutenFree = true }
        };
    }

    private List<MenuItem> GetRestaurantFlexitarianDishes()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Chicken & Veggie Buddha Bowl", Description = "Lean chicken with abundant fresh vegetables and quinoa", Price = 22m, Category = MenuCategory.MainCourse, IsGlutenFree = true },
            new MenuItem { Name = "Mushroom & Spinach Frittata", Description = "Protein-rich egg dish with vegetables and minimal meat", Price = 18m, Category = MenuCategory.MainCourse, IsVegetarian = true, IsGlutenFree = true }
        };
    }

    private List<MenuItem> GetRestaurantPaleoDishes()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Zucchini Noodle Bolognese", Description = "Spiralized zucchini with grass-fed beef meat sauce", Price = 24m, Category = MenuCategory.MainCourse, IsGlutenFree = true, IsDiabeticFriendly = true },
            new MenuItem { Name = "Grilled Steak with Roasted Sweet Potato", Description = "Grass-fed steak with roasted sweet potato and vegetables", Price = 36m, Category = MenuCategory.MainCourse, IsGlutenFree = true }
        };
    }

    private List<MenuItem> GetRestaurantKetoDishes()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Cauliflower \"Mac\" & Cheese", Description = "Low-carb cauliflower in rich cheese sauce", Price = 18m, Category = MenuCategory.MainCourse, IsVegetarian = true, IsGlutenFree = true, IsDiabeticFriendly = true },
            new MenuItem { Name = "Bacon-and-Egg Breakfast Muffins", Description = "High-fat, low-carb egg muffins with bacon", Price = 14m, Category = MenuCategory.Appetizer, IsGlutenFree = true, IsDiabeticFriendly = true }
        };
    }

    private List<MenuItem> GetRestaurantMediterraneanDishes()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Greek Salad with Feta & Olives", Description = "Traditional Greek salad with olive oil dressing", Price = 16m, Category = MenuCategory.Salad, IsVegetarian = true, IsGlutenFree = true },
            new MenuItem { Name = "Grilled Chicken Souvlaki", Description = "Marinated chicken skewers with Mediterranean herbs", Price = 24m, Category = MenuCategory.MainCourse, IsGlutenFree = true }
        };
    }

    private List<MenuItem> GetRestaurantLowCarbDishes()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Zucchini Lasagna", Description = "Layers of zucchini with meat sauce and cheese, no pasta", Price = 22m, Category = MenuCategory.MainCourse, IsGlutenFree = true, IsDiabeticFriendly = true },
            new MenuItem { Name = "Bunless Turkey Burger", Description = "Lean turkey patty wrapped in lettuce with toppings", Price = 18m, Category = MenuCategory.MainCourse, IsGlutenFree = true, IsDiabeticFriendly = true }
        };
    }

    private List<MenuItem> GetRestaurantHalalDishes()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Chicken Shawarma Plate", Description = "Halal chicken with rice, vegetables, and tahini sauce", Price = 24m, Category = MenuCategory.MainCourse, IsHalal = true },
            new MenuItem { Name = "Lamb Kofta with Tzatziki", Description = "Halal lamb meatballs with yogurt sauce and pita", Price = 26m, Category = MenuCategory.MainCourse, IsHalal = true }
        };
    }

    private List<MenuItem> GetRestaurantKosherDishes()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Matzo Ball Soup", Description = "Traditional Jewish soup with fluffy matzo balls", Price = 14m, Category = MenuCategory.Soup, IsKosher = true },
            new MenuItem { Name = "Roasted Salmon with Herbs", Description = "Kosher salmon with fresh herbs and lemon", Price = 28m, Category = MenuCategory.MainCourse, IsKosher = true, IsGlutenFree = true }
        };
    }

    private List<MenuItem> GetRestaurantDiabeticFriendlyDishes()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Veggie Omelet with Avocado", Description = "Protein-rich eggs with vegetables and healthy fats", Price = 16m, Category = MenuCategory.MainCourse, IsVegetarian = true, IsDiabeticFriendly = true, IsGlutenFree = true },
            new MenuItem { Name = "Turkey Lettuce Wraps", Description = "Lean turkey in crisp lettuce cups with low-carb vegetables", Price = 18m, Category = MenuCategory.MainCourse, IsDiabeticFriendly = true, IsGlutenFree = true }
        };
    }

    private List<MenuItem> GetRestaurantLactoseFreeDishes()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Vegan Cashew Alfredo Zoodles", Description = "Zucchini noodles with creamy cashew sauce", Price = 20m, Category = MenuCategory.MainCourse, IsVegan = true, IsLactoseFree = true, IsGlutenFree = true },
            new MenuItem { Name = "Coconut Mango Smoothie", Description = "Tropical smoothie made with coconut milk", Price = 8m, Category = MenuCategory.Beverage, IsVegan = true, IsLactoseFree = true, IsGlutenFree = true }
        };
    }

    private List<MenuItem> GetRestaurantGlutenFreeDishes()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Quinoa & Roasted Vegetable Salad", Description = "Protein-rich quinoa with seasonal roasted vegetables", Price = 18m, Category = MenuCategory.Salad, IsVegetarian = true, IsVegan = true, IsGlutenFree = true },
            new MenuItem { Name = "Rice Paper Spring Rolls with Shrimp", Description = "Fresh shrimp wrapped in rice paper with herbs", Price = 16m, Category = MenuCategory.Appetizer, IsGlutenFree = true }
        };
    }

    private List<MenuItem> GetRestaurantPeanutFreeDishes()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Roasted Beet & Apple Salad", Description = "Fresh beets and apples with seed-based dressing", Price = 16m, Category = MenuCategory.Salad, IsVegetarian = true, IsVegan = true, AllergenFree = new List<Allergy> { Allergy.Peanuts } },
            new MenuItem { Name = "Grilled Chicken Caesar Salad (no croutons)", Description = "Classic Caesar without bread and nuts", Price = 20m, Category = MenuCategory.Salad, IsGlutenFree = true, AllergenFree = new List<Allergy> { Allergy.Peanuts } }
        };
    }

    private List<MenuItem> GetRestaurantTreeNutFreeDishes()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Beef & Broccoli Stir-Fry", Description = "Tender beef with broccoli in savory sauce", Price = 22m, Category = MenuCategory.MainCourse, AllergenFree = new List<Allergy> { Allergy.TreeNuts } },
            new MenuItem { Name = "Quinoa & Black Bean Bowl", Description = "Protein-rich quinoa with black beans and vegetables", Price = 18m, Category = MenuCategory.MainCourse, IsVegetarian = true, IsVegan = true, AllergenFree = new List<Allergy> { Allergy.TreeNuts } }
        };
    }

    private List<MenuItem> GetRestaurantMilkFreeDishes()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Lentil Coconut Curry", Description = "Creamy curry made with coconut milk instead of dairy", Price = 20m, Category = MenuCategory.MainCourse, IsVegan = true, AllergenFree = new List<Allergy> { Allergy.Milk } },
            new MenuItem { Name = "Avocado & Tuna Lettuce Cups", Description = "Fresh tuna and avocado in crisp lettuce cups", Price = 18m, Category = MenuCategory.Appetizer, IsGlutenFree = true, AllergenFree = new List<Allergy> { Allergy.Milk } }
        };
    }

    private List<MenuItem> GetRestaurantEggFreeDishes()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Chia Seed Pudding", Description = "Creamy pudding made with chia seeds and plant milk", Price = 12m, Category = MenuCategory.Dessert, IsVegan = true, AllergenFree = new List<Allergy> { Allergy.Eggs } },
            new MenuItem { Name = "Overnight Oats with Rice Milk", Description = "No-cook oats soaked in rice milk with fruit", Price = 10m, Category = MenuCategory.Dessert, IsVegan = true, AllergenFree = new List<Allergy> { Allergy.Eggs } }
        };
    }

    private List<MenuItem> GetRestaurantSoyFreeDishes()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Kale & Apple Slaw", Description = "Fresh kale salad with apples and tahini dressing", Price = 14m, Category = MenuCategory.Salad, IsVegan = true, AllergenFree = new List<Allergy> { Allergy.Soy } },
            new MenuItem { Name = "Roasted Carrot Soup", Description = "Creamy carrot soup made without soy products", Price = 12m, Category = MenuCategory.Soup, IsVegan = true, AllergenFree = new List<Allergy> { Allergy.Soy } }
        };
    }

    private List<MenuItem> GetRestaurantFishFreeDishes()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Vegetable Paella", Description = "Traditional rice dish with vegetables and saffron", Price = 22m, Category = MenuCategory.MainCourse, IsVegetarian = true, IsVegan = true, AllergenFree = new List<Allergy> { Allergy.Fish } },
            new MenuItem { Name = "Quinoa Stuffed Peppers", Description = "Bell peppers filled with quinoa and vegetables", Price = 20m, Category = MenuCategory.MainCourse, IsVegetarian = true, IsVegan = true, AllergenFree = new List<Allergy> { Allergy.Fish } }
        };
    }

    private List<MenuItem> GetRestaurantShellfishFreeDishes()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Margherita Pizza on Gluten-Free Crust", Description = "Classic pizza with tomatoes, mozzarella, and basil", Price = 18m, Category = MenuCategory.MainCourse, IsVegetarian = true, IsGlutenFree = true, AllergenFree = new List<Allergy> { Allergy.Shellfish } },
            new MenuItem { Name = "Vegetarian Tacos", Description = "Plant-based tacos with beans, vegetables, and avocado", Price = 16m, Category = MenuCategory.MainCourse, IsVegetarian = true, IsVegan = true, AllergenFree = new List<Allergy> { Allergy.Shellfish } }
        };
    }

    private List<MenuItem> GetRestaurantWheatFreeDishes()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Zucchini Noodles with Pesto", Description = "Spiralized zucchini with fresh basil pesto", Price = 18m, Category = MenuCategory.MainCourse, IsVegetarian = true, AllergenFree = new List<Allergy> { Allergy.Wheat } },
            new MenuItem { Name = "Grilled Salmon & Vegetables", Description = "Fresh salmon with seasonal grilled vegetables", Price = 26m, Category = MenuCategory.MainCourse, IsGlutenFree = true, AllergenFree = new List<Allergy> { Allergy.Wheat } }
        };
    }

    private List<MenuItem> GetRestaurantSesameFreeDishes()
    {
        return new List<MenuItem>
        {
            new MenuItem { Name = "Citrus Chicken Salad", Description = "Grilled chicken with citrus dressing and mixed greens", Price = 20m, Category = MenuCategory.Salad, IsGlutenFree = true, AllergenFree = new List<Allergy> { Allergy.Sesame } },
            new MenuItem { Name = "Sweet Potato & Black Bean Tacos", Description = "Roasted sweet potato tacos without sesame", Price = 16m, Category = MenuCategory.MainCourse, IsVegetarian = true, IsVegan = true, AllergenFree = new List<Allergy> { Allergy.Sesame } }
        };
    }
} 