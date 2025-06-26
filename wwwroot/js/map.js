// Map.js loaded
console.log('Map.js loaded successfully');

// Favorites refresh callback functionality
let favoritesRefreshCallback = null;

window.setFavoritesRefreshCallback = function (dotNetRef)
{
    favoritesRefreshCallback = dotNetRef;
    console.log('Favorites refresh callback set');
}

window.refreshFavorites = function ()
{
    if (favoritesRefreshCallback)
    {
        favoritesRefreshCallback.invokeMethodAsync('RefreshFavorites')
            .catch(err => console.error('Error refreshing favorites:', err));
    }
}

// Form submission for logout
window.submitLogoutForm = function ()
{
    try
    {
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = '/Identity/Account/Logout';

        // Add anti-forgery token if available
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (token)
        {
            const tokenInput = document.createElement('input');
            tokenInput.type = 'hidden';
            tokenInput.name = '__RequestVerificationToken';
            tokenInput.value = token.value;
            form.appendChild(tokenInput);
        }

        document.body.appendChild(form);
        form.submit();
        return true;
    } catch (error)
    {
        console.error('Error submitting logout form:', error);
        return false;
    }
}

// Original map function for the dedicated map page
function initializeMap(restaurants)
{
    const map = L.map('map').setView([44.4268, 26.1025], 13); // București

    // Setăm tile-urile de la OpenStreetMap
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 18,
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);

    // Adăugăm restaurantele pe hartă
    restaurants.forEach(r =>
    {
        if (r.latitude && r.longitude)
        {
            const marker = L.marker([r.latitude, r.longitude]).addTo(map);
            marker.bindPopup(`
                <strong>${r.name}</strong><br>
                ${r.address}<br>
                ${r.phone ? `<i class='fas fa-phone'></i> ${r.phone}` : ''}
            `);
        }
    });
}

// New enhanced map function for the restaurant page
function initializeRestaurantMap(restaurants, centerLat = 44.4268, centerLon = 26.1025)
{
    console.log('initializeRestaurantMap called with', restaurants.length, 'restaurants');

    // Clear existing map if it exists
    if (window.restaurantMap)
    {
        window.restaurantMap.remove();
    }

    // Create new map
    window.restaurantMap = L.map('restaurantMap').setView([centerLat, centerLon], 13);

    // Add tile layer
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 18,
        attribution: '© OpenStreetMap contributors'
    }).addTo(window.restaurantMap);

    // Icons for different states
    const normalIcon = L.divIcon({
        className: 'custom-marker normal-marker',
        html: '<i class="fas fa-utensils"></i>',
        iconSize: [30, 30],
        iconAnchor: [15, 15]
    });

    const selectedIcon = L.divIcon({
        className: 'custom-marker selected-marker',
        html: '<i class="fas fa-utensils"></i>',
        iconSize: [35, 35],
        iconAnchor: [17.5, 17.5]
    });

    // Add markers for all restaurants
    const markers = [];
    restaurants.forEach(r =>
    {
        if (r.latitude && r.longitude)
        {
            const icon = r.isSelected ? selectedIcon : normalIcon;
            const marker = L.marker([r.latitude, r.longitude], { icon: icon }).addTo(window.restaurantMap);

            // Create popup content
            let popupContent = `
                <div class="restaurant-popup">
                    <h6 class="popup-title">${r.name}</h6>
                    <p class="popup-address"><i class="fas fa-map-marker-alt"></i> ${r.address}</p>
            `;

            if (r.phone)
            {
                popupContent += `<p class="popup-phone"><i class="fas fa-phone"></i> <a href="tel:${r.phone}">${r.phone}</a></p>`;
            }

            if (r.email)
            {
                popupContent += `<p class="popup-email"><i class="fas fa-envelope"></i> <a href="mailto:${r.email}">${r.email}</a></p>`;
            }

            if (r.website)
            {
                popupContent += `<p class="popup-website"><i class="fas fa-globe"></i> <a href="${r.website}" target="_blank">Visit Website</a></p>`;
            }

            popupContent += '</div>';

            marker.bindPopup(popupContent);
            markers.push(marker);

            // Open popup if this is the selected restaurant
            if (r.isSelected)
            {
                marker.openPopup();
            }
        }
    });

    // Fit map to show all markers if there are multiple
    if (markers.length > 1)
    {
        const group = new L.featureGroup(markers);
        window.restaurantMap.fitBounds(group.getBounds().pad(0.1));
    }

    console.log('Map initialized successfully with', markers.length, 'markers');
}
