import React from 'react';
import { Container, Content, Icon, Item as FormItem, Input, Label, Text, Button, View, List, ListItem, Header, Left, Right, Body, Title } from 'native-base'
import Analytics from 'appcenter-analytics';
import Auth from 'appcenter-auth'
import axios from "axios";
import { StyleSheet } from 'react-native';

export default class LoginScreen extends React.Component {
    state = {
        isLoggedIn: false,
        loadingSignIn: false,
        username: '',
        loadingApi: false,
        token: null,
        data: []
    }

    onLoginPressed = () => {
        Analytics.trackEvent('Login clicked', { Category: 'Sign in', name: 'test' })
        console.log('pressed login');
        let s = this.state;
        if (this.state.isLoggedIn) {
            Auth.signOut();
            s = this.state;
            s.isLoggedIn = false;
            s.data = [];
            this.setState(s);
        } else {
            this.signIn();
        }
    }

    async signIn() {
        try {
            let s = this.state;
            s.loadingSignIn = true;
            this.setState(s);
            const userInformation = await Auth.signIn();
            console.log('user is logged in', userInformation)
            const parsedToken = userInformation.idToken.split('.');
            const rawPayload = parsedToken[1];
            const decodedPayload = atob(rawPayload);
            const claims = JSON.parse(decodedPayload);
            console.log('claims', claims);
            const { name, email } = claims;

            let state = this.state;
            state.isLoggedIn = true;
            state.username = name;
            state.token = userInformation.idToken;
            state.loadingSignIn = false;
            this.setState(state);
        } catch (e) {
            let state = this.state;
            state.loadingSignIn = false;
            this.setState(state);
            console.log('Log failure', e)
        }
    }

    testApi = () => {
        let config = {
            headers: {
                Authorization: 'Bearer ' + this.state.token
            }
        }
        let s = this.state;
        s.loadingApi = true;
        this.setState(s);
        console.log('Sending config', config)
        let api = 'https://b2csampleapi20190919105829.azurewebsites.net/weatherforecast';
        console.log('Clicked test api:', api)
        axios.get(api, config)
            .then(res => {
                console.log('Response', res);
                let state = this.state;
                state.data = res.data;
                this.setState(state);
            })
            .catch(err => console.log('API Error:', err))
            .finally(() => {
                let s = this.state;
                s.loadingApi = false;
                this.setState(s);
            });
    }

    render() {
        return (
            <Container style={styles.container}>
                <Header>
                    <Left>
                    <Button transparent>
                        <Icon name="arrow-back" />
                    </Button>
                    </Left>
                    <Body>
                    <Title>Header</Title>
                    </Body>
                    <Right>
                    <Button transparent>
                        <Icon name="menu" />
                    </Button>
                    </Right>
                </Header>
                <Content>
                    <View padder>
                        <Text>You are{this.state.isLoggedIn ? '' : ' not'} logged in{this.state.isLoggedIn ? ': ' + this.state.username : ''}</Text>
                    </View>
                    <View padder>
                        <Button
                            block
                            rounded
                            success
                            onPress={this.onLoginPressed}
                        >
                            <Icon name={this.state.loadingSignIn ? 'ios-refresh' : "ios-mail" } color="white" fontSize={30} />
                            <Text>{this.state.isLoggedIn ? 'Sign Out' : 'Sign In'}</Text>
                        </Button>
                    </View>
                    {this.state.isLoggedIn ?
                    <View padder>
                        <Button
                            block
                            rounded
                            success
                            onPress={this.testApi}
                        >
                            <Icon name={this.state.loadingApi ? 'ios-refresh' : "ios-cloud"} color="white" fontSize={30} />
                            <Text>Test API</Text>
                        </Button>
                        <List>
                            {this.state.data.map((item, index) => (
                                <ListItem key={index}><Text>{item.temperatureC} C - {item.summary} - {new Date(item.date).toDateString()}</Text></ListItem>
                            ))}
                        </List>
                    </View>
                    : null }
                </Content>
            </Container>
        );
    }
}

const styles = StyleSheet.create({
    container: {
      width: '100%'
    }
  });