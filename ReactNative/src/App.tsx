import React from 'react';
import { StyleSheet } from 'react-native';
import { Container } from 'native-base';
import LoginScreen from './components/login/LoginScreen'
import {Provider} from 'react-redux';
import store from './store';

export default function App() {
  return (
    <Provider store={store}>
      <Container style={styles.container}>
        <LoginScreen />
      </Container>
    </Provider>
  );
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
