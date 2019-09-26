import React, { useState } from "react";
import { Header, Left, Button, Icon, Right, Title, Body } from "native-base";

export default function HeaderComponent() {
    const [title, setTitle] = useState('Leaderboard')

    this.render = () => {
        return (
            <Header>
                <Left>
                    <Button transparent>
                        <Icon name="menu" />
                    </Button>
                </Left>
                <Body>
                    <Title>{title}</Title>
                </Body>
            </Header>
        )
    }
    return title === 'Login' ? '' : this.render();
}
