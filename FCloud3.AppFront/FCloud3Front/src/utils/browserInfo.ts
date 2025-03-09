import Bowser from "bowser";

const browserInfo = Bowser.parse(window.navigator.userAgent);
const isFireFox = browserInfo.browser.name?.toLowerCase() === "firefox";

export {
    browserInfo, isFireFox
};