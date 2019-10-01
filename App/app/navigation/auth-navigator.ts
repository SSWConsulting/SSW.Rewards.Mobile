import { createSwitchNavigator } from "react-navigation"
import { OnboardingScreen } from "../screens/onboarding-screen"
import { LoginScreen } from "../screens/login-screen"

export const AuthNavigator = createSwitchNavigator(
  {
    onboarding: { screen: OnboardingScreen },
    login: { screen: LoginScreen }
  },
)


