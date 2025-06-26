using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using DineSure.Models;

namespace DineSure.Services;

public class AuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<User> userManager, 
        SignInManager<User> signInManager,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<(bool success, string? error, User? user)> LoginAsync(string email, string password, bool rememberMe = false)
    {
        try
        {
            _logger.LogInformation("Attempting to log in user {Email}", email);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User {Email} not found", email);
                return (false, "Invalid email or password.", null);
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: false);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in successfully", email);
                return (true, null, user);
            }
            
            if (result.IsLockedOut)
            {
                _logger.LogWarning("Login failed: User {Email} account is locked out", email);
                return (false, "Account is locked out.", null);
            }
            
            if (result.IsNotAllowed)
            {
                _logger.LogWarning("Login failed: User {Email} is not allowed to sign in", email);
                return (false, "Account is not confirmed.", null);
            }

            _logger.LogWarning("Login failed for user {Email}: Invalid credentials", email);
            return (false, "Invalid email or password.", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for user {Email}", email);
            return (false, "An unexpected error occurred. Please try again.", null);
        }
    }

    public async Task<(bool success, string[] errors)> RegisterAsync(User user, string password)
    {
        try
        {
            _logger.LogInformation("Attempting to register user {Email}", user.Email);

            // Normalize the email and username
            user.NormalizedEmail = _userManager.NormalizeName(user.Email ?? string.Empty);
            user.NormalizedUserName = _userManager.NormalizeName(user.UserName ?? string.Empty);

            // Check if user already exists
            if (!string.IsNullOrEmpty(user.Email))
            {
                var existingUser = await _userManager.FindByEmailAsync(user.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration failed: Email {Email} is already registered", user.Email);
                    return (false, new[] { "Email is already registered." });
                }
            }

            if (!string.IsNullOrEmpty(user.UserName))
            {
                var existingUser = await _userManager.FindByNameAsync(user.UserName);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration failed: Username {UserName} is already taken", user.UserName);
                    return (false, new[] { "Username is already taken." });
                }
            }

            var result = await _userManager.CreateAsync(user, password);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} registered successfully", user.Email);

                // Add the user to the default role if needed
                var addToRoleResult = await _userManager.AddToRoleAsync(user, "User");
                if (!addToRoleResult.Succeeded)
                {
                    _logger.LogWarning("Failed to add user {Email} to default role. Errors: {Errors}",
                        user.Email, string.Join(", ", addToRoleResult.Errors.Select(e => e.Description)));
                }

                return (true, Array.Empty<string>());
            }
            
            _logger.LogWarning("Registration failed for user {Email}. Errors: {Errors}", 
                user.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return (false, result.Errors.Select(e => e.Description).ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for user {Email}", user.Email);
            return (false, new[] { "An unexpected error occurred during registration. Please try again." });
        }
    }

    public async Task<User?> GetCurrentUserAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _userManager.FindByNameAsync(username);
    }

    public async Task<(bool success, string[] errors)> UpdateUserProfileAsync(string userEmail, User updatedUser)
    {
        try
        {
            _logger.LogInformation("Attempting to update profile for user {Email}", userEmail);

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                _logger.LogWarning("Update failed: User {Email} not found", userEmail);
                return (false, new[] { "User not found." });
            }

            // Check if username is being changed and if it's already taken
            if (!string.IsNullOrEmpty(updatedUser.UserName) && 
                user.UserName != updatedUser.UserName)
            {
                var existingUser = await _userManager.FindByNameAsync(updatedUser.UserName);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    _logger.LogWarning("Username update failed: Username {UserName} is already taken", updatedUser.UserName);
                    return (false, new[] { "Username is already taken." });
                }
                
                // Update username and normalized username
                user.UserName = updatedUser.UserName;
                user.NormalizedUserName = _userManager.NormalizeName(updatedUser.UserName);
            }

            // Update the user's profile information
            user.DietaryType = updatedUser.DietaryType;
            user.Allergies = updatedUser.Allergies;
            user.FoodPreferences = updatedUser.FoodPreferences;
            user.DietaryRestrictions = updatedUser.DietaryRestrictions;
            user.HasDiabetes = updatedUser.HasDiabetes;
            user.IsLactoseIntolerant = updatedUser.IsLactoseIntolerant;
            user.IsGlutenFree = updatedUser.IsGlutenFree;
            user.NeedsPureeFoods = updatedUser.NeedsPureeFoods;

            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} profile updated successfully", userEmail);
                return (true, Array.Empty<string>());
            }
            
            _logger.LogWarning("Profile update failed for user {Email}. Errors: {Errors}", 
                userEmail, string.Join(", ", result.Errors.Select(e => e.Description)));
            return (false, result.Errors.Select(e => e.Description).ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during profile update for user {Email}", userEmail);
            return (false, new[] { "An unexpected error occurred during profile update. Please try again." });
        }
    }

    public async Task<bool> IsUserAdminAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return false;
        
        return await _userManager.IsInRoleAsync(user, "Admin");
    }

    public async Task<(bool success, string[] errors)> CreateAdminUserAsync(string email, string password, string username)
    {
        try
        {
            _logger.LogInformation("Attempting to create admin user {Email}", email);

            // Check if admin user already exists
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                // If user exists but is not admin, make them admin
                if (!await _userManager.IsInRoleAsync(existingUser, "Admin"))
                {
                    var addToRoleResult = await _userManager.AddToRoleAsync(existingUser, "Admin");
                    if (addToRoleResult.Succeeded)
                    {
                        _logger.LogInformation("User {Email} promoted to admin", email);
                        return (true, Array.Empty<string>());
                    }
                    return (false, addToRoleResult.Errors.Select(e => e.Description).ToArray());
                }
                
                _logger.LogInformation("Admin user {Email} already exists", email);
                return (true, Array.Empty<string>());
            }

            // Create new admin user
            var adminUser = new User
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true, // Auto-confirm admin email
                DietaryType = DietaryType.None,
                Allergies = new List<Allergy>(),
                FoodPreferences = new List<string>(),
                DietaryRestrictions = new List<string>()
            };

            var result = await _userManager.CreateAsync(adminUser, password);
            
            if (result.Succeeded)
            {
                // Add admin role
                var addToRoleResult = await _userManager.AddToRoleAsync(adminUser, "Admin");
                if (addToRoleResult.Succeeded)
                {
                    _logger.LogInformation("Admin user {Email} created successfully", email);
                    return (true, Array.Empty<string>());
                }
                else
                {
                    // If role assignment fails, delete the user
                    await _userManager.DeleteAsync(adminUser);
                    return (false, addToRoleResult.Errors.Select(e => e.Description).ToArray());
                }
            }
            
            _logger.LogWarning("Admin user creation failed for {Email}. Errors: {Errors}", 
                email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return (false, result.Errors.Select(e => e.Description).ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during admin user creation for {Email}", email);
            return (false, new[] { "An unexpected error occurred during admin user creation." });
        }
    }

    // This method is no longer used for actual logout - that's handled by HTTP endpoints
    // But keeping it for compatibility with existing code
    public Task LogoutAsync()
    {
        // This is now handled by the /Account/Logout endpoint
        // This method exists only for compatibility
        return Task.CompletedTask;
    }
} 