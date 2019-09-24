import Auth from 'appcenter-auth'
import { AuthTypes } from './types';
import { AsyncStorage } from 'react-native';

export const signInAsync = () => {
    return async dispatch => {
        console.log('dispatching sign in attempt')
        dispatch(signInAttempt());
        try {
            const userInformation = await Auth.signIn();
            const parsedToken = userInformation.idToken.split('.');
            const payload = atob(parsedToken[1]);
            dispatch(signInSuccess(JSON.parse(payload)));
        } catch (e) {
            dispatch(signInFailure(e.message));
        }
    }
}
export const signInAttempt = () => {
    return {
        type: AuthTypes.SIGN_IN_ATTEMPT
    }
}
export const signInFailure = (errorMessage) => {
    return {
        type: AuthTypes.SIGN_IN_FAILURE,
        payload: errorMessage
    }
}
export const signInSuccess = (payload) => {
    return {
        type: AuthTypes.SIGN_IN_SUCCESS,
        payload: payload
    }
}
export const signOutAsync = () => {
    return dispatch => {
        console.log('signed out');
        Auth.signOut();
        dispatch(signOut())
    }
}
export const signOut = () => {
    return {
        type: AuthTypes.SIGN_OUT
    }
}