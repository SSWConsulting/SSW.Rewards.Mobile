window.downloadApp = window.downloadApp || {};

window.downloadApp.detectTarget = function () {
    const userAgent = (navigator.userAgent || "").toLowerCase();
    const platform = ((navigator.userAgentData && navigator.userAgentData.platform) || navigator.platform || "").toLowerCase();
    const touchPoints = navigator.maxTouchPoints || 0;

    const isAndroid = userAgent.includes("android");
    const isIOS = /iphone|ipad|ipod/.test(userAgent) || (platform.includes("mac") && touchPoints > 1);

    if (isAndroid) {
        return "android";
    }

    if (isIOS) {
        return "ios";
    }

    return "unknown";
};
