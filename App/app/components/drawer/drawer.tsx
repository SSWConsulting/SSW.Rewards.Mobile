import * as React from "react"
import { View, Button } from "react-native"
import { DrawerItems, SafeAreaView } from "react-navigation"

/**
 * Stateless functional component for your needs
 *
 * Component description here for TypeScript tips.
 */
export function Drawer(props) {

  const logout = () => {
    props.navigation.navigate('splash')
  }

  return (
    <View style={{flex:1}}>
        <SafeAreaView forceInset={{ top: 'always', horizontal: 'never' }}>
            <DrawerItems {...props} />
            <Button title="Logout" onPress={logout}/>
        </SafeAreaView>
    </View>
  )
}
