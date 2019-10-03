import * as React from "react"
import { observer } from "mobx-react"
import { ViewStyle, View, Text, TextStyle, ScrollView } from 'react-native';
// import { useStores } from "../../models/root-store"
import { NavigationScreenProps } from "react-navigation"
import { Header } from "../../components/header"
import { Item } from './earn-rewards-item'
import { Icon, Button } from "native-base"
import { Screen } from "../../components/screen"
import { palette } from "../../theme/palette";
import { EarnRewardsData as items } from './earn-rewards-data';

export interface EarnRewardsScreenProps extends NavigationScreenProps<{}> {
}

export const EarnRewardsNavigationOptions = {
  title: "Earn Rewards",
  tabBarIcon: ({ tintColor }) => (
    <Icon name="qr-scanner" style={{fontSize:25, color: tintColor}} />
  )
}

const ROOT: ViewStyle = {
}
const BODY: ViewStyle = {
  flex: 1,
  // justifyContent: 'flex-start',
  paddingBottom: 0,
}
const MAIN: ViewStyle = {
  flex: 1,
  padding: 10
}
const ITEM_ROW: ViewStyle = {
  flexDirection: 'row',
  justifyContent: 'space-around'
}
const BOTTOM_CONTAINER: ViewStyle = {
  alignItems: 'center',
  backgroundColor: "rgba(30, 30, 31, 0.75)",
  padding: 10,
  marginBottom: 'auto'
}
const BUTTON: ViewStyle = {
  borderRadius: 35,
  height: 70,
  width: 70,
  // top: -45,
  justifyContent: 'center',
  backgroundColor: palette.angry,
}
const QR_TEXT: TextStyle = {
  fontSize: 18,
  color: palette.white,
  margin: 0,
  padding: 0,
}

export const EarnRewardsScreen: React.FunctionComponent<EarnRewardsScreenProps> = observer((props) => {
  const openDrawer = () => props.navigation.toggleDrawer();
  // const { someStore } = useStores()
  const goToQRScanner = () => props.navigation.navigate('qrscanner');
  return (
    <Screen style={ROOT} preset="fixed">
      <Header
        headerText="Earn Points"
        drawerPress={openDrawer}
      />
      <ScrollView style={BODY}>
        <View style={MAIN}>
          <View style={ITEM_ROW}>
            <Item item={items[0]}  />
            <Item item={items[1]} />
          </View>
          <View style={ITEM_ROW}>
            <Item item={items[2]}  />
            <Item item={items[3]} />
          </View>
        </View>
      </ScrollView>
        <View style={BOTTOM_CONTAINER}>
          <Button style={BUTTON} onPress={goToQRScanner}><Icon name="camera" fontSize={45} /></Button>
          <Text style={QR_TEXT}>Scan QR Codes</Text>
        </View>
    </Screen>
  )
})
