using DineSure.Models;
using DineSure.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace DineSure.Services;

public class OverpassRestaurantService
{
    private readonly HttpClient _httpClient;

    public OverpassRestaurantService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(10); // Shorter timeout to prevent startup delays
    }

    // 1. Obține restaurante reale din Overpass API
    public async Task<List<RestaurantOsm>> GetRestaurantsFromOSMAsync()
    {
        var query = "[out:json][timeout:10];" +
                    "(" +
                    "node[\"amenity\"=\"restaurant\"](44.35,26.00,44.55,26.25);" +
                    "way[\"amenity\"=\"restaurant\"](44.35,26.00,44.55,26.25);" +
                    "relation[\"amenity\"=\"restaurant\"](44.35,26.00,44.55,26.25);" +
                    ");out center tags;";

        var url = "https://overpass-api.de/api/interpreter?data=" + Uri.EscapeDataString(query);
        
        string response;
        try
        {
            response = await _httpClient.GetStringAsync(url);
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"Failed to fetch data from Overpass API: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new TaskCanceledException("Overpass API request timed out", ex);
        }
        
        var result = new List<RestaurantOsm>();

        using var doc = JsonDocument.Parse(response);
        foreach (var el in doc.RootElement.GetProperty("elements").EnumerateArray())
        {
            if (!el.TryGetProperty("tags", out var tags)) continue;
            if (!tags.TryGetProperty("name", out var name)) continue;

            string? street = tags.TryGetProperty("addr:street", out var st) ? st.GetString() : null;
            string? number = tags.TryGetProperty("addr:housenumber", out var nr) ? nr.GetString() : null;
            string address = $"{street ?? "Stradă necunoscută"} {number}".Trim();

            string? phone = tags.TryGetProperty("contact:phone", out var ph) ? ph.GetString() :
                            tags.TryGetProperty("phone", out var ph2) ? ph2.GetString() : null;

            string? email = tags.TryGetProperty("contact:email", out var em) ? em.GetString() :
                            tags.TryGetProperty("email", out var em2) ? em2.GetString() : null;

            string? website = tags.TryGetProperty("contact:website", out var ws) ? ws.GetString() :
                              tags.TryGetProperty("website", out var ws2) ? ws2.GetString() : null;

            result.Add(new RestaurantOsm
            {
                Name = name.GetString() ?? "Unnamed Restaurant",
                Street = street,
                Number = number,
                Address = address,
                City = "Bucharest",
                PhoneNumber = phone,
                Email = email,
                Website = website,
                Lat = el.TryGetProperty("lat", out var lat) ? lat.GetDouble() : el.GetProperty("center").GetProperty("lat").GetDouble(),
                Lon = el.TryGetProperty("lon", out var lon) ? lon.GetDouble() : el.GetProperty("center").GetProperty("lon").GetDouble()
            });
        }

        return result;
    }

    // 2. Salvează restaurantele reale în baza de date (doar dacă nu există deja)
    public async Task SaveRestaurantsToDatabaseAsync(ApplicationDbContext context)
    {
        var osmRestaurants = await GetRestaurantsFromOSMAsync();

        foreach (var r in osmRestaurants)
        {
            bool duplicate = await context.Restaurants.AnyAsync(db =>
                db.Name == r.Name &&
                db.Address.Contains(r.Street ?? string.Empty) &&
                db.Address.Contains(r.Number ?? string.Empty));

            if (!duplicate)
            {
                var restaurant = new Restaurant
                {
                    Name = r.Name,
                    Address = r.Address ?? string.Empty,
                    City = 1, // 1 = Bucharest
                    Latitude = r.Lat,
                    Longitude = r.Lon,
                    PhoneNumber = r.PhoneNumber ?? string.Empty,
                    Email = r.Email ?? string.Empty,
                    Website = r.Website ?? string.Empty,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                context.Restaurants.Add(restaurant);
            }
        }

        await context.SaveChangesAsync();
    }

    // 3. Obține restaurantele deja salvate din baza de date
    public async Task<List<Restaurant>> GetRestaurantsFromDbAsync(ApplicationDbContext context)
    {
        return await context.Restaurants.ToListAsync();
    }
}
