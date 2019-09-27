import { createStackNavigator } from "react-navigation-stack";
import { createBottomTabNavigator } from 'react-navigation-tabs';
import { createAppContainer, createSwitchNavigator } from 'react-navigation';
import LeaderboardScreen from './components/leaderboard/LeaderboardScreen';
import EarnRewardsScreen from './components/rewards/EarnRewardsScreen';
import DevProfilesScreen from "./components/dev-profiles/DevProfilesScreen";
import AuthLoadingScreen from './components/login/AuthLoadingScreen';
import LoginScreen from "./components/login/LoginScreen";
import { getLeaderboardIcon, getEarnRewardsIcon, getDevProfilesIcon } from "./components/Icon";

// const defaultHeaderObject = {
//     header: props => 
// };

// const createDefaultStackNavigator = (screens, options) =>
//     createStackNavigator(screens, {
//         defaultNavigationOptions: {

//         }
//     });

const BottomTabs = createBottomTabNavigator(
    {
        Leaderboard: {
            screen: LeaderboardScreen,
            navigationOptions: {
                tabBarIcon: ({ tintColor }) => getLeaderboardIcon(tintColor)
            }
        },
        EarnRewards: {
            screen: EarnRewardsScreen,
            navigationOptions: {
                tabBarIcon: ({ tintColor }) => getEarnRewardsIcon(tintColor)
            }
        },
        DevProfiles: {
            screen: DevProfilesScreen,
            navigationOptions: {
                tabBarIcon: ({ tintColor }) => getDevProfilesIcon(tintColor)
            }
        }
    }
);

// 1. AsyncLoading Screen
// 2. Onboarding Screen
// 3. Login Screen
const AuthStack = createStackNavigator(
    {
        // Onboarding: { screen: OnboardingScreen }
        LoginScreen: { screen: LoginScreen }
    }
);

const AppStack = createStackNavigator(
    { BottomTabs: { screen: BottomTabs } }
);

export const RootStack = createAppContainer(
    createSwitchNavigator({
        AuthLoadingScreen: { screen: AuthLoadingScreen },
        AuthStack: { screen: AuthStack },
        AppStack: { screen: AppStack }
    })
);