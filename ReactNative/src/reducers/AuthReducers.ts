import { AuthTypes } from '../actions/types';

const INITIAL_STATE = {
    loadingLogin: false,
    loginErrorMessage: '',
    user: null
}

export default (state = INITIAL_STATE, action) => {
    switch (action.type) {
        case AuthTypes.SIGN_IN_ATTEMPT:
            return { ...state, loginErrorMessage: '', loadingLogin: true }
        case AuthTypes.SIGN_IN_SUCCESS:
            return { ...INITIAL_STATE, user: action.payload }
        case AuthTypes.SIGN_IN_FAILURE:
            return { ...state, loginErrorMessage: action.payload, loadingLogin: false }
        case AuthTypes.SIGN_OUT:
            return { ...INITIAL_STATE }
        default:
            return state;
    }
}