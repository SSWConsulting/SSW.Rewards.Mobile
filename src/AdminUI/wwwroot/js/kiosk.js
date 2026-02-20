window.kioskLeaderboard = window.kioskLeaderboard || {};

// Image preload cache — the kiosk re-renders the MudTable every 10s (page
// scroll) and 60s (data refresh), which recreates <img> DOM elements each
// time. External hosts (GitHub raw, Azure Blob) have inconsistent cache
// headers, causing visible flickering as images re-download. Keeping Image
// objects alive here ensures the browser serves them from memory cache.
window.kioskLeaderboard._preloadedImages = window.kioskLeaderboard._preloadedImages || {};

window.kioskLeaderboard.preloadImages = function (urls) {
    var cache = window.kioskLeaderboard._preloadedImages;
    for (var i = 0; i < urls.length; i++) {
        if (urls[i] && !cache[urls[i]]) {
            var img = new Image();
            img.src = urls[i];
            cache[urls[i]] = img;
        }
    }
};

window.kioskLeaderboard.getViewport = function () {
    return {
        width: window.innerWidth || 0,
        height: window.innerHeight || 0
    };
};

window.kioskLeaderboard._subscriptions = window.kioskLeaderboard._subscriptions || {};

window.kioskLeaderboard.subscribeViewportChanges = function (dotNetRef) {
    const subscriptionId = (window.crypto && window.crypto.randomUUID)
        ? window.crypto.randomUUID()
        : String(Date.now()) + Math.random().toString(16).slice(2);

    let timeoutId = null;

    const notify = function () {
        if (timeoutId) {
            clearTimeout(timeoutId);
        }

        timeoutId = setTimeout(function () {
            dotNetRef.invokeMethodAsync(
                "OnViewportChanged",
                window.innerWidth || 0,
                window.innerHeight || 0
            );
        }, 200);
    };

    window.addEventListener("resize", notify);
    window.addEventListener("orientationchange", notify);

    window.kioskLeaderboard._subscriptions[subscriptionId] = {
        notify: notify
    };

    return subscriptionId;
};

window.kioskLeaderboard.unsubscribeViewportChanges = function (subscriptionId) {
    const subscription = window.kioskLeaderboard._subscriptions[subscriptionId];
    if (!subscription) {
        return;
    }

    window.removeEventListener("resize", subscription.notify);
    window.removeEventListener("orientationchange", subscription.notify);
    delete window.kioskLeaderboard._subscriptions[subscriptionId];
};
