// Service Worker Registration for PWA
if ('serviceWorker' in navigator) {
    window.addEventListener('load', function() {
        navigator.serviceWorker.register('/static/shared/js/sw.js')
            .then(function(registration) {
                console.log('ServiceWorker registration successful with scope: ', registration.scope);
            }, function(err) {
                console.log('ServiceWorker registration failed: ', err);
            });
    });
}