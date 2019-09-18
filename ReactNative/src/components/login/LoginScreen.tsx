import React from 'react';
import { Container, Content, Icon, Item as FormItem, Input, Label, Text, Button, View } from 'native-base'
import Analytics from 'appcenter-analytics';
import Auth from 'appcenter-auth'
import axios from "axios";

export default class LoginScreen extends React.Component {
    state = {
        isLoggedIn: false,
        username: '',
        token: null
    }

    onLoginPressed = () => {
        Analytics.trackEvent('Login clicked', { Category: 'Sign in', name: 'test' })
        console.log('pressed login');
        if (this.state.isLoggedIn) {
            Auth.signOut();
            let state = this.state;
            state.isLoggedIn = false;
            this.setState(state);
        } else {
            this.signIn();
        }
    }

    async signIn() {
        try {
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
            this.setState(state);
        } catch (e) {
            console.log('Log failure', e)
        }
    }

    testApi = () => {
        // let config = {
        //     headers: {
        //         Authorization: 'Bearer ' + this.state.token
        //     }
        // }
        // console.log('Sending config', config)

        axios.get('https://10.0.2.2:44375/weatherforecast')
            .then(res => {
                console.log('Response', res);
            })
            .catch(err => console.log('API Error:', err));
    }

    render() {
        return (
            <Container>
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
                            <Icon name="ios-mail" color="white" fontSize={30} />
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
                            <Icon name="ios-cloud" color="white" fontSize={30} />
                            <Text>Test API</Text>
                        </Button>
                    </View>
                    : null }
                </Content>
            </Container>
        );
    }
}
