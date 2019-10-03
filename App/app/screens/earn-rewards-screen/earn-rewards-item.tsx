import React from "react";
import { Text, Image, StyleSheet, TouchableOpacity } from 'react-native'; 
import { palette } from '../../theme/palette';

export function Item({ item }) {
    const onPress = () => {
        alert('You get ' + item.points + ' points')
    }

    return (
    <TouchableOpacity style={styles.item} onPress={onPress}>
        <Image style={styles.image} source={item.image}/>
        <Text style={styles.title}>{item.title}</Text>
        <Text style={styles.points}>{item.points} pts</Text>
    </TouchableOpacity>
    );
  }

const styles = StyleSheet.create({
    item: {
        flex: 1,
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'flex-start',
        backgroundColor: '#dbdbdb',
        borderRadius: 10,
        shadowColor: '#000',
        shadowOpacity: 0.5,
        shadowOffset: { width: 0, height: 2 },
        shadowRadius: 2.62,
        elevation: 3,
        marginVertical: 5,
        marginHorizontal: 10,
        overflow: 'hidden'
    },
    image : {
        width: '100%',
    },
    title: {
        color: palette.black,
        fontSize: 14,
        padding: 5
    },
    points: {
        color: '#ffc700',
        fontSize: 14,
        padding: 5
    },
  });