import React from 'react';
import { Container, Content, Icon, Item as FormItem, Input, Label, Text, Button, View } from 'native-base'

export default class LoginScreen extends React.Component {
    render() {
        return (
            <Container>
                <Content>
                    <View padder>
                        <FormItem
                            stackedLabel
                        >
                            <Label>Email</Label>
                            <Input 
                                value="test"
                            />
                        </FormItem>
                    </View>
                    <View padder>
                        <FormItem
                            stackedLabel
                        >
                            <Label>Password</Label>
                            <Input 
                                value="test"
                            />
                        </FormItem>
                    </View>
                    <View padder>
                        <Button
                            block
                            rounded
                            success
                        >
                            <Icon name="ios-mail" color="white" fontSize={30} />
                            <Text>Login</Text>
                        </Button>
                    </View>
                </Content>
            </Container>
        );
    }
}
