# Restaurant Background Image Setup

## How to Add Your Restaurant Background Image

To use your exact restaurant photograph as the background:

### Step 1: Prepare Your Image
1. Save your restaurant image as `restaurant-background.jpg`
2. For best results, use a high-quality image (at least 1920x1080 pixels)
3. The image should be in JPG format for faster loading

### Step 2: Add the Image to Your Project
1. Copy your `restaurant-background.jpg` file
2. Paste it into the `wwwroot/images/` folder in your project
3. The file path should be: `wwwroot/images/restaurant-background.jpg`

### Step 3: Alternative Background Image
If you don't have the exact image file, you can:
1. Find a similar warm restaurant interior image online
2. Use a stock photo website like Unsplash, Pexels, or Pixabay
3. Search for "restaurant interior", "cozy dining", or "warm restaurant"
4. Download and rename it to `restaurant-background.jpg`

### Current Setup
The home page is now configured to:
- ✅ Use your background image as a full-screen hero background
- ✅ Display "Find the best restaurants" title with search functionality
- ✅ Have a clean search overlay that matches the style from your reference image
- ✅ Remove all animations for a clean, professional look

### File Structure
```
RestaurantsByMe/
├── wwwroot/
│   ├── images/
│   │   └── restaurant-background.jpg  ← Add your image here
│   └── css/
└── Pages/
    └── Index.razor  ← Updated with background image CSS
```

Once you add the image file, refresh your browser to see your exact restaurant photo as the background! 