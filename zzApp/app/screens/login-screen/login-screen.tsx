import * as React from "react"
import { observer } from "mobx-react"
import { ViewStyle, View } from "react-native"
import { Text } from "../../components/text"
import { Screen } from "../../components/screen"
// import { useStores } from "../../models/root-store"
import { color } from "../../theme"
import { NavigationScreenProps } from "react-navigation"
import { Button } from "../../components/button"
import Auth from 'appcenter-auth'
import AsyncStorage from "@react-native-community/async-storage"
import base64 from 'react-native-base64'


export interface LoginScreenProps extends NavigationScreenProps<{}> {
}

const FULL: ViewStyle = { flex: 1 }
const ROOT: ViewStyle = {
  backgroundColor: color.palette.angry,
  justifyContent: 'center', 
  alignItems: 'center',
}
const BOTTOM: ViewStyle = {
  width: '100%', 
  height: 50, 
  justifyContent: 'center', 
  alignItems: 'center',
  position: 'absolute',
  bottom: 100
}

export const LoginScreen: React.FunctionComponent<LoginScreenProps> = observer((props) => {
  // const { someStore } = useStores()

  const doLogin = async () => {
    let token = await AsyncStorage.getItem('token');
    if (token) {
      props.navigation.navigate('primaryNavigator');
    } else {
      const userInformation = await Auth.signIn();
      const parsedToken = userInformation.idToken.split('.');
      const payload = base64.decode(parsedToken[1]);
      AsyncStorage.setItem('token', parsedToken[1]);
      console.tron.log(payload);
    }
  };

  return (
    <View style={FULL}>
    <Screen style={ROOT} preset="fixed">
      <Text>SSW Logo</Text>
      <View style={BOTTOM}>
        <Button onPress={ doLogin }>
          <Text>Login</Text>
        </Button>
      </View>
    </Screen>
    </View>
  )
})
