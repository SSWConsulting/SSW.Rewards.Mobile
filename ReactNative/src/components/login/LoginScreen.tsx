import React from 'react';
import { Container, Content, Icon, Item as FormItem, Input, Label, Text, Button, View, List, ListItem } from 'native-base'
import Analytics from 'appcenter-analytics';
import Auth from 'appcenter-auth'
import axios from "axios";

export default class LoginScreen extends React.Component {
    state = {
        isLoggedIn: false,
        username: '',
        token: null,
        data: []
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
        let config = {
            headers: {
                Authorization: 'Bearer ' + this.state.token
            }
        }
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
                        <List>
                            {this.state.data.map((item, index) => (
                                <ListItem key={index}><Text>{item.temperatureC} - {item.summary} - {new Date(item.date).toDateString()}</Text></ListItem>
                            ))}
                        </List>
                    </View>
                    : null }
                </Content>
            </Container>
        );
    }
}
