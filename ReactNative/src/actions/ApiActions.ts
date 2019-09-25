import { ApiTypes } from "./types";
import { getWeatherForecast } from '../api/weather';

export const getWeatherForecastAsync = () => {
    return async dispatch => {
        dispatch(getWeatherForecastAttempt());
        try {
            let data = await getWeatherForecast();
            dispatch(getWeatherForecastSuccess(data));
        } catch (error) {
            dispatch(getWeatherForecastFailure(error.message));
        }
    }
}
export const getWeatherForecastAttempt = () => {
    return {
        type: ApiTypes.GET_WEATHER_FORECAST_ATTEMPT
    }
}
export const getWeatherForecastSuccess = (data) => {
    return {
        type: ApiTypes.GET_WEATHER_FORECAST_SUCCESS,
        payload: data
    }
}
export const getWeatherForecastFailure = (errorMessage) => {
    return {
        type: ApiTypes.GET_WEATHER_FORECAST_FAILURE,
        payload: errorMessage
    }
}