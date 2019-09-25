import AsyncStorage from '@react-native-community/async-storage';
import CONSTANTS from '../config/constants'

export const getAuthToken = async () => {
    try {
        let token = await AsyncStorage.getItem(CONSTANTS.AUTH_TOKEN);
        return token;
    } catch (error) {
        console.log('Error fetching auth token', error);
    }
}

export const setAuthToken = async (token) => {
    try {
        console.log('new token:', token)
        await AsyncStorage.setItem(CONSTANTS.AUTH_TOKEN, token);
    } catch (error) {
        console.log('Error fetching auth token', error);
    }
}