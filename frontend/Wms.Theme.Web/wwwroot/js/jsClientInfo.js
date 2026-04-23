function getBrowserName(userAgent) {
    if (/edg/i.test(userAgent)) {
        return "Edge";
    } else if (/chrome|crios|crmo/i.test(userAgent) && !/edg/i.test(userAgent)) {
        return "Chrome";
    } else if (/firefox|fxios/i.test(userAgent)) {
        return "Firefox";
    } else if (/safari/i.test(userAgent) && !/chrome/i.test(userAgent)) {
        return "Safari";
    } else if (/msie|trident/i.test(userAgent)) {
        return "Internet Explorer";
    } else {
        return "Unknown";
    }
}

function getBrowserVersion(ua) {
    if (/edg/i.test(ua)) {
        const match = ua.match(/Edg\/(\d+\.\d+)/);
        return match ? match[1] : "";
    } else if (/chrome|crios|crmo/i.test(ua) && !/edg/i.test(ua)) {
        const match = ua.match(/Chrome\/(\d+\.\d+)/);
        return match ? match[1] : "";
    } else if (/firefox|fxios/i.test(ua)) {
        const match = ua.match(/Firefox\/(\d+\.\d+)/);
        return match ? match[1] : "";
    } else if (/safari/i.test(ua) && !/chrome/i.test(ua)) {
        const match = ua.match(/Version\/(\d+\.\d+)/);
        return match ? match[1] : "";
    } else {
        return "";
    }
}

function getOS() {
    const ua = navigator.userAgent;
    const platform = navigator.platform;

    if (/windows nt 10/i.test(ua)) {
        return "Windows 10/11";
    } else if (/windows nt 6\.3/i.test(ua)) {
        return "Windows 8.1";
    } else if (/windows nt 6\.2/i.test(ua)) {
        return "Windows 8";
    } else if (/windows nt 6\.1/i.test(ua)) {
        return "Windows 7";
    } else if (/macintosh|mac os x/i.test(ua)) {
        return "macOS";
    } else if (/android/i.test(ua)) {
        return "Android";
    } else if (/iphone|ipad|ipod/i.test(ua)) {
        return "iOS";
    } else if (/linux/i.test(platform)) {
        return "Linux";
    } else {
        return "Unknown OS";
    }
}

// util state function
function debounce(fn, delay, immediate = false) {
    let timer;

    return function (...args) {
        const context = this;

        const callNow = immediate && !timer;

        clearTimeout(timer);

        timer = setTimeout(() => {
            timer = null;
            if (!immediate) fn.apply(context, args);
        }, delay);

        if (callNow) fn.apply(context, args);
    };
}

// Attach to window object to ensure global availability
window.debounce = debounce;
window.getBrowserName = getBrowserName;
window.getBrowserVersion = getBrowserVersion;
window.getOS = getOS;