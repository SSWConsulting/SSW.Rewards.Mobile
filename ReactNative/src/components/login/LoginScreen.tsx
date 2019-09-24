import React from 'react';
import { Container, Content, Icon, Text, Button, View, List, ListItem } from 'native-base'
import Auth from 'appcenter-auth'
import axios from 'axios';
import { StyleSheet } from 'react-native';
import { connect } from 'react-redux';
import LoginHeader from './LoginHeader';
import { signInAsync, signOutAsync } from '../../actions/AuthActions';

class LoginScreen extends React.Component {
    state = {
        token: null,
        loadingApi: false,
        data: []
    }

    onLoginPressed = () => {
        console.log('pressed login with dispatch');
        if (this.props.user) {
            this.props.signOutAsync();
        } else {
            this.props.signInAsync();
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
                <LoginHeader />
                <Content>
                    <View padder>
                        <Text>You are{this.props.user ? '' : ' not'} logged in{this.props.user ? ': ' + this.props.user.name : ''}</Text>
                    </View>
                    <View padder>
                        <Button
                            block
                            rounded
                            success
                            onPress={this.onLoginPressed}
                        >
                            <Icon name={this.props.loadingLogin ? 'ios-refresh' : "ios-mail" } color="white" fontSize={30} />
                            <Text>{this.props.user ? 'Sign Out' : 'Sign In'}</Text>
                        </Button>
                    </View>
                    {this.props.user ?
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

const mapStateToProps = ({ auth }) => {
    return {
        user: auth.user,
        loadingLogin: auth.loadingLogin,
        loadingErrorMessage: auth.loadingErrorMessage
    };
}

const mapDispatchToProps = dispatch => {
    return {
        signInAsync: () => dispatch(signInAsync()),
        signOutAsync: () => dispatch(signOutAsync())
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(LoginScreen);