using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace RestaurantsByMe.Services
{
    public interface IEmailService
    {
        Task SendReservationConfirmationAsync(string customerEmail, string customerName, string restaurantName, DateTime reservationDateTime, int guests, int reservationId);
        Task SendReservationCancellationAsync(string customerEmail, string customerName, string restaurantName, DateTime reservationDateTime);
        Task SendTestEmailAsync(string toEmail);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendReservationConfirmationAsync(string customerEmail, string customerName, string restaurantName, DateTime reservationDateTime, int guests, int reservationId)
        {
            try
            {
                var subject = $"Reservation Confirmation - {restaurantName}";
                var body = GenerateConfirmationEmailBody(customerName, restaurantName, reservationDateTime, guests, reservationId);
                
                await SendEmailAsync(customerEmail, subject, body);
                _logger.LogInformation($"Confirmation email sent to {customerEmail} for reservation {reservationId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send confirmation email to {customerEmail}: {ex.Message}");
                // Don't throw exception - email failure shouldn't stop reservation creation
            }
        }

        public async Task SendReservationCancellationAsync(string customerEmail, string customerName, string restaurantName, DateTime reservationDateTime)
        {
            try
            {
                var subject = $"Reservation Cancelled - {restaurantName}";
                var body = GenerateCancellationEmailBody(customerName, restaurantName, reservationDateTime);
                
                await SendEmailAsync(customerEmail, subject, body);
                _logger.LogInformation($"Cancellation email sent to {customerEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send cancellation email to {customerEmail}: {ex.Message}");
            }
        }

        public async Task SendTestEmailAsync(string toEmail)
        {
            try
            {
                var subject = "🧪 DineSure Email Test";
                var body = $"""
                    <html>
                    <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
                        <div style="max-width: 600px; margin: 0 auto; padding: 20px;">
                            <h2 style="color: #007bff;">🧪 Email Configuration Test</h2>
                            
                            <p>Hello!</p>
                            
                            <p>If you're reading this email, congratulations! Your DineSure email configuration is working correctly.</p>
                            
                            <div style="background-color: #d4edda; border: 1px solid #c3e6cb; border-radius: 5px; padding: 15px; margin: 20px 0;">
                                <h3 style="margin-top: 0; color: #155724;">✅ Email Service Status: Active</h3>
                                <p>Your reservation confirmation emails will now be sent successfully!</p>
                            </div>
                            
                            <p>You can now make restaurant reservations and receive:</p>
                            <ul>
                                <li>✅ Instant confirmation emails</li>
                                <li>📅 Reservation details</li>
                                <li>🔗 Direct links to confirm or cancel</li>
                            </ul>
                            
                            <hr style="margin: 30px 0; border: none; border-top: 1px solid #eee;">
                            
                            <p style="font-size: 12px; color: #666;">
                                This is an automated test email from DineSure.<br>
                                Time sent: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
                            </p>
                        </div>
                    </body>
                    </html>
                    """;
                
                await SendEmailAsync(toEmail, subject, body);
                _logger.LogInformation($"Test email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send test email to {toEmail}: {ex.Message}");
                throw;
            }
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var isDevelopment = _configuration.GetValue<bool>("EmailService:UseDevelopmentMode", true);
            
            if (isDevelopment)
            {
                // Development mode - just log the email
                _logger.LogInformation($"""
                    
                    ================================================
                    📧 EMAIL CONFIRMATION (DEVELOPMENT MODE)
                    ================================================
                    To: {toEmail}
                    Subject: {subject}
                    
                    ✅ Email would be sent in production
                    📝 This is logged for development purposes
                    
                    Email Content:
                    {body}
                    ================================================
                    
                    """);
                
                // Also log to console for visibility
                Console.WriteLine($"📧 [EMAIL SERVICE] Confirmation email logged for {toEmail}");
                await Task.CompletedTask;
            }
            else
            {
                // Production mode - send actual email
                var smtpHost = _configuration["EmailService:SmtpHost"];
                var smtpPort = _configuration.GetValue<int>("EmailService:SmtpPort", 587);
                var username = _configuration["EmailService:Username"];
                var password = _configuration["EmailService:Password"];
                var fromEmail = _configuration["EmailService:FromEmail"];
                var fromName = _configuration["EmailService:FromName"] ?? "DineSure";

                _logger.LogInformation($"Attempting to send email to {toEmail} via {smtpHost}:{smtpPort}");

                try
                {
                    using var client = new SmtpClient(smtpHost, smtpPort)
                    {
                        Credentials = new NetworkCredential(username, password),
                        EnableSsl = true,
                        Timeout = 30000 // 30 seconds timeout
                    };

                    var message = new MailMessage()
                    {
                        From = new MailAddress(fromEmail ?? username ?? "noreply@dinesure.com", fromName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };
                    
                    message.To.Add(toEmail);

                    await client.SendMailAsync(message);
                    _logger.LogInformation($"✅ Email sent successfully to {toEmail}");
                    Console.WriteLine($"📧 [EMAIL SERVICE] ✅ Email sent successfully to {toEmail}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"❌ Failed to send email to {toEmail}: {ex.Message}");
                    Console.WriteLine($"📧 [EMAIL SERVICE] ❌ Failed to send email: {ex.Message}");
                    throw; // Re-throw to let calling code handle the error
                }
            }
        }

        private string GenerateConfirmationEmailBody(string customerName, string restaurantName, DateTime reservationDateTime, int guests, int reservationId)
        {
            var confirmUrl = $"{_configuration["ApplicationUrl"]}/reservation/confirm/{reservationId}";
            var cancelUrl = $"{_configuration["ApplicationUrl"]}/reservation/cancel/{reservationId}";

            return $"""
                <html>
                <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
                    <div style="max-width: 600px; margin: 0 auto; padding: 20px;">
                        <h2 style="color: #007bff;">🎉 Reservation Confirmation</h2>
                        
                        <p>Dear {customerName},</p>
                        
                        <p>Thank you for choosing <strong>{restaurantName}</strong>! Your reservation has been received and is currently pending confirmation.</p>
                        
                        <div style="background-color: #f8f9fa; border-left: 4px solid #007bff; padding: 15px; margin: 20px 0;">
                            <h3 style="margin-top: 0; color: #007bff;">Reservation Details</h3>
                            <p><strong>Restaurant:</strong> {restaurantName}</p>
                            <p><strong>Date & Time:</strong> {reservationDateTime:dddd, MMMM dd, yyyy} at {reservationDateTime:h:mm tt}</p>
                            <p><strong>Number of Guests:</strong> {guests}</p>
                            <p><strong>Reservation ID:</strong> #{reservationId}</p>
                            <p><strong>Status:</strong> <span style="color: #ffc107;">Pending Confirmation</span></p>
                        </div>
                        
                        <div style="text-align: center; margin: 30px 0;">
                            <a href="{confirmUrl}" style="background-color: #28a745; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin-right: 10px;">✅ Confirm Reservation</a>
                            <a href="{cancelUrl}" style="background-color: #dc3545; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px;">❌ Cancel Reservation</a>
                        </div>
                        
                        <p><strong>What happens next?</strong></p>
                        <ul>
                            <li>Click "Confirm Reservation" to finalize your booking</li>
                            <li>The restaurant will be notified once you confirm</li>
                            <li>You can cancel anytime before your reservation date</li>
                        </ul>
                        
                        <hr style="margin: 30px 0; border: none; border-top: 1px solid #eee;">
                        
                        <p style="font-size: 12px; color: #666;">
                            If you didn't make this reservation, please ignore this email or contact us immediately.
                        </p>
                        
                        <p style="font-size: 12px; color: #666;">
                            Best regards,<br>
                            The RestaurantsByMe Team
                        </p>
                    </div>
                </body>
                </html>
                """;
        }

        private string GenerateCancellationEmailBody(string customerName, string restaurantName, DateTime reservationDateTime)
        {
            return $"""
                <html>
                <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
                    <div style="max-width: 600px; margin: 0 auto; padding: 20px;">
                        <h2 style="color: #dc3545;">❌ Reservation Cancelled</h2>
                        
                        <p>Dear {customerName},</p>
                        
                        <p>Your reservation at <strong>{restaurantName}</strong> has been cancelled.</p>
                        
                        <div style="background-color: #f8d7da; border-left: 4px solid #dc3545; padding: 15px; margin: 20px 0;">
                            <h3 style="margin-top: 0; color: #dc3545;">Cancelled Reservation</h3>
                            <p><strong>Restaurant:</strong> {restaurantName}</p>
                            <p><strong>Date & Time:</strong> {reservationDateTime:dddd, MMMM dd, yyyy} at {reservationDateTime:h:mm tt}</p>
                            <p><strong>Status:</strong> <span style="color: #dc3545;">Cancelled</span></p>
                        </div>
                        
                        <p>We're sorry to see you go! If you'd like to make a new reservation, feel free to visit our website anytime.</p>
                        
                        <hr style="margin: 30px 0; border: none; border-top: 1px solid #eee;">
                        
                        <p style="font-size: 12px; color: #666;">
                            Best regards,<br>
                            The RestaurantsByMe Team
                        </p>
                    </div>
                </body>
                </html>
                """;
        }
    }
} 