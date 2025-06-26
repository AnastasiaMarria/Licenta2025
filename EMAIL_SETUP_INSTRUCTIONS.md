# 📧 Email Configuration Setup for DineSure

## Quick Setup Guide

### Step 1: Create Gmail App Password

1. **Go to Google Account Settings**: https://myaccount.google.com/
2. **Enable 2-Factor Authentication** (if not already enabled):
   - Security → 2-Step Verification → Get Started
3. **Create App Password**:
   - Security → 2-Step Verification → App passwords
   - Select app: "Mail"
   - Select device: "Windows Computer" 
   - Click "Generate"
   - **Copy the 16-character password** (e.g., `abcd efgh ijkl mnop`)

### Step 2: Update Configuration

Open `appsettings.Development.json` and replace:
```json
{
  "EmailService": {
    "UseDevelopmentMode": false,
    "FromEmail": "your-email@gmail.com",
    "Username": "your-email@gmail.com", 
    "Password": "your-16-character-app-password"
  }
}
```

### Step 3: Test the Email

1. Run the application: `dotnet run`
2. Make a reservation
3. Check your email inbox!

## Alternative: Use Your Current Email

If you want to use `anastasia.alexandru06@gmail.com`:

1. Generate app password for that account
2. Update `appsettings.Development.json`:
```json
{
  "EmailService": {
    "UseDevelopmentMode": false,
    "FromEmail": "anastasia.alexandru06@gmail.com",
    "Username": "anastasia.alexandru06@gmail.com",
    "Password": "your-app-password-here"
  }
}
```

## Security Notes

- ✅ Never commit real passwords to Git
- ✅ Use App Passwords, not your regular Gmail password
- ✅ The `appsettings.Development.json` is for local development only

## Troubleshooting

If emails don't send:
1. Check console for error messages
2. Verify 2FA is enabled on Gmail
3. Ensure App Password is correct (16 characters, no spaces)
4. Check spam folder

## Current Email Template

Users will receive professional emails with:
- 🎉 Reservation confirmation details
- 📅 Date, time, restaurant info
- ✅ Confirm reservation button
- ❌ Cancel reservation button
- 🆔 Reservation ID for reference

Ready to receive real emails! 📧 