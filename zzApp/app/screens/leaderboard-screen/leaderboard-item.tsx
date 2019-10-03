import React from "react";
import { View, Text, Image, StyleSheet } from "react-native"; 
import { color } from "../../theme";
import { palette } from '../../theme/palette';

export function Item({ item }) {
    return (
    <View style={[styles.item, item.selected ? styles.highlighted: null]}>
        <Text style={styles.rank}>{item.position}</Text>
        <Image style={styles.avatar} source={{uri: item.picture}}/>
        <Text style={styles.name}>{item.name}</Text>
        <Text style={styles.points}>{item.points}pts</Text>
    </View>
    );
  }

const styles = StyleSheet.create({
    item: {
        flex: 1,
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'flex-start',
        backgroundColor: color.item,
        borderRadius: 2,
        shadowColor: '#000',
        shadowOpacity: 0.5,
        shadowOffset: { width: 0, height: 2 },
        shadowRadius: 2.62,
        elevation: 3,
        padding: 12,
        marginVertical: 5,
        marginHorizontal: 10,
    },
    highlighted: {
        backgroundColor: palette.white,
        borderLeftWidth: 3,
        borderLeftColor: color.primary
    },
    rank: {
        color: color.font,
        fontSize: 18,
        padding: 10
    },
    avatar : {
        height: 40,
        width: 40,
        borderRadius: 20,
        borderWidth: 1,
        borderColor: '#979797',
        padding: 5,
        marginHorizontal: 10
    },
    name: {
        color: color.font,
        fontSize: 14,
        padding: 5
    },
    points: {
        color: color.font,
        fontSize: 14,
        marginLeft: 'auto',
        padding: 5
    },
  });