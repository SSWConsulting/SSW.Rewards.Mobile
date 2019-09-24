import { Platform } from 'react-native';

const CONSTANTS = {
    IS_ENV_DEVELOPMENT: __DEV__,
    IS_ANDROID: Platform.OS === "android",
    IS_IOS: Platform.OS === "ios",
    IS_DEBUG_MODE_ENABLED: Boolean(window.navigator.userAgent),
    API_URL: 'https://b2csampleapi20190919105829.azurewebsites.net',

    AUTH_TOKEN: 'AUTH_TOKEN'
}

export default CONSTANTS;