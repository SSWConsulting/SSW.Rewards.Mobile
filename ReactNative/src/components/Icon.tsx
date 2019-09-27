import React from 'react';
import { Icon } from "native-base";

const getIcon = (name, color) => (<Icon name={name} fontSize={25} color={color} />);

export const getLeaderboardIcon = (color) => getIcon("star", color);
export const getEarnRewardsIcon = (color) => getIcon("gift", color)
export const getDevProfilesIcon = (color) => getIcon("contact", color)
