/**
 * Roblox Revival - Main JavaScript
 */

// API Helper
const API = {
    async get(url) {
        const response = await fetch(url);
        return response.json();
    },
    
    async post(url, data) {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data),
        });
        return response.json();
    }
};

// Check if user is authenticated
async function checkAuth() {
    try {
        const response = await fetch('/Account/CheckAuth');
        return await response.json();
    } catch (error) {
        console.error('Auth check failed:', error);
        return { authenticated: false };
    }
}

// Play a game
async function playGame(placeId) {
    if (!placeId) {
        showError('This game has no playable places.');
        return;
    }

    const auth = await checkAuth();
    
    if (!auth.authenticated) {
        // Redirect to login with return URL
        window.location.href = `/Account/Login?returnUrl=${encodeURIComponent('/Game/PlaceLauncher.ashx?placeId=' + placeId)}`;
        return;
    }

    try {
        const response = await fetch(`/Game/PlaceLauncher.ashx?placeId=${placeId}`);
        const text = await response.text();

        if (text.startsWith('ERROR:')) {
            showError(text.replace('ERROR: ', ''));
            return;
        }

        // Parse response: machineAddress|machinePort|placeId|gameId|serverId|ticket|placeVersionId
        const parts = text.split('|');
        if (parts.length >= 5) {
            const serverInfo = {
                address: parts[0],
                port: parseInt(parts[1]),
                serverId: parts[4],
                ticket: parts[5]
            };
            
            // Connect to server via Roblox client protocol
            connectToServer(serverInfo);
        }
    } catch (error) {
        console.error('Play game error:', error);
        showError('Failed to launch game. Please try again.');
    }
}

// Connect to game server
function connectToServer(serverInfo) {
    // This would typically launch the Roblox client with connection parameters
    // For web-based games, this would establish a WebSocket connection
    
    // Example: window.location.href = `roblox://connect?server=${serverInfo.serverId}&ticket=${serverInfo.ticket}`;
    
    console.log('Connecting to server:', serverInfo);
    
    // For demo purposes, show connection info
    showInfo(`Connecting to ${serverInfo.address}:${serverInfo.port}...`);
}

// Show error message
function showError(message) {
    alert('Error: ' + message);
}

// Show info message
function showInfo(message) {
    alert(message);
}

// Format number with commas
function formatNumber(num) {
    return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    // Add any initialization code here
    console.log('Roblox Revival initialized');
});