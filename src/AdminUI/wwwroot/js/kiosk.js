window.kioskLeaderboard = window.kioskLeaderboard || {};

window.kioskLeaderboard.getViewport = function () {
    return {
        width: window.innerWidth || 0,
        height: window.innerHeight || 0
    };
};
