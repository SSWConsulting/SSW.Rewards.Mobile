import * as React from "react"
import { observer } from "mobx-react"
import { ViewStyle, View } from 'react-native';
// import { useStores } from "../../models/root-store"
import { color } from "../../theme"
import { NavigationScreenProps } from "react-navigation"
import { Header } from "../../components/header"
import { Icon, Button } from "native-base"
import { Screen } from "../../components/screen"

export interface EarnRewardsScreenProps extends NavigationScreenProps<{}> {
}

export const EarnRewardsNavigationOptions = {
  title: "Earn Rewards",
  tabBarIcon: ({ tintColor }) => (
    <Icon name="qr-scanner" style={{fontSize:25, color: tintColor}} />
  )
}

const ROOT: ViewStyle = {
  backgroundColor: color.palette.black
}
const BODY: ViewStyle = {
  alignItems: 'center',
  justifyContent: 'center',
  height: '100%'
}

export const EarnRewardsScreen: React.FunctionComponent<EarnRewardsScreenProps> = observer((props) => {
  const openDrawer = () => props.navigation.toggleDrawer();
  // const { someStore } = useStores()
  const goToQRScanner = () => props.navigation.navigate('qrscanner');
  return (
    <Screen style={ROOT} preset="fixed">
      <Header
        headerText="Earn Rewards"
        drawerPress={openDrawer}
      />
      <View style={BODY}>
        <Button onPress={goToQRScanner}><Icon name="camera" /></Button>
      </View>
    </Screen>
  )
})
