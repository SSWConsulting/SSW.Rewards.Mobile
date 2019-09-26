import React from 'react';
import { Container, Text } from "native-base";
import { StyleSheet } from 'react-native';

export default function EarnRewardsScreen() {
    const render = () => {
        return (
            <Container style={styles.Container}>
                <Text>This is the Earn Rewards screen</Text>
            </Container>
        );
    }

    return render();
}

const styles = StyleSheet.create({
    Container: {
        alignItems: 'center',
        justifyContent: 'center'
    }
});