import * as React from "react"
import { observer } from "mobx-react"
import { ViewStyle, View } from "react-native"
import { Screen } from "../../components/screen"
// import { useStores } from "../../models/root-store"
import { color } from "../../theme"
import { NavigationScreenProps, FlatList } from "react-navigation"
import { Header } from "../../components/header";
import { LeaderboardData } from './leaderboard-data';
import { Item } from "./leaderboard-item"
import { Icon } from "native-base"

export interface LeaderboardScreenProps extends NavigationScreenProps<{}> {
}

const ROOT: ViewStyle = {
  backgroundColor: color.background
}
const BODY: ViewStyle = {
  height: '100%',
  width: '100%',
  paddingTop: 5
}

export const LeaderboardNavigationOptions = {
  title: "Leaderboard",
  tabBarIcon: ({ tintColor }) => (
    <Icon name="star" style={{fontSize: 25, color: tintColor}} />
  )
}

export const LeaderboardScreen: React.FunctionComponent<LeaderboardScreenProps> = observer((props) => {
  // const { someStore } = useStores()

  const leaderboardData = LeaderboardData;
  const openDrawer = () => {
    props.navigation.toggleDrawer();
  }
  
  return (
    <Screen style={ROOT} preset="fixed">
      <Header
        headerText="SSW Leaderboard"
        drawerPress={openDrawer}
      />
      <View style={BODY}>
        <FlatList
          data={leaderboardData}
          renderItem={({ item }) => <Item item={item} />}
          keyExtractor={item => item.position.toString()}
        />
      </View>
    </Screen>
  )
})
