import { createSwitchNavigator, createDrawerNavigator } from "react-navigation"
import { PrimaryNavigator } from "./primary-navigator"
import { SplashScreen } from "../screens/splash-screen"
import { AuthNavigator } from "./auth-navigator"
import { UserProfileScreen } from "../screens/user-profile-screen"
import { palette } from '../theme/palette';

const DrawerStack = createDrawerNavigator(
  {
    home: { screen: PrimaryNavigator, navigationOptions: { title: "Home" } },
    userProfile: { screen: UserProfileScreen, navigationOptions: { title: "User Profile" } }
  },
  {
    // TODO: Doesn't get updated when reloading app
    drawerBackgroundColor: palette.darkGrey,
    contentOptions: {
      inactiveTintColor: palette.lightGrey
    }
  }
)

export const RootNavigator = createSwitchNavigator(
  {
    splash: { screen: SplashScreen },
    authNavigator: { screen: AuthNavigator },
    primaryNavigator: { screen: DrawerStack },
  },
  {
    initialRouteName: 'splash'
  }
)
 