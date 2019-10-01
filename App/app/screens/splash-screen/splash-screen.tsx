import * as React from "react"
import { observer } from "mobx-react"
import { ViewStyle } from "react-native"
import { Screen } from "../../components/screen"
// import { useStores } from "../../models/root-store"
import { color } from "../../theme"
import { NavigationScreenProps } from "react-navigation"
import { Spinner } from "../../components/spinner"

export interface SplashScreenProps extends NavigationScreenProps<{}> {
}

const ROOT: ViewStyle = {
  backgroundColor: color.palette.black,
  justifyContent: 'center'
}

export const SplashScreen: React.FunctionComponent<SplashScreenProps> = observer((props) => {
  // const { someStore } = useStores()

  React.useEffect(() => {
    console.tron.log("Calling timeout effect");
    setTimeout(() => {
      console.tron.log("timeout triggered");
      props.navigation.navigate('authNavigator');
    }, 1000);
  }, []);

  return (
    <Screen style={ROOT}>
      <Spinner />
    </Screen>
  )
})
