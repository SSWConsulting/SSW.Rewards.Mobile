const INITIAL_STATE = {
    loadingLogin: false,
    loginErrorMessage: '',
    user: null
}

export default (state = INITIAL_STATE, action) => {
    switch (action.type) {
        case 'SIGN_IN_ATTEMPT':
            return { ...state, loginErrorMessage: '', loadingLogin: true }
        case 'SIGN_IN_SUCCESS':
            return { ...INITIAL_STATE, user: action.payload }
        case 'SIGN_IN_FAILURE':
            return { ...state, loginErrorMessage: '', loadingLogin: false }
        default:
            return state;
    }
}