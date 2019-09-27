import React from 'react';
import { Container, Content, Icon, Text, Button, View, List, ListItem } from 'native-base'
import { StyleSheet } from 'react-native';
import { connect } from 'react-redux';
import { signInAsync, signOutAsync } from '../../actions/AuthActions';

class LoginScreen extends React.Component {
    onLoginPressed = () => {
        if (this.props.user) {
            this.props.signOutAsync();
        } else {
            this.props.signInAsync();
        }
    }

    render() {
        return (
            <Container style={styles.container}>
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
        loadingErrorMessage: auth.loadingErrorMessage,
    };
}

const mapDispatchToProps = dispatch => {
    return {
        signInAsync: () => dispatch(signInAsync()),
        signOutAsync: () => dispatch(signOutAsync()),
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(LoginScreen);