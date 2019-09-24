import axios from 'axios';
import { getAuthToken } from '../services/AuthService';
import CONSTANTS from '../config/constants';

export const getWeatherForecast = async () => {
    let token = await getAuthToken();
    let config = {
        headers: {
            Authorization: 'Bearer ' + token
        }
    }

    try {
        let { data } = await axios.get(`${CONSTANTS.API_URL}/weatherforecast`, config);
        return data;
    } catch (error) {
        console.log('API Error:', error)
        return [];
    }
}