import Auth from 'appcenter-auth'
import { AuthTypes } from './types';

export const signInAsync = () => {
    return async dispatch => {
        dispatch(AuthTypes.SIGN_IN_ATTEMPT);
        try {
            const userInformation = await Auth.signIn();
            const parsedToken = userInformation.idToken.split('.');
            const payload = atob(parsedToken[1]);
            dispatch(AuthTypes.SIGN_IN_SUCCESS, JSON.parse(payload));
        } catch (e) {
            dispatch(AuthTypes.SIGN_IN_FAILURE, e.message);
        }
    }
}
export const signInAttempt = () => {
    return {
        type: AuthTypes.SIGN_IN_ATTEMPT
    }
}
export const signInFailure = () => {
    return {
        type: AuthTypes.SIGN_IN_FAILURE
    }
}
export const signInSuccess = () => {
    return {
        type: AuthTypes.SIGN_IN_SUCCESS
    }
}
export const signOut = () => {
    return {
        type: AuthTypes.SIGN_OUT
    }
}