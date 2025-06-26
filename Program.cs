using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DineSure.Data;
using DineSure.Models;
using DineSure.Services;
using RestaurantsByMe.Services;
using static DineSure.Models.User;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Database context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors());

// Identity
builder.Services.AddDefaultIdentity<User>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.Cookie.Name = "RestaurantsByMe.Auth";
    options.Cookie.HttpOnly = true;

    if (builder.Environment.IsDevelopment())
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    }
    else
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    }
});

// Others
builder.Services.AddAuthorization();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// App services
builder.Services.AddScoped<AuthService>();
builder.Services.AddHttpClient<OverpassRestaurantService>();
builder.Services.AddScoped<FavoriteService>();
builder.Services.AddScoped<ReservationService>();
builder.Services.AddScoped<MenuService>();
builder.Services.AddScoped<RestaurantTagSeeder>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/Account/Login", async (HttpContext context, SignInManager<User> signInManager, UserManager<User> userManager) =>
{
    var form = await context.Request.ReadFormAsync();
    var email = form["Email"].ToString();
    var password = form["Password"].ToString();
    var rememberMe = form["RememberMe"].ToString().Contains("true");

    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
    {
        context.Response.Redirect("/login?error=Invalid email or password");
        return;
    }

    var user = await userManager.FindByEmailAsync(email);
    if (user == null)
    {
        context.Response.Redirect("/login?error=Invalid email or password");
        return;
    }

    var result = await signInManager.PasswordSignInAsync(user.UserName!, password, rememberMe, false);

    if (result.Succeeded)
    {
        context.Response.Redirect("/");
        return;
    }

    if (result.IsLockedOut)
    {
        context.Response.Redirect("/login?error=Account is locked out. Please try again later");
        return;
    }

    if (result.RequiresTwoFactor)
    {
        context.Response.Redirect("/login?error=Two-factor authentication is required");
        return;
    }

    context.Response.Redirect("/login?error=Invalid email or password");
});

app.MapPost("/Account/Logout", async (HttpContext context, SignInManager<User> signInManager) =>
{
    await signInManager.SignOutAsync();
    context.Response.Redirect("/");
});

// Temporary endpoint to seed restaurant tags
app.MapPost("/api/seed-restaurant-tags", async (RestaurantTagSeeder seeder) =>
{
    await seeder.SeedRestaurantTags();
    return Results.Ok("Restaurant tags seeded successfully!");
});

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// DB Init + Seed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var overpassService = services.GetRequiredService<OverpassRestaurantService>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Running database migrations...");
        context.Database.Migrate();
        logger.LogInformation("Database migrations completed successfully");

        logger.LogInformation("Checking and creating roles...");
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }
        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole("User"));
        }

        logger.LogInformation("Checking for test user...");
        var testEmail = "test@test.com";
        var testUser = await userManager.FindByEmailAsync(testEmail);
        if (testUser == null)
        {
            testUser = new User
            {
                UserName = "Maria_R",
                Email = testEmail,
                EmailConfirmed = true,
                NormalizedEmail = testEmail.ToUpper(),
                NormalizedUserName = "MARIA_R"
            };

            var createResult = await userManager.CreateAsync(testUser, "Test123!");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(testUser, "User");
                logger.LogInformation("Test user created successfully.");
            }
            else
            {
                logger.LogError("Failed to create test user: {Errors}", string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            // Update existing test user with better name
            if (testUser.UserName == "testuser")
            {
                testUser.UserName = "Maria_R";
                testUser.NormalizedUserName = "MARIA_R";
                await userManager.UpdateAsync(testUser);
                logger.LogInformation("Updated test user name.");
            }
        }

        var adminEmail = "anastasia.alexandru06@gmail.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = "Alex_M",
                Email = adminEmail,
                EmailConfirmed = true,
                NormalizedEmail = adminEmail.ToUpper(),
                NormalizedUserName = "ALEX_M"
            };

            var createResult = await userManager.CreateAsync(adminUser, "Admin123!");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                await userManager.AddToRoleAsync(adminUser, "User");
                logger.LogInformation("Admin user created successfully.");
            }
            else
            {
                logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            // Update existing admin user with better name
            if (adminUser.UserName == "anastasia")
            {
                adminUser.UserName = "Alex_M";
                adminUser.NormalizedUserName = "ALEX_M";
                await userManager.UpdateAsync(adminUser);
                logger.LogInformation("Updated admin user name.");
            }
        }

        // Create additional fictive users for reviews
        var fictiveUsers = new[]
        {
            new { Email = "david.popescu@gmail.com", UserName = "David_P", Password = "User123!" },
            new { Email = "elena.ionescu@gmail.com", UserName = "Elena_I", Password = "User123!" },
            new { Email = "mihai.radu@gmail.com", UserName = "Mihai_R", Password = "User123!" },
            new { Email = "laura.stan@gmail.com", UserName = "Laura_S", Password = "User123!" }
        };

        foreach (var fictiveUserData in fictiveUsers)
        {
            var existingUser = await userManager.FindByEmailAsync(fictiveUserData.Email);
            if (existingUser == null)
            {
                var fictiveUser = new User
                {
                    UserName = fictiveUserData.UserName,
                    Email = fictiveUserData.Email,
                    EmailConfirmed = true,
                    NormalizedEmail = fictiveUserData.Email.ToUpper(),
                    NormalizedUserName = fictiveUserData.UserName.ToUpper()
                };

                var createResult = await userManager.CreateAsync(fictiveUser, fictiveUserData.Password);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(fictiveUser, "User");
                    logger.LogInformation($"Fictive user {fictiveUserData.UserName} created successfully.");
                }
                else
                {
                    logger.LogError($"Failed to create fictive user {fictiveUserData.UserName}: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }
        }

        logger.LogInformation("Importing real restaurant data from Overpass API...");
        try
        {
            await overpassService.SaveRestaurantsToDatabaseAsync(context);
            logger.LogInformation("Overpass import complete.");
        }
        catch (HttpRequestException httpEx) when (httpEx.Message.Contains("504") || httpEx.Message.Contains("Gateway Timeout"))
        {
            logger.LogWarning("Overpass API is temporarily unavailable (504 Gateway Timeout). Continuing with existing restaurant data.");
        }
        catch (TaskCanceledException)
        {
            logger.LogWarning("Overpass API request timed out. Continuing with existing restaurant data.");
        }
        catch (Exception importEx)
        {
            logger.LogWarning(importEx, "Failed to import restaurants from Overpass API. Continuing with existing restaurant data.");
        }

        // Assign dietary tags to restaurants that don't have them
        // TODO: Fix logger scope and re-enable this
        // await AssignDietaryTagsToRestaurants(context, logger);

        // Seed fictive reviews
        logger.LogInformation("Seeding fictive reviews...");
        await SeedFictiveReviews(context, userManager, logger);
    }
    catch (Exception ex)
    {
        var errorLogger = services.GetRequiredService<ILogger<Program>>();
        errorLogger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();

static async Task SeedFictiveReviews(ApplicationDbContext context, UserManager<User> userManager, ILogger logger)
{
    try
    {
        // Get all restaurants
        var restaurants = context.Restaurants.ToList();
        if (!restaurants.Any())
        {
            logger.LogInformation("No restaurants found, skipping review seeding.");
            return;
        }

        // Get all fictive users for reviews
        var userEmails = new[]
        {
            "test@test.com",
            "anastasia.alexandru06@gmail.com",
            "david.popescu@gmail.com",
            "elena.ionescu@gmail.com",
            "mihai.radu@gmail.com",
            "laura.stan@gmail.com"
        };

        var reviewUsers = new List<User>();
        foreach (var email in userEmails)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {
                reviewUsers.Add(user);
            }
        }

        if (!reviewUsers.Any())
        {
            logger.LogWarning("No users found for seeding reviews.");
            return;
        }

        // Predefined review templates
        var reviewTemplates = new[]
        {
            new { Title = "Great food and service", Comment = "I had an amazing experience at this restaurant. The food was delicious and the service was excellent. The atmosphere was cozy and perfect for a nice dinner. Will definitely come back!", Rating = 5 },
            new { Title = "Good value for money", Comment = "The prices are reasonable and the portions are generous. The staff was friendly and attentive. The food arrived quickly and was well-prepared. A solid choice for a casual meal.", Rating = 4 },
            new { Title = "Excellent atmosphere", Comment = "Beautiful interior design and very welcoming atmosphere. The menu has great variety and everything we ordered was fresh and tasty. Perfect place for special occasions.", Rating = 5 },
            new { Title = "Fresh and tasty", Comment = "The ingredients are clearly fresh and high quality. The chef knows what they're doing - every dish was perfectly seasoned and presented beautifully. Highly recommended!", Rating = 4 },
            new { Title = "Cozy and comfortable", Comment = "Nice intimate setting with comfortable seating. The service was prompt and the staff was knowledgeable about the menu. Good place to relax and enjoy a meal.", Rating = 4 },
            new { Title = "Outstanding cuisine", Comment = "This place exceeded my expectations! The flavors were incredible and the presentation was restaurant-quality. The wine selection pairs perfectly with the food.", Rating = 5 },
            new { Title = "Friendly staff", Comment = "The waiters were very helpful and made great recommendations. The kitchen accommodated our dietary restrictions without any issues. Great customer service overall.", Rating = 4 },
            new { Title = "Perfect for families", Comment = "Kid-friendly environment with something for everyone on the menu. The portions are large enough to share and the prices are family-friendly. We'll be back with the kids!", Rating = 4 },
            new { Title = "Authentic flavors", Comment = "You can taste the authenticity in every bite. The recipes seem traditional and well-executed. The spice levels are perfect and the ingredients are clearly high quality.", Rating = 5 },
            new { Title = "Quick and convenient", Comment = "Fast service without compromising on quality. Perfect for lunch breaks or when you're in a hurry. The online ordering system works great too.", Rating = 4 }
        };

        var random = new Random();
        var restaurantsSeeded = 0;

        foreach (var restaurant in restaurants)
        {
            // Check if restaurant already has reviews
            var existingReviewsCount = context.RestaurantReviews
                .Where(r => r.RestaurantId == restaurant.Id)
                .Count();

            if (existingReviewsCount >= 2)
            {
                continue; // Skip if already has 2+ reviews
            }

            // Add 2 reviews per restaurant
            var reviewsToAdd = 2 - existingReviewsCount;
            var usedTemplates = new HashSet<int>();

            for (int i = 0; i < reviewsToAdd; i++)
            {
                // Select a random template that hasn't been used for this restaurant
                int templateIndex;
                do
                {
                    templateIndex = random.Next(reviewTemplates.Length);
                } while (usedTemplates.Contains(templateIndex));
                
                usedTemplates.Add(templateIndex);
                var template = reviewTemplates[templateIndex];

                // Select a random user who hasn't reviewed this restaurant yet
                var availableUsers = reviewUsers.Where(u => !context.RestaurantReviews
                    .Any(r => r.RestaurantId == restaurant.Id && r.UserId == u.Id)).ToList();

                if (!availableUsers.Any())
                {
                    continue; // Skip if no available users for this restaurant
                }

                var reviewUser = availableUsers[random.Next(availableUsers.Count)];

                var review = new RestaurantReview
                {
                    RestaurantId = restaurant.Id,
                    UserId = reviewUser.Id,
                    Title = template.Title,
                    Comment = template.Comment,
                    Rating = template.Rating,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30)), // Random date within last 30 days
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                };

                context.RestaurantReviews.Add(review);
            }

            restaurantsSeeded++;
        }

        await context.SaveChangesAsync();
        logger.LogInformation($"Successfully seeded reviews for {restaurantsSeeded} restaurants.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error occurred while seeding fictive reviews.");
    }
}

/* TODO: Fix logger scope issue
static async Task AssignDietaryTagsToRestaurants(ApplicationDbContext context, ILogger logger)
{
    var restaurants = context.Restaurants.ToList();
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
    
    foreach (var restaurant in restaurants)
    {
        bool needsUpdate = false;
        
        // Ensure every restaurant has at least one dietary type
        if (!restaurant.SupportedDietaryTypes.Any())
        {
            // Assign 1-3 random dietary types based on cuisine
            int numDietaryTypes = random.Next(1, 4);
            var selectedTypes = new HashSet<DietaryType>();
            
            // Add cuisine-appropriate dietary types
            if (restaurant.CuisineTypes.Contains(CuisineType.Italian))
            {
                selectedTypes.Add(DietaryType.Mediterranean);
                if (random.NextDouble() < 0.6) selectedTypes.Add(DietaryType.Vegetarian);
            }
            else if (restaurant.CuisineTypes.Contains(CuisineType.Indian))
            {
                selectedTypes.Add(DietaryType.Vegetarian);
                if (random.NextDouble() < 0.4) selectedTypes.Add(DietaryType.Vegan);
            }
            else if (restaurant.CuisineTypes.Contains(CuisineType.Japanese) || restaurant.CuisineTypes.Contains(CuisineType.Thai))
            {
                selectedTypes.Add(DietaryType.Pescatarian);
                if (random.NextDouble() < 0.5) selectedTypes.Add(DietaryType.LowCarb);
            }
            else if (restaurant.CuisineTypes.Contains(CuisineType.French))
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
        await context.SaveChangesAsync();
        logger.LogInformation($"Successfully assigned dietary tags to {updatedCount} restaurants.");
    }
    else
    {
        logger.LogInformation("All restaurants already have dietary tags assigned.");
    }
} */

public record LoginRequest(string Email, string Password, bool RememberMe);
