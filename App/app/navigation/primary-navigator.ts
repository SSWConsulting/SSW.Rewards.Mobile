import { LeaderboardScreen, LeaderboardNavigationOptions } from "../screens/leaderboard-screen"
import { EarnRewardsScreen, EarnRewardsNavigationOptions  } from "../screens/earn-rewards-screen"
import { DevProfilesScreen, DevProfilesNavigationOptions } from "../screens/dev-profiles-screen"
import { createBottomTabNavigator, createStackNavigator } from "react-navigation"
import { palette } from '../theme/palette';
import { QrScannerScreen } from "../screens/qr-scanner-screen";

export const EarnRewardsNavigator = createStackNavigator(
  {
    earnRewards: EarnRewardsScreen,
    qrscanner: QrScannerScreen
  },
  {
    headerMode: 'none'
  }
)

export const PrimaryNavigator = createBottomTabNavigator(
  {
    devProfiles: { screen: DevProfilesScreen, navigationOptions: DevProfilesNavigationOptions },
    earnRewardStack: { screen: EarnRewardsNavigator, navigationOptions: EarnRewardsNavigationOptions },
    leaderboard: { screen: LeaderboardScreen, navigationOptions: LeaderboardNavigationOptions },
  },
  {
    tabBarOptions: {
        activeTintColor: palette.red,
        inactiveTintColor: palette.lightGrey,
        style: {
          backgroundColor: palette.black
        }
    }
  }
)



