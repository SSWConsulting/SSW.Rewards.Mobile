import * as React from "react"
import { observer } from "mobx-react"
import { Dimensions } from "react-native"
import { Screen } from "../../components/screen"
// import { useStores } from "../../models/root-store"
import { NavigationScreenProps } from "react-navigation"
import { Header } from "../../components/header"
import { Icon } from 'native-base';
import Carousel from 'react-native-snap-carousel';
import { DeveloperProfiles } from "./developer-profiles"
import SliderEntry from "./slider-entry"

export interface DevProfilesScreenProps extends NavigationScreenProps<{}> {
}

export const DevProfilesNavigationOptions = {
  title: "Developer Profiles",
  tabBarIcon: ({ tintColor }) => (
    <Icon name="contact"  style={{fontSize: 25, color: tintColor}} />
  )
}

const { width: viewportWidth } = Dimensions.get('window');

export const DevProfilesScreen: React.FunctionComponent<DevProfilesScreenProps> = observer((props) => {
  const openDrawer = () => {
    props.navigation.toggleDrawer();
  }
  // const { someStore } = useStores()

  return (
    <Screen preset="fixed">
      <Header
        headerText="Developer Profiles"
        drawerPress={openDrawer}
      />
      <Carousel data={DeveloperProfiles}
        loop={true}
        renderItem={({item, index}) => <SliderEntry data={item} />}
        sliderWidth={viewportWidth}
        itemWidth={viewportWidth}
        hasParallaxImages={true}/>
    </Screen>
  )
})
