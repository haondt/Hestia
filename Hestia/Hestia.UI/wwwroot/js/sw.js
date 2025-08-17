// Simplified Service Worker - No offline functionality
// Just shows a message when internet connection is required

// Install event
self.addEventListener('install', (event) => {
  console.log('Service worker installed');
  self.skipWaiting();
});

// Activate event
self.addEventListener('activate', (event) => {
  console.log('Service worker activated');
  self.clients.claim();
});

// Fetch event - require internet connection
self.addEventListener('fetch', (event) => {
  // Skip cross-origin requests
  if (!event.request.url.startsWith(self.location.origin)) {
    return;
  }

  event.respondWith(
    fetch(event.request)
      .then((response) => {
        // Server responded (even if with 404, 500, etc.) - pass it through
        return response;
      })
      .catch((error) => {
        // Only network failures (no internet) reach this catch block
        // fetch() only throws for network issues, not HTTP status codes
        console.log('Network error detected:', error);
        
        return new Response(
          `<!DOCTYPE html>
          <html lang="en">
          <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>Hestia - Internet Required</title>
            <style>
              body { 
                font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                margin: 0;
                padding: 20px;
                min-height: 100vh;
                display: flex;
                align-items: center;
                justify-content: center;
                background: #f5f5f5;
                color: #333;
              }
              .container {
                text-align: center;
                max-width: 400px;
                padding: 40px;
                background: white;
                border-radius: 12px;
                box-shadow: 0 4px 20px rgba(0,0,0,0.1);
              }
              .icon {
                font-size: 48px;
                margin-bottom: 20px;
                color: #e05f35;
              }
              h1 {
                margin: 0 0 16px 0;
                font-size: 24px;
                font-weight: 600;
                color: #333;
              }
              p {
                margin: 0 0 24px 0;
                font-size: 16px;
                line-height: 1.5;
                color: #666;
              }
              .retry-btn {
                background: #e05f35;
                color: white;
                border: none;
                padding: 12px 24px;
                border-radius: 6px;
                font-size: 16px;
                cursor: pointer;
                transition: background 0.2s;
              }
              .retry-btn:hover {
                background: #c54f2c;
              }
            </style>
          </head>
          <body>
            <div class="container">
              <div class="icon">🔌</div>
              <h1>Internet Connection Required</h1>
              <p>Hestia requires an active internet connection to work properly. Please check your network and try again.</p>
              <button class="retry-btn" onclick="window.location.reload()">Retry</button>
            </div>
          </body>
          </html>`,
          {
            status: 503,
            statusText: 'Service Unavailable',
            headers: { 'Content-Type': 'text/html' }
          }
        );
      })
  );
});