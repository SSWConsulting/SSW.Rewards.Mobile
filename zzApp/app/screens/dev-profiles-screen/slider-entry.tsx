import React, { Component } from 'react';
import { View, Text, TouchableOpacity, StyleSheet, Platform, Dimensions } from 'react-native';
import PropTypes from 'prop-types';
import { palette } from '../../theme/palette';
import { Wallpaper } from '../../components/wallpaper';

const IS_IOS = Platform.OS === 'ios';
const { height: viewportHeight } = Dimensions.get('window');

const styles = StyleSheet.create({
    slideContainer: {
        flex: 1,
    },
    skillContainer: {
        position: 'absolute',
        right: 10,
        flex: 1,
        flexDirection: 'column',
        alignContent:'flex-end',
        marginTop: 10,
        marginBottom: IS_IOS ? 0 : -1, // Prevent a random Android rendering issue
    },
    skillBox: {
        height: 75,
        width: 75,
        borderWidth: 2,
        borderColor: palette.black,
        marginBottom: 20,
    },
    textContainer: {
        marginTop: Math.round(viewportHeight * 0.65),
        backgroundColor: "rgb(30, 30, 31)",
        opacity: 0.75,
        padding: 20,
        paddingTop: 20,
        paddingBottom: 50,
    },
    title: {
        color: palette.lighterGrey,
        fontSize: 26,
        fontWeight: 'bold',
        letterSpacing: 0.5
    },
    subtitle: {
        marginTop: 6,
        color: palette.lightGrey,
        fontSize: 12,
        fontStyle: 'italic'
    }
});

export default class SliderEntry extends Component {

    static propTypes = {
        data: PropTypes.object.isRequired,
        parallax: PropTypes.bool,
        parallaxProps: PropTypes.object
    };

    renderSkillLogo(skill: string) {
        //TODO: render on the RHS of the screen
        return (
            <Text key={skill} style={styles.skillBox}>{skill}</Text>
        );
    }

    render () {
        const { data: { name, title, avatar, skills, profile } } = this.props;
        
        return (
            <TouchableOpacity style={styles.slideContainer} activeOpacity={1}
              onPress={() => { alert(`You've clicked '${name}'`); }}>
                <Wallpaper backgroundImage={ avatar } style={{resizeMode: 'contain', marginLeft: -60}} />
                <View style={[styles.skillContainer]}>
                    {skills.map(this.renderSkillLogo)}
                </View>
                <View style={[styles.textContainer]}>
                    <Text style={[styles.title]}>
                        { name.toUpperCase() }
                    </Text>
                    <Text style={[styles.subtitle]}>
                        { title }
                    </Text>
                </View>
            </TouchableOpacity>
        );
    }
}