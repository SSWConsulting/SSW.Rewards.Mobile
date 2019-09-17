import React from 'react';
import { StyleSheet, Text, View, Button, TextInput } from 'react-native';
import LoginScreen from './components/login/LoginScreen'

export default function App() {


  return (
    <View style={styles.container}>
      <LoginScreen />
    </View>
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
