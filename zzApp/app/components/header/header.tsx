import * as React from "react"
import { View, ViewStyle, TextStyle } from "react-native"
import { HeaderProps } from "./header.props"
import { Button } from "../button"
import { Icon } from "native-base"
import { Icon as CustomIcon } from "../icon"
import { Text } from "../text"
import { spacing } from "../../theme"
import { translate } from "../../i18n/"
import { color } from '../../theme/color';

// static styles
const ROOT: ViewStyle = {
  backgroundColor: color.headerBackground,
  flexDirection: "row",
  paddingHorizontal: spacing[4],
  alignItems: "center",
  paddingTop: spacing[2],
  paddingBottom: spacing[2],
  justifyContent: "flex-start",
}
const TITLE: TextStyle = { textAlign: "center", fontSize: 18 }
const TITLE_MIDDLE: ViewStyle = { flex: 1, justifyContent: "center" }
const LEFT: ViewStyle = { width: 32 }
const RIGHT: ViewStyle = { width: 32 }
const MENU_BUTTON: ViewStyle = { backgroundColor: 'transparent', marginLeft: 10 }
const MENU_ICON: TextStyle = { fontSize: 24, color: color.palette.white }

/**
 * Header that appears on many screens. Will hold navigation buttons and screen title.
 */
export const Header: React.FunctionComponent<HeaderProps> = props => {
  const {
    showHamburger,
    onLeftPress,
    onRightPress,
    rightIcon,
    leftIcon,
    headerText,
    headerTx,
    style,
    titleStyle,
    drawerPress,
  } = props
  const header = headerText || (headerTx && translate(headerTx)) || ""

  const renderHamburger = () => {
    if(!!!showHamburger){
      return (
        <Button onPress={drawerPress} style={MENU_BUTTON}>
          <Icon name={'menu'} style={MENU_ICON} />
        </Button>
      );
    }
    return null;
  }

  return (
    <View style={{ ...ROOT, ...style, borderWidth: 1, borderColor: '#000' }}>
      {renderHamburger()}
      {leftIcon ? (
        <Button preset="link" onPress={onLeftPress}>
          <CustomIcon icon={leftIcon} />
        </Button>
      ) : (
        <View style={LEFT} />
      )}
      <View style={TITLE_MIDDLE}>
        <Text style={{ ...TITLE, ...titleStyle }} text={header} />
      </View>
      {rightIcon ? (
        <Button preset="link" onPress={onRightPress}>
          <CustomIcon icon={rightIcon} />
        </Button>
      ) : (
        <View style={RIGHT} />
      )}
    </View>
  )
}
