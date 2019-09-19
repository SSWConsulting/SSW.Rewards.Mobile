import React from 'react';
import { StyleSheet, View } from 'react-native';
import { Container, Header, Left, Body, Title, Right, Button, Icon } from 'native-base';
import LoginScreen from './components/login/LoginScreen'

export default function App() {


  return (
    <Container style={styles.container}>
      <LoginScreen />
    </Container>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#fff',
    alignItems: 'center',
    justifyContent: 'center',
  },
  textInput: {
    height: 40
  }
});
