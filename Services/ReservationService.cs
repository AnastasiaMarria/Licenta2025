using Microsoft.EntityFrameworkCore;
using DineSure.Data;
using DineSure.Models;
using RestaurantsByMe.Services;
using Microsoft.Extensions.Logging;

namespace DineSure.Services;

public class ReservationService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(ApplicationDbContext context, IEmailService emailService, ILogger<ReservationService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Reservation> CreateReservationAsync(Reservation reservation)
    {
        reservation.CreatedAt = DateTime.UtcNow;
        reservation.UpdatedAt = DateTime.UtcNow;
        reservation.Status = ReservationStatus.Pending;

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();
        
        // Send confirmation email
        try
        {
            var restaurant = await _context.Restaurants.FindAsync(reservation.RestaurantId);
            if (restaurant != null)
            {
                await _emailService.SendReservationConfirmationAsync(
                    reservation.CustomerEmail,
                    reservation.CustomerName,
                    restaurant.Name,
                    reservation.ReservationDateTime,
                    reservation.NumberOfGuests,
                    reservation.Id
                );
            }
        }
        catch (Exception emailEx)
        {
            // Log email error but don't fail the reservation
            _logger.LogError(emailEx, "Failed to send confirmation email for reservation {ReservationId}", reservation.Id);
        }

        // Trigger real-time update for reports
        await TriggerReportsUpdate();

        return reservation;
    }

    private Task TriggerReportsUpdate()
    {
        // This method can be expanded later with SignalR or other real-time mechanisms
        // For now, it's a placeholder for future real-time functionality
        return Task.CompletedTask;
    }

    public async Task<List<Reservation>> GetUserReservationsAsync(string userId)
    {
        return await _context.Reservations
            .Where(r => r.UserId == userId)
            .Include(r => r.Restaurant)
            .OrderByDescending(r => r.ReservationDateTime)
            .ToListAsync();
    }

    public async Task<List<Reservation>> GetRestaurantReservationsAsync(int restaurantId, DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await _context.Reservations
            .Where(r => r.RestaurantId == restaurantId 
                       && r.ReservationDateTime >= startOfDay 
                       && r.ReservationDateTime < endOfDay
                       && r.Status != ReservationStatus.Cancelled)
            .OrderBy(r => r.ReservationDateTime)
            .ToListAsync();
    }

    public async Task<List<TimeSlot>> GetAvailableTimeSlotsAsync(int restaurantId, DateTime date)
    {
        var existingReservations = await GetRestaurantReservationsAsync(restaurantId, date);
        var availableSlots = new List<TimeSlot>();

        // Define restaurant hours (11:00 AM to 10:00 PM)
        var openingTime = new TimeSpan(11, 0, 0); // 11:00 AM
        var closingTime = new TimeSpan(22, 0, 0); // 10:00 PM
        var slotDuration = TimeSpan.FromMinutes(30); // 30-minute slots

        // Generate all possible time slots
        for (var time = openingTime; time < closingTime; time = time.Add(slotDuration))
        {
            var slotDateTime = date.Date.Add(time);
            
            // Skip past time slots for today
            if (date.Date == DateTime.Today && slotDateTime <= DateTime.Now)
                continue;

            // Count existing reservations for this time slot (assuming 2-hour dining window)
            var conflictingReservations = existingReservations.Count(r => 
                Math.Abs((r.ReservationDateTime - slotDateTime).TotalMinutes) < 120);

            // Assume restaurant has capacity for 10 concurrent reservations
            var maxCapacity = 10;
            var isAvailable = conflictingReservations < maxCapacity;

            availableSlots.Add(new TimeSlot
            {
                DateTime = slotDateTime,
                IsAvailable = isAvailable,
                ExistingReservations = conflictingReservations
            });
        }

        return availableSlots;
    }

    public async Task<bool> UpdateReservationStatusAsync(int reservationId, ReservationStatus status)
    {
        var reservation = await _context.Reservations.FindAsync(reservationId);
        if (reservation == null) return false;

        reservation.Status = status;
        reservation.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelReservationAsync(int reservationId, string userId)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Restaurant)
            .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == userId);
            
        if (reservation == null) return false;

        reservation.Status = ReservationStatus.Cancelled;
        reservation.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        // Send cancellation email
        try
        {
            if (reservation.Restaurant != null)
            {
                await _emailService.SendReservationCancellationAsync(
                    reservation.CustomerEmail,
                    reservation.CustomerName,
                    reservation.Restaurant.Name,
                    reservation.ReservationDateTime
                );
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the cancellation
            Console.WriteLine($"Failed to send cancellation email: {ex.Message}");
        }
        
        return true;
    }
}

public class TimeSlot
{
    public DateTime DateTime { get; set; }
    public bool IsAvailable { get; set; }
    public int ExistingReservations { get; set; }
} 