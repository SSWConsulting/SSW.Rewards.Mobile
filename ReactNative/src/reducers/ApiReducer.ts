import { ApiTypes } from '../actions/types';

const INITIAL_STATE = {
    loadingApi: false,
    errorMessage: '',
    data: []
}

export default (state = INITIAL_STATE, action) => {
    switch (action.type) {
        case ApiTypes.GET_WEATHER_FORECAST_ATTEMPT:
            return { ...state, errorMessage: '', loadingApi: true }
        case ApiTypes.GET_WEATHER_FORECAST_SUCCESS:
            return { ...INITIAL_STATE, data: action.payload }
        case ApiTypes.GET_WEATHER_FORECAST_FAILURE:
            return { ...INITIAL_STATE, errorMessage: action.payload }
        default:
            return state;
    }
}