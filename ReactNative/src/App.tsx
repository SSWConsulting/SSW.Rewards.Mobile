import React, { useState, useEffect } from 'react';
import { StyleSheet } from 'react-native';
import { Container, Icon } from 'native-base';
import {Provider} from 'react-redux';
import store from './store';
import { createAppContainer } from 'react-navigation';
import { createBottomTabNavigator } from 'react-navigation-tabs';
import AppIntroSlider from 'react-native-app-intro-slider';
import LoginScreen from './components/login/LoginScreen';
import LeaderboardScreen from './components/leaderboard/LeaderboardScreen';
import AsyncStorage from '@react-native-community/async-storage';
import CONSTANTS from './config/constants';

export default function App() {
  const [isOnboardingComplete, setIsOnboardingComplete] = useState(false);

  useEffect(() => {
    console.log('getting onboarding state from storage');
    const getPersistedState = async () => {
      const value = await AsyncStorage.getItem(CONSTANTS.COMPLETED_ONBOARDING);
      setIsOnboardingComplete(value === '1');
    };
    getPersistedState();
  }, []);

  const renderApp = () => {
    return (
      <Provider store={store}>
        <Router />
      </Provider>
    );
  };

  const onOnboardingDone = async () => {
    console.log('onOnboardingDone: setting state in storage');
    await AsyncStorage.setItem(CONSTANTS.COMPLETED_ONBOARDING, '1');
    setIsOnboardingComplete(true);
  };

  const renderOnboarding = () => {
    return (
      <AppIntroSlider slides={slides} onDone={onOnboardingDone} />
    );
  };

  return isOnboardingComplete ? renderApp() : renderOnboarding();
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#fff',
    alignItems: 'center',
    justifyContent: 'center',
    width: '100%'
  }
});

const slides = [
  {
    key: 'earning-points',
    backgroundColor: '#cc4141',
    image: require('./images/onboarding/earning-points.png'),
    title: 'How to get them?',
    text: 'Talk to SSW people, attend talks QR codes will be shown at the end of the presentations or complete the techquiz in this app.',
  },
  {
    key: 'google-home',
    backgroundColor: '#bcbcbc',
    image: require('./images/onboarding/google-home.png'),
    title: 'Google Home',
    text: 'Get on the top of the leaderboard and win a Google Hub Max or one of the MI Wirst bands',
  },
  {
    key: 'hub-max',
    backgroundColor: '#bcbcbc',
    image: require('./images/onboarding/hub-max.png'),
    title: 'Google Hub Max',
    text: "Get on the top of the leaderboard and win a Google Hub Max or one of the MI Wirst bands",
  },
  {
    key: 'mi-band',
    backgroundColor: '#bcbcbc',
    image: require('./images/onboarding/mi-band.png'),
    title: 'MI Band',
    text: "Get on the top of the leaderboard and win a Google Hub Max or one of the MI Wirst bands",
  },
  {
    key: 'free-consultation',
    backgroundColor: '#bcbcbc',
    image: require('./images/onboarding/free-consultation.png'),
    title: 'Free Consultation',
    text: "A free audit on your .NET CORE and or your Angular application ",
  },
  {
    key: 'net-core-superpowers',
    backgroundColor: '#bcbcbc',
    image: require('./images/onboarding/net-core-superpowers.png'),
    title: '.NET CORE Superpowers',
    text: "Get on the top of the leaderboard and win a Google Hub Max or one of the MI Wirst bands",
  },
];

const bottomTabNavigator = createBottomTabNavigator(
  {
    Login: {
      screen: LoginScreen,
      navigationOptions: {
        tabBarIcon: ({ tintColor }) => (
          <Icon name="home" fontSize={25} color={tintColor} />
        )
      }
    },
    Leaderboard: {
      screen: LeaderboardScreen,
      navigationOptions: {
        tabBarIcon: ({ tintColor }) => (
          <Icon name="star" fontSize={25} color={tintColor} />
        )
      }
    }
  },
  {
    initialRouteName: 'Login'
  }
);

const Router = createAppContainer(bottomTabNavigator)
