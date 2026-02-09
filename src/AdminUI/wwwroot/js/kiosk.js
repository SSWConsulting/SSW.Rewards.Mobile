window.kioskLeaderboard = window.kioskLeaderboard || {};

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
