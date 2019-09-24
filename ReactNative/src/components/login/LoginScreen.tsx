import React from 'react';
import { Container, Content, Icon, Text, Button, View, List, ListItem } from 'native-base'
import { StyleSheet } from 'react-native';
import { connect } from 'react-redux';
import LoginHeader from './LoginHeader';
import { signInAsync, signOutAsync } from '../../actions/AuthActions';
import { getWeatherForecastAsync } from '../../actions/ApiActions';

class LoginScreen extends React.Component {
    onLoginPressed = () => {
        if (this.props.user) {
            this.props.signOutAsync();
        } else {
            this.props.signInAsync();
        }
    }

    testApi = () => {
        this.props.getWeatherForecast();
    }
    render() {
        return (
            <Container style={styles.container}>
                <LoginHeader />
                <Content>
                    <View padder>
                        <Text>You are{this.props.user ? '' : ' not'} logged in{this.props.user ? `: ${this.props.user.given_name} ${this.props.user.family_name}` : ''}</Text>
                    </View>
                    <View padder>
                        <Button
                            block
                            rounded
                            success
                            onPress={this.onLoginPressed}
                        >
                            <Icon name={this.props.loadingLogin ? 'ios-refresh' : 'ios-mail' } color="white" fontSize={30} />
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
                            <Icon name={this.props.loadingApi ? 'ios-refresh' : 'ios-cloud' } color="white" fontSize={30} />
                            <Text>Test API</Text>
                        </Button>
                        <List>
                            {this.props.data && this.props.data.map((item, index) => (
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

const mapStateToProps = ({ auth, api }) => {
    return {
        user: auth.user,
        loadingLogin: auth.loadingLogin,
        loadingErrorMessage: auth.loadingErrorMessage,
            
        loadingApi: api.loadingApi,
        errorMessage: api.errorMessage,
        data: api.data
    };
}

const mapDispatchToProps = dispatch => {
    return {
        signInAsync: () => dispatch(signInAsync()),
        signOutAsync: () => dispatch(signOutAsync()),
        getWeatherForecast: () => dispatch(getWeatherForecastAsync())
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(LoginScreen);