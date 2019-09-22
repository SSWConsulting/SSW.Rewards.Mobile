import React from 'react';
import { Header, Left, Button, Icon, Right, Title, Body } from 'native-base';

export default class LoginHeader extends React.Component {
    render() {
        return (
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
        );
    }
}