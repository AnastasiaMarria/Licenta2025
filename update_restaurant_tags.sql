-- Temporary script to add dietary tags to restaurants for testing
-- Run this manually in SQL Server Management Studio or VS Code SQL extension

-- Update a few restaurants with different dietary options
UPDATE Restaurants 
SET SupportedDietaryTypes = '["Vegetarian", "Mediterranean"]',
    AllergenFreeOptions = '["Milk", "Eggs", "Peanuts"]',
    HasVegetarianOptions = 1,
    HasLactoseFreeOptions = 1
WHERE Id IN (1, 2, 3, 4, 5);

UPDATE Restaurants 
SET SupportedDietaryTypes = '["Vegan", "LowCarb"]',
    AllergenFreeOptions = '["Milk", "Eggs", "Wheat", "Soy"]',
    HasVeganOptions = 1,
    HasGlutenFreeOptions = 1,
    HasLactoseFreeOptions = 1
WHERE Id IN (6, 7, 8, 9, 10);

UPDATE Restaurants 
SET SupportedDietaryTypes = '["Paleo", "Keto"]',
    AllergenFreeOptions = '["Wheat", "Soy", "TreeNuts"]',
    HasGlutenFreeOptions = 1,
    HasDiabeticFriendlyOptions = 1
WHERE Id IN (11, 12, 13, 14, 15);

UPDATE Restaurants 
SET SupportedDietaryTypes = '["Pescatarian", "Mediterranean"]',
    AllergenFreeOptions = '["Shellfish", "TreeNuts"]',
    HasLactoseFreeOptions = 1
WHERE Id IN (16, 17, 18, 19, 20);

UPDATE Restaurants 
SET SupportedDietaryTypes = '["Mediterranean"]',
    AllergenFreeOptions = '["Peanuts", "TreeNuts", "Sesame"]',
    HasHalalOptions = 1
WHERE Id IN (21, 22, 23, 24, 25);

UPDATE Restaurants 
SET SupportedDietaryTypes = '["Vegetarian"]',
    AllergenFreeOptions = '["Fish", "Shellfish", "Soy"]',
    HasKosherOptions = 1,
    HasVegetarianOptions = 1
WHERE Id IN (26, 27, 28, 29, 30);

-- Add tags to many more restaurants with random distributions
UPDATE Restaurants 
SET SupportedDietaryTypes = '["Vegetarian"]',
    AllergenFreeOptions = '["Milk"]',
    HasVegetarianOptions = 1,
    HasLactoseFreeOptions = 1
WHERE Id % 7 = 1 AND SupportedDietaryTypes = '[]';

UPDATE Restaurants 
SET SupportedDietaryTypes = '["Vegan"]',
    AllergenFreeOptions = '["Milk", "Eggs"]',
    HasVeganOptions = 1,
    HasLactoseFreeOptions = 1
WHERE Id % 7 = 2 AND SupportedDietaryTypes = '[]';

UPDATE Restaurants 
SET SupportedDietaryTypes = '["Mediterranean"]',
    AllergenFreeOptions = '["Peanuts"]',
    HasDiabeticFriendlyOptions = 1
WHERE Id % 7 = 3 AND SupportedDietaryTypes = '[]';

UPDATE Restaurants 
SET SupportedDietaryTypes = '["Paleo"]',
    AllergenFreeOptions = '["Wheat"]',
    HasGlutenFreeOptions = 1
WHERE Id % 7 = 4 AND SupportedDietaryTypes = '[]';

UPDATE Restaurants 
SET SupportedDietaryTypes = '["Keto"]',
    AllergenFreeOptions = '["Soy"]',
    HasDiabeticFriendlyOptions = 1
WHERE Id % 7 = 5 AND SupportedDietaryTypes = '[]';

UPDATE Restaurants 
SET SupportedDietaryTypes = '["LowCarb"]',
    AllergenFreeOptions = '["TreeNuts"]',
    HasDiabeticFriendlyOptions = 1
WHERE Id % 7 = 6 AND SupportedDietaryTypes = '[]';

UPDATE Restaurants 
SET SupportedDietaryTypes = '["Pescatarian"]',
    AllergenFreeOptions = '["Shellfish"]',
    HasLactoseFreeOptions = 1
WHERE Id % 7 = 0 AND SupportedDietaryTypes = '[]'; 