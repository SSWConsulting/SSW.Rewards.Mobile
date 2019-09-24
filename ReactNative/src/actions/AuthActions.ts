import Auth from 'appcenter-auth'
import { AuthTypes } from './types';
import { setAuthToken } from '../services/AuthService';

export const signInAsync = () => {
    return async dispatch => {
        dispatch(signInAttempt());
        try {
            const userInformation = await Auth.signIn();
            setAuthToken(userInformation.idToken);
            const parsedToken = userInformation.idToken.split('.');
            const payload = atob(parsedToken[1]);
            dispatch(signInSuccess(JSON.parse(payload)));
        } catch (error) {
            console.log(error);
            dispatch(signInFailure(error.message));
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
    console.log('sign in success:', payload)
    return {
        type: AuthTypes.SIGN_IN_SUCCESS,
        payload: payload
    }
}
export const signOutAsync = () => {
    return dispatch => {
        Auth.signOut();
        setAuthToken(null);
        dispatch(signOut())
    }
}
export const signOut = () => {
    return {
        type: AuthTypes.SIGN_OUT
    }
}