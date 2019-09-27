import React from 'react';
import { Text, Container } from 'native-base';
import { StyleSheet } from 'react-native';

export default function LeaderboardScreen() {
    const render = () => {
        return (
            <Container style={styles.Container}>
                <Text>This is the Leaderboard screen</Text>
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