import * as React from "react"
import { observer } from "mobx-react"
// import { useStores } from "../../models/root-store"
import { NavigationScreenProps } from "react-navigation"
import AppIntroSlider from 'react-native-app-intro-slider';
import { onboardingSlides } from "./onboarding-screens"
import { View, ViewStyle } from "react-native";

export interface OnboardingScreenProps extends NavigationScreenProps<{}> {
}

const FULL: ViewStyle = { flex: 1 }

export const OnboardingScreen: React.FunctionComponent<OnboardingScreenProps> = observer((props) => {
  // const { someStore } = useStores()

  const onOnboardingDone = async () => {
    props.navigation.navigate('login');
  };

  return (
    <View style={ FULL }>
      <AppIntroSlider slides={onboardingSlides} onDone={onOnboardingDone} />
    </View>
  )
})
